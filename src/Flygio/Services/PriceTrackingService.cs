using Flygio.Data;
using Flygio.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Services;

public class PriceTrackingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PriceTrackingService> _logger;

    public PriceTrackingService(IServiceProvider serviceProvider, ILogger<PriceTrackingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 30s startup delay
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("PriceTrackingService: Starting price fetch cycle");

            try
            {
                await FetchPricesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PriceTrackingService: Unhandled error in fetch cycle");
            }

            // Wait 6 hours
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task FetchPricesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FlygioDbContext>>();
        var flightSearch = scope.ServiceProvider.GetRequiredService<IFlightSearchService>();

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var routes = await db.FlightRoutes
            .Where(r => r.IsPopular || r.PriceAlerts.Any(a => a.IsActive && a.IsConfirmed))
            .ToListAsync(ct);

        _logger.LogInformation("PriceTrackingService: Fetching prices for {Count} routes", routes.Count);

        foreach (var route in routes)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                var prices = await flightSearch.GetLatestPricesAsync(route.OriginIata, route.DestinationIata);

                foreach (var price in prices.Take(10))
                {
                    db.PricePoints.Add(new PricePoint
                    {
                        FlightRouteId = route.Id,
                        Price = price.Price,
                        Airline = price.Airline,
                        DepartureDate = price.DepartureDate,
                        ReturnDate = price.ReturnDate,
                        NumberOfChanges = price.NumberOfChanges,
                        FoundAt = DateTime.UtcNow
                    });
                }

                await db.SaveChangesAsync(ct);
                _logger.LogInformation("PriceTrackingService: Saved {Count} prices for {Origin}-{Dest}",
                    prices.Count, route.OriginIata, route.DestinationIata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PriceTrackingService: Failed to fetch prices for {Origin}-{Dest}",
                    route.OriginIata, route.DestinationIata);
            }
        }
    }
}
