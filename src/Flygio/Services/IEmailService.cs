namespace Flygio.Services;

public interface IEmailService
{
    Task SendPriceAlertAsync(string toEmail, string routeDescription, int oldPrice, int newPrice, string affiliateUrl);
    Task SendConfirmationEmailAsync(string toEmail, string confirmationUrl, string routeDescription);
}
