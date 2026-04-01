namespace Flygio.Services;

public class StripeSettings
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; set; } = "";
    public string PublishableKey { get; set; } = "";
    public string WebhookSecret { get; set; } = "";
    public string PriceId { get; set; } = "";
    public int MonthlyPriceSek { get; set; } = 79;
}
