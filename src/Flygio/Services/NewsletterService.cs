using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Flygio.Services;

public class NewsletterService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<ResendSettings> resendSettings,
    ILogger<NewsletterService> logger)
{
    private readonly ResendSettings _resend = resendSettings.Value;

    public async Task<(int sent, int failed)> SendWeeklyNewsletterAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_resend.ApiKey))
        {
            logger.LogWarning("Resend API key not configured, skipping newsletter send");
            return (0, 0);
        }

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();

        var subscribers = await db.NewsletterSubscribers
            .Where(s => s.IsActive)
            .ToListAsync(ct);

        if (subscribers.Count == 0)
        {
            logger.LogInformation("No active newsletter subscribers");
            return (0, 0);
        }

        var html = await BuildNewsletterHtmlAsync(db, ct);
        if (html is null)
        {
            logger.LogInformation("No deals found for newsletter this week");
            return (0, 0);
        }

        var subject = $"Veckans bästa flygpriser - {DateTime.UtcNow:d MMMM yyyy}";
        var sent = 0;
        var failed = 0;

        foreach (var subscriber in subscribers)
        {
            if (ct.IsCancellationRequested) break;

            var personalizedHtml = html.Replace("UNSUBSCRIBE_URL_PLACEHOLDER",
                $"https://flygio.se/nyhetsbrev/avsluta?token={subscriber.UnsubscribeToken}");

            var success = await SendEmailAsync(subscriber.Email, subject, personalizedHtml, ct);
            if (success)
            {
                subscriber.LastSentAt = DateTime.UtcNow;
                sent++;
            }
            else
            {
                failed++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Newsletter sent to {Sent} subscribers, {Failed} failures", sent, failed);
        return (sent, failed);
    }

    private async Task<string?> BuildNewsletterHtmlAsync(FlygioDbContext db, CancellationToken ct)
    {
        var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

        // Top 5 cheapest routes this week
        var cheapestRoutes = await db.PricePoints
            .Where(p => p.ScrapedAt >= oneWeekAgo)
            .GroupBy(p => new { p.FlightRoute.OriginCode, p.FlightRoute.DestinationCode })
            .Select(g => new
            {
                g.Key.OriginCode,
                g.Key.DestinationCode,
                MinPrice = g.Min(p => p.Price),
                Currency = g.OrderBy(p => p.Price).Select(p => p.Currency).First(),
                DepartureDate = g.OrderBy(p => p.Price).Select(p => p.DepartureDate).First()
            })
            .OrderBy(r => r.MinPrice)
            .Take(5)
            .ToListAsync(ct);

        if (cheapestRoutes.Count == 0) return null;

        // Price drops (routes where current price < avg last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var priceDrops = await db.PricePoints
            .Where(p => p.ScrapedAt >= thirtyDaysAgo)
            .GroupBy(p => new { p.FlightRoute.OriginCode, p.FlightRoute.DestinationCode })
            .Select(g => new
            {
                g.Key.OriginCode,
                g.Key.DestinationCode,
                CurrentMin = g.Where(p => p.ScrapedAt >= oneWeekAgo).Min(p => (decimal?)p.Price),
                AvgPrice = g.Where(p => p.ScrapedAt < oneWeekAgo).Average(p => (decimal?)p.Price),
                Currency = g.OrderBy(p => p.Price).Select(p => p.Currency).First()
            })
            .Where(r => r.CurrentMin != null && r.AvgPrice != null && r.CurrentMin < r.AvgPrice * 0.85m)
            .OrderBy(r => r.CurrentMin / r.AvgPrice)
            .Take(3)
            .ToListAsync(ct);

        // New destinations (routes added this week)
        var newRoutes = await db.FlightRoutes
            .Where(r => r.PricePoints.Any(p => p.ScrapedAt >= oneWeekAgo)
                      && !r.PricePoints.Any(p => p.ScrapedAt < oneWeekAgo))
            .Select(r => new { r.OriginCode, r.DestinationCode })
            .Take(5)
            .ToListAsync(ct);

        var routeRows = new StringBuilder();
        foreach (var r in cheapestRoutes)
        {
            var bookingUrl = $"https://flygio.se/go/aviasales?origin={r.OriginCode}&dest={r.DestinationCode}&dep={r.DepartureDate:yyyy-MM-dd}";
            routeRows.Append($"""
                <tr>
                    <td style="padding: 12px 16px; border-bottom: 1px solid #e5e7eb;">{r.OriginCode} &rarr; {r.DestinationCode}</td>
                    <td style="padding: 12px 16px; border-bottom: 1px solid #e5e7eb; font-weight: bold; color: #16a34a;">{r.MinPrice:N0} {r.Currency}</td>
                    <td style="padding: 12px 16px; border-bottom: 1px solid #e5e7eb;">{r.DepartureDate:d MMM}</td>
                    <td style="padding: 12px 16px; border-bottom: 1px solid #e5e7eb;"><a href="{bookingUrl}" style="color: #0ea5e9; text-decoration: none; font-weight: 600;">Boka &rarr;</a></td>
                </tr>
                """);
        }

        var priceDropSection = "";
        if (priceDrops.Count > 0)
        {
            var dropItems = new StringBuilder();
            foreach (var d in priceDrops)
            {
                var pctDrop = (1 - d.CurrentMin!.Value / d.AvgPrice!.Value) * 100;
                dropItems.Append($"""
                    <li style="margin-bottom: 8px;">{d.OriginCode} &rarr; {d.DestinationCode}: <strong>{d.CurrentMin:N0} {d.Currency}</strong> (ned {pctDrop:N0}% mot genomsnitt)</li>
                    """);
            }
            priceDropSection = $"""
                <div style="margin-bottom: 24px;">
                    <h2 style="font-size: 18px; color: #1e293b; margin: 0 0 12px 0;">Prisfall denna vecka</h2>
                    <ul style="margin: 0; padding-left: 20px; color: #374151;">{dropItems}</ul>
                </div>
                """;
        }

        var newDestSection = "";
        if (newRoutes.Count > 0)
        {
            var destItems = new StringBuilder();
            foreach (var n in newRoutes)
                destItems.Append($"<li style=\"margin-bottom: 4px;\">{n.OriginCode} &rarr; {n.DestinationCode}</li>");

            newDestSection = $"""
                <div style="margin-bottom: 24px;">
                    <h2 style="font-size: 18px; color: #1e293b; margin: 0 0 12px 0;">Nya destinationer</h2>
                    <ul style="margin: 0; padding-left: 20px; color: #374151;">{destItems}</ul>
                </div>
                """;
        }

        return $"""
            <!DOCTYPE html>
            <html lang="sv">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"></head>
            <body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333; background: #f8fafc;">
                <div style="background: linear-gradient(135deg, #0ea5e9, #6366f1); padding: 30px; border-radius: 12px; color: white; text-align: center; margin-bottom: 24px;">
                    <h1 style="margin: 0 0 8px 0; font-size: 24px;">&#9992; Flygio Veckobrev</h1>
                    <p style="margin: 0; font-size: 16px; opacity: 0.9;">Veckans bästa flygpriser från Sverige</p>
                </div>

                <div style="background: white; border-radius: 12px; padding: 24px; margin-bottom: 24px; box-shadow: 0 1px 3px rgba(0,0,0,0.1);">
                    <h2 style="font-size: 18px; color: #1e293b; margin: 0 0 16px 0;">Topp 5 billigaste flygningar</h2>
                    <table style="width: 100%; border-collapse: collapse; font-size: 14px;">
                        <thead>
                            <tr style="background: #f1f5f9;">
                                <th style="padding: 10px 16px; text-align: left; font-weight: 600; color: #64748b;">Rutt</th>
                                <th style="padding: 10px 16px; text-align: left; font-weight: 600; color: #64748b;">Pris</th>
                                <th style="padding: 10px 16px; text-align: left; font-weight: 600; color: #64748b;">Datum</th>
                                <th style="padding: 10px 16px; text-align: left; font-weight: 600; color: #64748b;"></th>
                            </tr>
                        </thead>
                        <tbody>
                            {routeRows}
                        </tbody>
                    </table>
                </div>

                {priceDropSection}
                {newDestSection}

                <div style="text-align: center; margin-bottom: 24px;">
                    <a href="https://flygio.se" style="display: inline-block; background: #0ea5e9; color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: bold; font-size: 16px;">Sök fler flygpriser</a>
                </div>

                <hr style="border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;">

                <p style="font-size: 12px; color: #9ca3af; text-align: center;">
                    Du får detta mejl för att du prenumererar på Flygios nyhetsbrev.<br>
                    <a href="UNSUBSCRIBE_URL_PLACEHOLDER" style="color: #9ca3af;">Avsluta prenumeration</a>
                </p>
            </body>
            </html>
            """;
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string html, CancellationToken ct)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _resend.ApiKey);

            var payload = new
            {
                from = $"{_resend.FromName} <{_resend.FromEmail}>",
                to = new[] { toEmail },
                subject,
                html
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.resend.com/emails", content, ct);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Newsletter email sent to {Email}", toEmail);
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Resend API error {Status}: {Body}", (int)response.StatusCode, errorBody);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send newsletter to {Email}", toEmail);
            return false;
        }
    }
}
