using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Flygio.Services;

public class StripeService(
    IServiceScopeFactory scopeFactory,
    IOptions<StripeSettings> stripeSettings,
    ILogger<StripeService> logger)
{
    private readonly StripeSettings _settings = stripeSettings.Value;

    public async Task<string> CreateCheckoutSessionAsync(int userId, string userEmail, string successUrl, string cancelUrl)
    {
        StripeConfiguration.ApiKey = _settings.SecretKey;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
        var user = await db.Users.FindAsync(userId);

        string? customerId = user?.StripeCustomerId;

        if (customerId is null && user is not null)
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = userEmail,
                Metadata = new Dictionary<string, string> { { "flygio_user_id", userId.ToString() } }
            });
            customerId = customer.Id;
            user.StripeCustomerId = customerId;
            await db.SaveChangesAsync();
        }

        var options = new SessionCreateOptions
        {
            Customer = customerId,
            Mode = "subscription",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = _settings.PriceId,
                    Quantity = 1
                }
            ],
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string> { { "flygio_user_id", userId.ToString() } }
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(options);
        return session.Url;
    }

    public async Task<string> CreateCustomerPortalSessionAsync(int userId, string returnUrl)
    {
        StripeConfiguration.ApiKey = _settings.SecretKey;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
        var user = await db.Users.FindAsync(userId);

        if (user?.StripeCustomerId is null)
            throw new InvalidOperationException("User has no Stripe customer ID");

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = user.StripeCustomerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task HandleWebhookEventAsync(string json, string signature)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _settings.WebhookSecret);

        switch (stripeEvent.Type)
        {
            case EventTypes.CustomerSubscriptionCreated:
            case EventTypes.CustomerSubscriptionUpdated:
            case EventTypes.CustomerSubscriptionDeleted:
                await HandleSubscriptionEventAsync(stripeEvent);
                break;
            case EventTypes.InvoicePaymentFailed:
                await HandlePaymentFailedAsync(stripeEvent);
                break;
        }
    }

    private async Task HandleSubscriptionEventAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription is null) return;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();

        var user = await db.Users.FirstOrDefaultAsync(u => u.StripeCustomerId == subscription.CustomerId);
        if (user is null)
        {
            logger.LogWarning("No user found for Stripe customer {CustomerId}", subscription.CustomerId);
            return;
        }

        user.StripeSubscriptionId = subscription.Id;
        user.SubscriptionStatus = subscription.Status;
        user.SubscriptionCurrentPeriodEnd = subscription.Items?.Data?.FirstOrDefault()?.CurrentPeriodEnd;
        user.IsPremium = subscription.Status is "active" or "trialing";

        await db.SaveChangesAsync();
        logger.LogInformation("Subscription {Status} for user {UserId} (customer {CustomerId})",
            subscription.Status, user.Id, subscription.CustomerId);
    }

    private async Task HandlePaymentFailedAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice is null) return;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();

        var user = await db.Users.FirstOrDefaultAsync(u => u.StripeCustomerId == invoice.CustomerId);
        if (user is null) return;

        user.SubscriptionStatus = "past_due";
        user.IsPremium = false;
        await db.SaveChangesAsync();

        logger.LogWarning("Payment failed for user {UserId} (customer {CustomerId})", user.Id, invoice.CustomerId);
    }
}
