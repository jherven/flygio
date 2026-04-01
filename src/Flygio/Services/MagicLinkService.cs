using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Flygio.Services;

public class MagicLinkService(
    FlygioDbContext db,
    IHttpClientFactory httpClientFactory,
    IOptions<ResendSettings> resendSettings,
    ILogger<MagicLinkService> logger)
{
    private readonly ResendSettings _resend = resendSettings.Value;

    public async Task<bool> SendMagicLinkAsync(string email)
    {
        var link = new MagicLink { Email = email.Trim().ToLowerInvariant() };
        db.MagicLinks.Add(link);
        await db.SaveChangesAsync();

        return await SendLoginEmailAsync(link);
    }

    public async Task<AppUser?> ValidateTokenAsync(string token)
    {
        var link = await db.MagicLinks
            .FirstOrDefaultAsync(m => m.Token == token && !m.IsUsed && m.ExpiresAt > DateTime.UtcNow);

        if (link is null) return null;

        link.IsUsed = true;

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == link.Email);
        if (user is null)
        {
            user = new AppUser { Email = link.Email };
            db.Users.Add(user);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return user;
    }

    private async Task<bool> SendLoginEmailAsync(MagicLink link)
    {
        if (string.IsNullOrEmpty(_resend.ApiKey))
        {
            logger.LogWarning("Resend API key not configured, cannot send magic link");
            return false;
        }

        try
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _resend.ApiKey);

            var loginUrl = $"https://flygio.se/auth/verify?token={link.Token}";

            var payload = new
            {
                from = $"{_resend.FromName} <{_resend.FromEmail}>",
                to = new[] { link.Email },
                subject = "Logga in på Flygio",
                html = BuildLoginEmailHtml(loginUrl)
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api.resend.com/emails", content);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Magic link sent to {Email}", link.Email);
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            logger.LogWarning("Resend API error {Status}: {Body}", (int)response.StatusCode, errorBody);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send magic link to {Email}", link.Email);
            return false;
        }
    }

    private static string BuildLoginEmailHtml(string loginUrl)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="sv">
            <head><meta charset="utf-8"></head>
            <body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333;">
                <div style="background: linear-gradient(135deg, #0ea5e9, #6366f1); padding: 30px; border-radius: 12px; color: white; text-align: center; margin-bottom: 20px;">
                    <h1 style="margin: 0 0 8px 0; font-size: 24px;">Logga in på Flygio</h1>
                    <p style="margin: 0; font-size: 16px; opacity: 0.9;">Klicka på knappen nedan för att logga in</p>
                </div>

                <div style="text-align: center; margin: 30px 0;">
                    <a href="{loginUrl}" style="display: inline-block; background: #0ea5e9; color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: bold; font-size: 16px;">Logga in</a>
                </div>

                <p style="font-size: 14px; color: #666; text-align: center;">
                    Länken är giltig i 15 minuter. Om du inte begärde detta mejl kan du ignorera det.
                </p>

                <hr style="border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;">

                <p style="font-size: 12px; color: #9ca3af; text-align: center;">
                    <a href="https://flygio.se" style="color: #6366f1;">Flygio.se</a> &mdash; Hitta billiga flyg från Sverige
                </p>
            </body>
            </html>
            """;
    }
}
