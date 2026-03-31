using System.Globalization;
using Flygio.Components;
using Flygio.Data;
using Flygio.Data.Models;
using Flygio.Services;
using Microsoft.EntityFrameworkCore;

// Set Swedish culture as default for date/number formatting
var svCulture = new CultureInfo("sv-SE");
CultureInfo.DefaultThreadCurrentCulture = svCulture;
CultureInfo.DefaultThreadCurrentUICulture = svCulture;

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
    await DestinationSeeder.SeedAsync(db);

    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
else
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
    await ArticleSeeder.SeedAsync(db);
    await DestinationSeeder.SeedAsync(db);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapHealthChecks("/healthz");
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Price history API endpoint
app.MapGet("/api/price-history/{airportCode}", async (
    string airportCode,
    int? days,
    FlygioDbContext db) =>
{
    var lookback = days switch
    {
        60 => 60,
        90 => 90,
        _ => 30
    };
    var since = DateTime.UtcNow.AddDays(-lookback);

    var points = await db.PricePoints
        .Where(p => p.FlightRoute.DestinationCode == airportCode
                    && p.ScrapedAt >= since)
        .GroupBy(p => p.ScrapedAt.Date)
        .OrderBy(g => g.Key)
        .Select(g => new
        {
            Date = g.Key.ToString("yyyy-MM-dd"),
            MinPrice = g.Min(p => p.Price),
            AvgPrice = Math.Round(g.Average(p => p.Price), 0),
            MaxPrice = g.Max(p => p.Price),
            Count = g.Count()
        })
        .ToListAsync();

    return Results.Json(points);
});

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
