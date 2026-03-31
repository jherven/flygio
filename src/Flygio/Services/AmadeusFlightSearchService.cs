using System.Net.Http.Headers;
using System.Text.Json;
using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Flygio.Services;

public class AmadeusFlightSearchService(
    HttpClient httpClient,
    AmadeusTokenService tokenService,
    IOptions<AmadeusSettings> settings,
    IMemoryCache cache,
    IServiceScopeFactory scopeFactory,
    ILogger<AmadeusFlightSearchService> logger)
{
    private readonly AmadeusSettings _settings = settings.Value;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public async Task<FlightSearchResult> SearchAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate = null,
        int adults = 1,
        int maxResults = 10,
        CancellationToken ct = default)
    {
        var cacheKey = $"flight:{originCode}:{destinationCode}:{departureDate:yyyy-MM-dd}:{returnDate:yyyy-MM-dd}:{adults}";

        if (cache.TryGetValue(cacheKey, out FlightSearchResult? cached))
        {
            logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached!;
        }

        var token = await tokenService.GetAccessTokenAsync(ct);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = $"{_settings.BaseUrl}/v2/shopping/flight-offers" +
                  $"?originLocationCode={Uri.EscapeDataString(originCode)}" +
                  $"&destinationLocationCode={Uri.EscapeDataString(destinationCode)}" +
                  $"&departureDate={departureDate:yyyy-MM-dd}" +
                  $"&adults={adults}" +
                  $"&max={maxResults}" +
                  $"&currencyCode=SEK";

        if (returnDate.HasValue)
            url += $"&returnDate={returnDate.Value:yyyy-MM-dd}";

        logger.LogInformation("Amadeus search: {Origin}->{Dest} on {Date}", originCode, destinationCode, departureDate);

        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        var result = MapResponse(originCode, destinationCode, json);

        cache.Set(cacheKey, result, CacheDuration);

        // Persist price points in the background
        _ = Task.Run(() => PersistPricePointsAsync(result), CancellationToken.None);

        return result;
    }

    private static FlightSearchResult MapResponse(string originCode, string destinationCode, JsonElement json)
    {
        var offers = new List<FlightOffer>();

        if (json.TryGetProperty("data", out var data))
        {
            var dictionaries = json.TryGetProperty("dictionaries", out var dict) ? dict : default;

            foreach (var offer in data.EnumerateArray())
            {
                var price = offer.GetProperty("price");
                var priceTotal = decimal.Parse(price.GetProperty("grandTotal").GetString()!);
                var currency = price.GetProperty("currency").GetString()!;

                var itineraries = offer.GetProperty("itineraries");
                var outbound = itineraries[0];
                var segments = outbound.GetProperty("segments");
                var firstSegment = segments[0];
                var lastSegment = segments[segments.GetArrayLength() - 1];

                var airlineCode = firstSegment.GetProperty("carrierCode").GetString()!;
                var airline = airlineCode;
                if (dictionaries.ValueKind == JsonValueKind.Object &&
                    dictionaries.TryGetProperty("carriers", out var carriers) &&
                    carriers.TryGetProperty(airlineCode, out var airlineName))
                {
                    airline = airlineName.GetString()!;
                }

                var departureStr = firstSegment.GetProperty("departure").GetProperty("at").GetString()!;
                var departure = DateTime.Parse(departureStr);

                DateTime? returnDate = null;
                if (itineraries.GetArrayLength() > 1)
                {
                    var inbound = itineraries[1];
                    var returnSegments = inbound.GetProperty("segments");
                    var returnFirst = returnSegments[0];
                    returnDate = DateTime.Parse(returnFirst.GetProperty("departure").GetProperty("at").GetString()!);
                }

                var stops = segments.GetArrayLength() - 1;
                var duration = outbound.GetProperty("duration").GetString()!;

                offers.Add(new FlightOffer
                {
                    Provider = "Amadeus",
                    Price = priceTotal,
                    Currency = currency,
                    DepartureDate = departure,
                    ReturnDate = returnDate,
                    Airline = airline,
                    Stops = stops,
                    Duration = duration
                });
            }
        }

        return new FlightSearchResult
        {
            Origin = originCode,
            Destination = destinationCode,
            OriginCode = originCode,
            DestinationCode = destinationCode,
            Offers = offers
        };
    }

    private async Task PersistPricePointsAsync(FlightSearchResult result)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();

            var route = await db.FlightRoutes
                .FirstOrDefaultAsync(r => r.OriginCode == result.OriginCode && r.DestinationCode == result.DestinationCode);

            if (route is null)
            {
                route = new FlightRoute
                {
                    Origin = result.OriginCode,
                    Destination = result.DestinationCode,
                    OriginCode = result.OriginCode,
                    DestinationCode = result.DestinationCode,
                    Airline = result.Offers.FirstOrDefault()?.Airline
                };
                db.FlightRoutes.Add(route);
                await db.SaveChangesAsync();
            }

            var pricePoints = result.Offers.Select(o => new PricePoint
            {
                FlightRouteId = route.Id,
                Provider = o.Provider,
                Price = o.Price,
                Currency = o.Currency,
                DepartureDate = o.DepartureDate,
                ReturnDate = o.ReturnDate,
                ScrapedAt = DateTime.UtcNow
            });

            db.PricePoints.AddRange(pricePoints);
            await db.SaveChangesAsync();

            logger.LogInformation("Persisted {Count} price points for {Origin}->{Dest}",
                result.Offers.Count, result.OriginCode, result.DestinationCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist price points for {Origin}->{Dest}",
                result.OriginCode, result.DestinationCode);
        }
    }
}
