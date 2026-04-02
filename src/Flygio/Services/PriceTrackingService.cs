using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Flygio.Services;

public class PriceTrackingSettings
{
    public const string SectionName = "PriceTracking";

    public int IntervalHours { get; set; } = 6;
    public int DaysAhead { get; set; } = 30;
    public int MaxCallsPerRun { get; set; } = 20;
}

public class PriceTrackingService(
    IServiceScopeFactory scopeFactory,
    IOptions<PriceTrackingSettings> settings,
    IOptions<TravelpayoutsSettings> tpSettings,
    IHttpClientFactory httpClientFactory,
    ILogger<PriceTrackingService> logger) : BackgroundService
{
    private readonly PriceTrackingSettings _settings = settings.Value;
    private readonly string _tpToken = tpSettings.Value.ApiToken;

    private static readonly (string OriginCode, string Origin, string DestCode, string Dest)[] PopularRoutes =
    [
        ("ARN", "Stockholm Arlanda", "BCN", "Barcelona"),
        ("ARN", "Stockholm Arlanda", "BKK", "Bangkok"),
        ("ARN", "Stockholm Arlanda", "LHR", "London Heathrow"),
        ("ARN", "Stockholm Arlanda", "AGP", "Málaga"),
        ("ARN", "Stockholm Arlanda", "ATH", "Aten"),
        ("ARN", "Stockholm Arlanda", "ALC", "Alicante"),
        ("ARN", "Stockholm Arlanda", "PMI", "Palma de Mallorca"),
        ("GOT", "Göteborg Landvetter", "LHR", "London Heathrow"),
        ("GOT", "Göteborg Landvetter", "BCN", "Barcelona"),
        ("GOT", "Göteborg Landvetter", "AGP", "Málaga"),
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PriceTrackingService started (Travelpayouts). Interval: {Hours}h",
            _settings.IntervalHours);

        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TrackPricesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Price tracking run failed");
            }

            await Task.Delay(TimeSpan.FromHours(_settings.IntervalHours), stoppingToken);
        }
    }

    private async Task TrackPricesAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_tpToken))
        {
            logger.LogWarning("Travelpayouts API token not configured, skipping price tracking");
            return;
        }

        logger.LogInformation("Starting price tracking run for {Count} routes via Travelpayouts", PopularRoutes.Length);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();

        await EnsureRoutesExistAsync(db);

        var httpClient = httpClientFactory.CreateClient();
        var callsMade = 0;

        foreach (var route in PopularRoutes)
        {
            if (ct.IsCancellationRequested) break;
            if (callsMade >= _settings.MaxCallsPerRun) break;

            try
            {
                // Travelpayouts /v1/prices/cheap — returns cheapest tickets for a route
                var url = $"https://api.travelpayouts.com/v1/prices/cheap" +
                          $"?origin={route.OriginCode}" +
                          $"&destination={route.DestCode}" +
                          $"&currency=SEK" +
                          $"&token={_tpToken}";

                var response = await httpClient.GetAsync(url, ct);

                if ((int)response.StatusCode == 429)
                {
                    logger.LogWarning("Travelpayouts rate limit hit, pausing for 30s");
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);
                    continue;
                }

                response.EnsureSuccessStatusCode();
                callsMade++;

                var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
                await PersistTravelpayoutsPricesAsync(db, route.OriginCode, route.DestCode, json);

                logger.LogInformation("Tracked {Origin}->{Dest}: prices persisted via Travelpayouts",
                    route.OriginCode, route.DestCode);

                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Failed to fetch prices for {Origin}->{Dest}", route.OriginCode, route.DestCode);
            }
        }

        logger.LogInformation("Price tracking run complete. {Calls} API calls made", callsMade);
    }

    private static async Task EnsureRoutesExistAsync(FlygioDbContext db)
    {
        foreach (var route in PopularRoutes)
        {
            var exists = await db.FlightRoutes
                .AnyAsync(r => r.OriginCode == route.OriginCode && r.DestinationCode == route.DestCode);

            if (!exists)
            {
                db.FlightRoutes.Add(new FlightRoute
                {
                    Origin = route.Origin,
                    Destination = route.Dest,
                    OriginCode = route.OriginCode,
                    DestinationCode = route.DestCode,
                    IsPopular = true
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task PersistTravelpayoutsPricesAsync(
        FlygioDbContext db,
        string originCode,
        string destCode,
        JsonElement json)
    {
        if (!json.TryGetProperty("success", out var success) || !success.GetBoolean())
            return;

        if (!json.TryGetProperty("data", out var data))
            return;

        if (!data.TryGetProperty(destCode, out var destData))
            return;

        var flightRoute = await db.FlightRoutes
            .FirstAsync(r => r.OriginCode == originCode && r.DestinationCode == destCode);

        // Travelpayouts returns data keyed by transfer count: "0" = direct, "1" = 1 stop, etc.
        foreach (var transferGroup in destData.EnumerateObject())
        {
            var ticket = transferGroup.Value;
            if (!ticket.TryGetProperty("price", out var priceEl)) continue;

            var price = priceEl.GetDecimal();

            DateTime? departureDate = null;
            if (ticket.TryGetProperty("departure_at", out var depEl))
                departureDate = DateTime.Parse(depEl.GetString()!);

            DateTime? returnDate = null;
            if (ticket.TryGetProperty("return_at", out var retEl))
            {
                var retStr = retEl.GetString();
                if (!string.IsNullOrEmpty(retStr))
                    returnDate = DateTime.Parse(retStr);
            }

            db.PricePoints.Add(new PricePoint
            {
                FlightRouteId = flightRoute.Id,
                Provider = "Travelpayouts",
                Price = price,
                Currency = "SEK",
                DepartureDate = departureDate ?? DateTime.UtcNow.Date.AddDays(14),
                ReturnDate = returnDate,
                ScrapedAt = DateTime.UtcNow
            });
        }

        flightRoute.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
