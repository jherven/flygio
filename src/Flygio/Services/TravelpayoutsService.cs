using System.Text.Json;
using Flygio.Configuration;
using Flygio.Services.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Flygio.Services;

public class TravelpayoutsService : IFlightSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TravelpayoutsService> _logger;
    private readonly TravelpayoutsOptions _options;
    private readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TravelpayoutsService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<TravelpayoutsOptions> options,
        ILogger<TravelpayoutsService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Access-Token", _options.ApiToken);
    }

    public async Task<List<FlightOffer>> GetLatestPricesAsync(string origin, string destination)
    {
        var cacheKey = $"latest_{origin}_{destination}";
        if (_cache.TryGetValue(cacheKey, out List<FlightOffer>? cached) && cached is not null)
            return cached;

        try
        {
            var json = await RateLimitedGetAsync($"prices/latest?origin={origin}&destination={destination}&currency=sek&limit=30");
            if (json is null) return [];

            var result = new List<FlightOffer>();
            if (json.RootElement.TryGetProperty("data", out var data))
            {
                foreach (var item in data.EnumerateArray())
                {
                    result.Add(ParseFlightOffer(item, origin, destination));
                }
            }

            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest prices for {Origin}-{Destination}", origin, destination);
            return [];
        }
    }

    public async Task<List<FlightOffer>> GetCheapestAsync(string origin, string destination)
    {
        var cacheKey = $"cheap_{origin}_{destination}";
        if (_cache.TryGetValue(cacheKey, out List<FlightOffer>? cached) && cached is not null)
            return cached;

        try
        {
            var json = await RateLimitedGetAsync($"prices/cheap?origin={origin}&destination={destination}&currency=sek");
            if (json is null) return [];

            var result = new List<FlightOffer>();
            if (json.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty(destination, out var destData))
            {
                foreach (var prop in destData.EnumerateObject())
                {
                    var item = prop.Value;
                    result.Add(new FlightOffer
                    {
                        Origin = origin,
                        Destination = destination,
                        Price = item.TryGetProperty("price", out var p) ? p.GetInt32() : 0,
                        Airline = item.TryGetProperty("airline", out var a) ? a.GetString() : null,
                        DepartureDate = item.TryGetProperty("departure_at", out var d) ? DateTime.Parse(d.GetString()!) : DateTime.MinValue,
                        ReturnDate = item.TryGetProperty("return_at", out var r) ? DateTime.Parse(r.GetString()!) : null,
                        NumberOfChanges = item.TryGetProperty("transfers", out var t) ? t.GetInt32() : 0,
                        FoundAt = item.TryGetProperty("expires_at", out var f) ? DateTime.Parse(f.GetString()!) : DateTime.UtcNow
                    });
                }
            }

            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cheapest prices for {Origin}-{Destination}", origin, destination);
            return [];
        }
    }

    public async Task<List<CalendarPrice>> GetMonthMatrixAsync(string origin, string destination)
    {
        var cacheKey = $"matrix_{origin}_{destination}";
        if (_cache.TryGetValue(cacheKey, out List<CalendarPrice>? cached) && cached is not null)
            return cached;

        try
        {
            var json = await RateLimitedGetAsync($"prices/month-matrix?origin={origin}&destination={destination}&currency=sek");
            if (json is null) return [];

            var result = new List<CalendarPrice>();
            if (json.RootElement.TryGetProperty("data", out var data))
            {
                foreach (var item in data.EnumerateArray())
                {
                    result.Add(new CalendarPrice
                    {
                        Date = item.TryGetProperty("depart_date", out var d) ? DateTime.Parse(d.GetString()!) : DateTime.MinValue,
                        Price = item.TryGetProperty("value", out var p) ? p.GetInt32() : 0,
                        Airline = item.TryGetProperty("airline", out var a) ? a.GetString() : null,
                        NumberOfChanges = item.TryGetProperty("transfers", out var t) ? t.GetInt32() : 0
                    });
                }
            }

            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get month matrix for {Origin}-{Destination}", origin, destination);
            return [];
        }
    }

    public async Task<List<CalendarPrice>> GetCalendarPricesAsync(string origin, string destination, int month, int year)
    {
        var cacheKey = $"cal_{origin}_{destination}_{year}_{month}";
        if (_cache.TryGetValue(cacheKey, out List<CalendarPrice>? cached) && cached is not null)
            return cached;

        try
        {
            var departDate = new DateTime(year, month, 1).ToString("yyyy-MM-dd");
            var json = await RateLimitedGetAsync($"prices/calendar?origin={origin}&destination={destination}&currency=sek&depart_date={departDate}");
            if (json is null) return [];

            var result = new List<CalendarPrice>();
            if (json.RootElement.TryGetProperty("data", out var data))
            {
                foreach (var prop in data.EnumerateObject())
                {
                    var item = prop.Value;
                    result.Add(new CalendarPrice
                    {
                        Date = DateTime.Parse(prop.Name),
                        Price = item.TryGetProperty("price", out var p) ? p.GetInt32() : 0,
                        Airline = item.TryGetProperty("airline", out var a) ? a.GetString() : null,
                        NumberOfChanges = item.TryGetProperty("transfers", out var t) ? t.GetInt32() : 0
                    });
                }
            }

            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get calendar prices for {Origin}-{Destination} {Year}-{Month}", origin, destination, year, month);
            return [];
        }
    }

    public async Task<List<PopularRoute>> GetPopularRoutesAsync(string origin = "ARN")
    {
        var cacheKey = $"popular_{origin}";
        if (_cache.TryGetValue(cacheKey, out List<PopularRoute>? cached) && cached is not null)
            return cached;

        try
        {
            var json = await RateLimitedGetAsync($"city-directions?origin={origin}&currency=sek");
            if (json is null) return [];

            var result = new List<PopularRoute>();
            if (json.RootElement.TryGetProperty("data", out var data))
            {
                foreach (var prop in data.EnumerateObject())
                {
                    var item = prop.Value;
                    var dest = prop.Name;
                    result.Add(new PopularRoute
                    {
                        Origin = origin,
                        Destination = dest,
                        OriginCity = IataData.GetCityName(origin),
                        DestinationCity = IataData.GetCityName(dest),
                        CheapestPrice = item.TryGetProperty("price", out var p) ? p.GetInt32() : null,
                        Popularity = item.TryGetProperty("popularity", out var pop) ? pop.GetInt32() : 0
                    });
                }
            }

            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get popular routes for {Origin}", origin);
            return [];
        }
    }

    private async Task<JsonDocument?> RateLimitedGetAsync(string url)
    {
        await _rateLimiter.WaitAsync();
        try
        {
            var elapsed = DateTime.UtcNow - _lastRequestTime;
            if (elapsed.TotalMilliseconds < 1100)
            {
                await Task.Delay(1100 - (int)elapsed.TotalMilliseconds);
            }

            var response = await _httpClient.GetAsync(url);
            _lastRequestTime = DateTime.UtcNow;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Travelpayouts API returned {StatusCode} for {Url}", response.StatusCode, url);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(content);
        }
        finally
        {
            _rateLimiter.Release();
        }
    }

    private static FlightOffer ParseFlightOffer(JsonElement item, string origin, string destination)
    {
        return new FlightOffer
        {
            Origin = origin,
            Destination = destination,
            Price = item.TryGetProperty("value", out var v) ? v.GetInt32() :
                    item.TryGetProperty("price", out var p) ? p.GetInt32() : 0,
            Airline = item.TryGetProperty("gate", out var g) ? g.GetString() :
                      item.TryGetProperty("airline", out var a) ? a.GetString() : null,
            DepartureDate = item.TryGetProperty("depart_date", out var d) ? DateTime.Parse(d.GetString()!) :
                            item.TryGetProperty("departure_at", out var d2) ? DateTime.Parse(d2.GetString()!) : DateTime.MinValue,
            ReturnDate = item.TryGetProperty("return_date", out var r) ? DateTime.Parse(r.GetString()!) :
                         item.TryGetProperty("return_at", out var r2) ? DateTime.Parse(r2.GetString()!) : null,
            NumberOfChanges = item.TryGetProperty("number_of_changes", out var n) ? n.GetInt32() :
                              item.TryGetProperty("transfers", out var t) ? t.GetInt32() : 0,
            FoundAt = item.TryGetProperty("found_at", out var f) ? DateTime.Parse(f.GetString()!) : DateTime.UtcNow
        };
    }
}
