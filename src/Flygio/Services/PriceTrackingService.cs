using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
    ILogger<PriceTrackingService> logger) : BackgroundService
{
    private readonly PriceTrackingSettings _settings = settings.Value;

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
        logger.LogInformation("PriceTrackingService started. Interval: {Hours}h, DaysAhead: {Days}, MaxCalls: {Max}",
            _settings.IntervalHours, _settings.DaysAhead, _settings.MaxCallsPerRun);

        // Wait a bit on startup to let the app fully initialize
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
        logger.LogInformation("Starting price tracking run for {Count} routes", PopularRoutes.Length);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<AmadeusTokenService>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        await EnsureRoutesExistAsync(db);

        var departureDate = DateTime.UtcNow.Date.AddDays(14);
        var returnDate = departureDate.AddDays(7);
        var callsMade = 0;

        foreach (var route in PopularRoutes)
        {
            if (ct.IsCancellationRequested) break;
            if (callsMade >= _settings.MaxCallsPerRun)
            {
                logger.LogWarning("Reached max API calls per run ({Max}), stopping early", _settings.MaxCallsPerRun);
                break;
            }

            try
            {
                var token = await tokenService.GetAccessTokenAsync(ct);
                var httpClient = httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var url = $"{scope.ServiceProvider.GetRequiredService<IOptions<AmadeusSettings>>().Value.BaseUrl}" +
                          $"/v2/shopping/flight-offers" +
                          $"?originLocationCode={route.OriginCode}" +
                          $"&destinationLocationCode={route.DestCode}" +
                          $"&departureDate={departureDate:yyyy-MM-dd}" +
                          $"&returnDate={returnDate:yyyy-MM-dd}" +
                          $"&adults=1&max=5&currencyCode=SEK";

                var response = await httpClient.GetAsync(url, ct);

                if ((int)response.StatusCode == 429)
                {
                    logger.LogWarning("Amadeus rate limit hit, pausing for 60s");
                    await Task.Delay(TimeSpan.FromSeconds(60), ct);
                    continue;
                }

                response.EnsureSuccessStatusCode();
                callsMade++;

                var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(ct);
                await PersistPricesAsync(db, route.OriginCode, route.DestCode, departureDate, returnDate, json);

                logger.LogInformation("Tracked {Origin}->{Dest}: prices persisted", route.OriginCode, route.DestCode);

                // Small delay between calls to be respectful of rate limits
                await Task.Delay(TimeSpan.FromSeconds(2), ct);
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

    private static async Task PersistPricesAsync(
        FlygioDbContext db,
        string originCode,
        string destCode,
        DateTime departureDate,
        DateTime? returnDate,
        System.Text.Json.JsonElement json)
    {
        var flightRoute = await db.FlightRoutes
            .FirstAsync(r => r.OriginCode == originCode && r.DestinationCode == destCode);

        if (!json.TryGetProperty("data", out var data)) return;

        foreach (var offer in data.EnumerateArray())
        {
            var price = offer.GetProperty("price");
            var priceTotal = decimal.Parse(price.GetProperty("grandTotal").GetString()!);
            var currency = price.GetProperty("currency").GetString()!;

            db.PricePoints.Add(new PricePoint
            {
                FlightRouteId = flightRoute.Id,
                Provider = "Amadeus",
                Price = priceTotal,
                Currency = currency,
                DepartureDate = departureDate,
                ReturnDate = returnDate,
                ScrapedAt = DateTime.UtcNow
            });
        }

        flightRoute.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
