using Flygio.Components;
using Flygio.Data;
using Flygio.Data.Models;
using Flygio.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<FlygioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<FlygioDbContext>();

// Amadeus API
builder.Services.Configure<AmadeusSettings>(builder.Configuration.GetSection(AmadeusSettings.SectionName));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<AmadeusTokenService>();
builder.Services.AddSingleton<AmadeusTokenService>();
builder.Services.AddHttpClient<AmadeusFlightSearchService>();
builder.Services.AddScoped<AmadeusFlightSearchService>();

// Travelpayouts affiliate
builder.Services.Configure<TravelpayoutsSettings>(builder.Configuration.GetSection(TravelpayoutsSettings.SectionName));
builder.Services.AddSingleton<TravelpayoutsAffiliateLinkService>();

// Umami analytics
builder.Services.Configure<UmamiSettings>(builder.Configuration.GetSection(UmamiSettings.SectionName));

// Background price tracking
builder.Services.Configure<PriceTrackingSettings>(builder.Configuration.GetSection(PriceTrackingSettings.SectionName));
builder.Services.AddHostedService<PriceTrackingService>();

// Price alerts via email (Resend)
builder.Services.Configure<ResendSettings>(builder.Configuration.GetSection(ResendSettings.SectionName));
builder.Services.AddHostedService<PriceAlertService>();

var app = builder.Build();

// Auto-migrate and seed in production
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
    await db.Database.MigrateAsync();
    await ArticleSeeder.SeedAsync(db);

    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
else
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
    await ArticleSeeder.SeedAsync(db);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapHealthChecks("/healthz");
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Affiliate click redirect endpoint
app.MapGet("/go/{provider}", async (
    string provider,
    string origin,
    string dest,
    string dep,
    string? ret,
    HttpContext httpContext,
    TravelpayoutsAffiliateLinkService affiliateService,
    FlygioDbContext db) =>
{
    var departureDate = DateTime.Parse(dep);
    DateTime? returnDate = ret is not null ? DateTime.Parse(ret) : null;

    var subId = TravelpayoutsAffiliateLinkService.BuildSubId(origin, dest);
    var click = new AffiliateClick
    {
        Provider = provider,
        OriginCode = origin,
        DestinationCode = dest,
        SubId = subId,
        UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
        Referer = httpContext.Request.Headers.Referer.ToString(),
        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
    };
    db.AffiliateClicks.Add(click);
    await db.SaveChangesAsync();

    var affiliateUrl = affiliateService.ResolveAffiliateUrl(provider, origin, dest, departureDate, returnDate);
    return Results.Redirect(affiliateUrl);
});

app.Run();
