using System.Text;
using System.Text.Json;
using Flygio.Configuration;
using Microsoft.Extensions.Options;

namespace Flygio.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly ResendOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(HttpClient httpClient, IOptions<ResendOptions> options, ILogger<EmailService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://api.resend.com/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
    }

    public async Task SendPriceAlertAsync(string toEmail, string routeDescription, int oldPrice, int newPrice, string affiliateUrl)
    {
        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <div style="background: linear-gradient(135deg, #0ea5e9, #0284c7); padding: 30px; text-align: center; border-radius: 12px 12px 0 0;">
                    <h1 style="color: white; margin: 0; font-size: 24px;">Prisfall!</h1>
                </div>
                <div style="background: #f8fafc; padding: 30px; border-radius: 0 0 12px 12px;">
                    <p style="font-size: 16px; color: #374151;">Priset på din bevakade rutt har sjunkit!</p>
                    <div style="background: white; border-radius: 8px; padding: 20px; margin: 20px 0; text-align: center;">
                        <p style="font-size: 14px; color: #6b7280; margin: 0 0 8px 0;">{routeDescription}</p>
                        <p style="color: #ef4444; font-size: 16px; text-decoration: line-through; margin: 0;">{oldPrice:N0} SEK</p>
                        <p style="color: #16a34a; font-size: 32px; font-weight: bold; margin: 8px 0;">{newPrice:N0} SEK</p>
                    </div>
                    <div style="text-align: center; margin: 24px 0;">
                        <a href="{affiliateUrl}" style="background: #f97316; color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: bold; display: inline-block;" rel="nofollow sponsored">Se pris & boka</a>
                    </div>
                    <p style="font-size: 12px; color: #9ca3af; text-align: center;">
                        Detta mail skickades från flygio.se. Affiliate-länken ovan hjälper oss att driva tjänsten.
                    </p>
                </div>
            </div>
            """;

        await SendEmailAsync(toEmail, $"Prisfall: {routeDescription}", html);
    }

    public async Task SendConfirmationEmailAsync(string toEmail, string confirmationUrl, string routeDescription)
    {
        var html = $"""
            <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
                <div style="background: linear-gradient(135deg, #0ea5e9, #0284c7); padding: 30px; text-align: center; border-radius: 12px 12px 0 0;">
                    <h1 style="color: white; margin: 0; font-size: 24px;">Bekräfta prisbevakning</h1>
                </div>
                <div style="background: #f8fafc; padding: 30px; border-radius: 0 0 12px 12px;">
                    <p style="font-size: 16px; color: #374151;">Bekräfta din prisbevakning för:</p>
                    <p style="font-size: 18px; font-weight: bold; color: #0ea5e9; text-align: center;">{routeDescription}</p>
                    <div style="text-align: center; margin: 24px 0;">
                        <a href="{confirmationUrl}" style="background: #0ea5e9; color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: bold; display: inline-block;">Bekräfta bevakning</a>
                    </div>
                    <p style="font-size: 12px; color: #9ca3af; text-align: center;">
                        Om du inte skapat denna bevakning kan du ignorera detta mail.
                    </p>
                </div>
            </div>
            """;

        await SendEmailAsync(toEmail, $"Bekräfta prisbevakning: {routeDescription}", html);
    }

    private async Task SendEmailAsync(string to, string subject, string html)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("Resend API key not configured, skipping email to {To}", to);
            return;
        }

        var payload = new
        {
            from = $"{_options.FromName} <{_options.FromEmail}>",
            to = new[] { to },
            subject,
            html
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("emails", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send email via Resend: {StatusCode} {Error}", response.StatusCode, error);
        }
    }
}
