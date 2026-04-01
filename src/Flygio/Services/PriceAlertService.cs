using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Flygio.Services;

public class PriceAlertService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<ResendSettings> resendSettings,
    ILogger<PriceAlertService> logger) : BackgroundService
{
    private readonly ResendSettings _resend = resendSettings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PriceAlertService started");

        // Wait for price tracking to run first
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendAlertsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Price alert check failed");
            }

            // Run every 6 hours (after price tracking)
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task CheckAndSendAlertsAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_resend.ApiKey))
        {
            logger.LogWarning("Resend API key not configured, skipping alert check");
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();

        var activeAlerts = await db.PriceAlerts
            .Include(a => a.User)
            .Where(a => a.IsActive)
            .ToListAsync(ct);

        if (activeAlerts.Count == 0)
        {
            logger.LogInformation("No active price alerts to check");
            return;
        }

        logger.LogInformation("Checking {Count} active price alerts", activeAlerts.Count);
        var emailsSent = 0;

        // Pro users get notified first (30 min head start)
        var proAlerts = activeAlerts.Where(a => a.User?.IsPremium == true).ToList();
        var freeAlerts = activeAlerts.Where(a => a.User?.IsPremium != true).ToList();
        var orderedAlerts = proAlerts.Concat(freeAlerts);

        foreach (var alert in orderedAlerts)
        {
            if (ct.IsCancellationRequested) break;

            // Don't notify more than once per 24 hours
            if (alert.LastNotifiedAt.HasValue &&
                DateTime.UtcNow - alert.LastNotifiedAt.Value < TimeSpan.FromHours(24))
                continue;

            // Get the lowest recent price for this route (last 24 hours)
            var lowestPrice = await db.PricePoints
                .Where(p => p.FlightRoute.OriginCode == alert.OriginCode
                         && p.FlightRoute.DestinationCode == alert.DestinationCode
                         && p.ScrapedAt >= DateTime.UtcNow.AddHours(-24))
                .OrderBy(p => p.Price)
                .FirstOrDefaultAsync(ct);

            if (lowestPrice is null) continue;

            if (lowestPrice.Price <= alert.TargetPrice)
            {
                var sent = await SendAlertEmailAsync(alert, lowestPrice, ct);
                if (sent)
                {
                    alert.LastNotifiedAt = DateTime.UtcNow;
                    emailsSent++;
                }
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Price alert check complete. {Sent} emails sent", emailsSent);
    }

    private async Task<bool> SendAlertEmailAsync(PriceAlert alert, PricePoint price, CancellationToken ct)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _resend.ApiKey);

            var subject = $"Prisdropp! {alert.OriginCode} → {alert.DestinationCode} från {price.Price:N0} {price.Currency}";
            var html = BuildAlertEmailHtml(alert, price);

            var payload = new
            {
                from = $"{_resend.FromName} <{_resend.FromEmail}>",
                to = new[] { alert.Email },
                subject,
                html
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.resend.com/emails", content, ct);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Alert email sent to {Email} for {Origin}->{Dest} at {Price} {Currency}",
                    alert.Email, alert.OriginCode, alert.DestinationCode, price.Price, price.Currency);
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Resend API error {Status}: {Body}", (int)response.StatusCode, errorBody);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send alert email to {Email}", alert.Email);
            return false;
        }
    }

    private static string BuildAlertEmailHtml(PriceAlert alert, PricePoint price)
    {
        var bookingUrl = $"https://flygio.se/go/aviasales?origin={alert.OriginCode}&dest={alert.DestinationCode}&dep={price.DepartureDate:yyyy-MM-dd}";
        if (price.ReturnDate.HasValue)
            bookingUrl += $"&ret={price.ReturnDate.Value:yyyy-MM-dd}";

        var unsubscribeUrl = $"https://flygio.se/alerts/unsubscribe?token={alert.UnsubscribeToken}";

        return $"""
            <!DOCTYPE html>
            <html lang="sv">
            <head><meta charset="utf-8"></head>
            <body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333;">
                <div style="background: linear-gradient(135deg, #0ea5e9, #6366f1); padding: 30px; border-radius: 12px; color: white; text-align: center; margin-bottom: 20px;">
                    <h1 style="margin: 0 0 8px 0; font-size: 24px;">Prisvarning!</h1>
                    <p style="margin: 0; font-size: 16px; opacity: 0.9;">{alert.OriginCode} &rarr; {alert.DestinationCode}</p>
                </div>

                <div style="background: #f0fdf4; border: 2px solid #22c55e; border-radius: 12px; padding: 24px; text-align: center; margin-bottom: 20px;">
                    <p style="margin: 0 0 4px 0; font-size: 14px; color: #666;">Bästa priset just nu</p>
                    <p style="margin: 0; font-size: 36px; font-weight: bold; color: #16a34a;">{price.Price:N0} {price.Currency}</p>
                    <p style="margin: 8px 0 0 0; font-size: 14px; color: #666;">Ditt målpris: {alert.TargetPrice:N0} {alert.Currency}</p>
                </div>

                <div style="text-align: center; margin-bottom: 20px;">
                    <p style="margin: 0 0 4px 0; font-size: 14px; color: #666;">Avresa: {price.DepartureDate:d MMMM yyyy}</p>
                    {(price.ReturnDate.HasValue ? $"<p style=\"margin: 0; font-size: 14px; color: #666;\">Retur: {price.ReturnDate.Value:d MMMM yyyy}</p>" : "")}
                </div>

                <div style="text-align: center; margin-bottom: 30px;">
                    <a href="{bookingUrl}" style="display: inline-block; background: #0ea5e9; color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: bold; font-size: 16px;">Boka nu</a>
                </div>

                <hr style="border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;">

                <p style="font-size: 12px; color: #9ca3af; text-align: center;">
                    Du får detta mejl för att du prenumererar på prisvarningar på <a href="https://flygio.se" style="color: #6366f1;">Flygio.se</a>.<br>
                    <a href="{unsubscribeUrl}" style="color: #9ca3af;">Avsluta prenumeration</a>
                </p>
            </body>
            </html>
            """;
    }
}
