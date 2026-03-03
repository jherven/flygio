using Flygio.Configuration;
using Flygio.Data;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Services;

public class AlertService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AlertService> _logger;

    public AlertService(IServiceProvider serviceProvider, ILogger<AlertService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start 30 min after PriceTrackingService
        await Task.Delay(TimeSpan.FromMinutes(30) + TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("AlertService: Checking price alerts");

            try
            {
                await CheckAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AlertService: Unhandled error in alert check");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task CheckAlertsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FlygioDbContext>>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var affiliateService = scope.ServiceProvider.GetRequiredService<AffiliateService>();

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var alerts = await db.PriceAlerts
            .Include(a => a.FlightRoute)
            .Where(a => a.IsActive && a.IsConfirmed)
            .ToListAsync(ct);

        var emailsSent = 0;

        foreach (var alert in alerts)
        {
            if (emailsSent >= 50 || ct.IsCancellationRequested) break;

            try
            {
                var latestPrice = await db.PricePoints
                    .Where(p => p.FlightRouteId == alert.FlightRouteId)
                    .OrderByDescending(p => p.FoundAt)
                    .FirstOrDefaultAsync(ct);

                if (latestPrice is null) continue;

                var previousPrice = await db.PricePoints
                    .Where(p => p.FlightRouteId == alert.FlightRouteId && p.FoundAt < latestPrice.FoundAt)
                    .OrderByDescending(p => p.FoundAt)
                    .FirstOrDefaultAsync(ct);

                var shouldNotify = false;

                // Notify if price is under MaxPrice
                if (latestPrice.Price <= alert.MaxPrice)
                    shouldNotify = true;

                // Notify if price dropped >= 10%
                if (previousPrice is not null && latestPrice.Price <= previousPrice.Price * 0.9m)
                    shouldNotify = true;

                if (!shouldNotify) continue;

                var route = alert.FlightRoute;
                var routeDesc = $"{IataData.GetCityName(route.OriginIata)} → {IataData.GetCityName(route.DestinationIata)}";
                var affiliateUrl = affiliateService.GenerateGenericLink(route.OriginIata, route.DestinationIata);

                await emailService.SendPriceAlertAsync(
                    alert.Email,
                    routeDesc,
                    previousPrice?.Price ?? latestPrice.Price,
                    latestPrice.Price,
                    affiliateUrl);

                emailsSent++;
                _logger.LogInformation("AlertService: Sent price alert to {Email} for {Route}", alert.Email, routeDesc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AlertService: Failed to process alert {AlertId}", alert.Id);
            }
        }

        _logger.LogInformation("AlertService: Sent {Count} alert emails", emailsSent);
    }
}
