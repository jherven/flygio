using System.Globalization;
using System.Security.Claims;
using Flygio.Components;
using Flygio.Data;
using Flygio.Data.Models;
using Flygio.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

// Set Swedish culture as default for date/number formatting
var svCulture = new CultureInfo("sv-SE");
CultureInfo.DefaultThreadCurrentCulture = svCulture;
CultureInfo.DefaultThreadCurrentUICulture = svCulture;

var builder = WebApplication.CreateBuilder(args);

// Sentry error monitoring
builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["Sentry:Dsn"] ?? "";
    o.TracesSampleRate = 0.2;
    o.SendDefaultPii = false;
    o.MinimumEventLevel = LogLevel.Error;
    o.Environment = builder.Environment.EnvironmentName;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<FlygioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<FlygioDbContext>()
    .AddUrlGroup(new Uri("https://test.api.amadeus.com/v1/security/oauth2/token"), "amadeus-api", tags: ["external"]);

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.NoCache());
    options.AddPolicy("SitemapCache", builder => builder.Expire(TimeSpan.FromHours(1)));
    options.AddPolicy("ApiCache", builder => builder.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("StaticContent", builder => builder.Expire(TimeSpan.FromHours(24)));
});

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

// User accounts & auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/konto/logga-in";
        options.LogoutPath = "/konto/logga-ut";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "flygio_session";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<MagicLinkService>();
builder.Services.AddScoped<UserSessionService>();

var app = builder.Build();

app.UseSentryTracing();

// Auto-migrate and seed in production
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
    await db.Database.MigrateAsync();
    await ArticleSeeder.SeedAsync(db);
    await DestinationSeeder.SeedAsync(db);
    await RouteSeeder.SeedAsync(db);
    await SeoContentSeeder.SeedMonthlyPriceTrendArticlesAsync(db);
    await SeoContentSeeder.SeedTravelGuideArticlesAsync(db);

    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
else
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
    await ArticleSeeder.SeedAsync(db);
    await DestinationSeeder.SeedAsync(db);
    await RouteSeeder.SeedAsync(db);
    await SeoContentSeeder.SeedMonthlyPriceTrendArticlesAsync(db);
    await SeoContentSeeder.SeedTravelGuideArticlesAsync(db);
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseOutputCache();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public, max-age=604800, immutable";
    }
});

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/healthz/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("external")
});
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
}).CacheOutput("ApiCache");

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

// Hotel affiliate click redirect endpoint
app.MapGet("/go/hotellook", async (
    string city,
    string checkin,
    string checkout,
    int? adults,
    HttpContext httpContext,
    TravelpayoutsAffiliateLinkService affiliateService,
    FlygioDbContext db) =>
{
    var checkIn = DateTime.Parse(checkin);
    var checkOut = DateTime.Parse(checkout);
    var guestCount = adults ?? 2;

    var subId = TravelpayoutsAffiliateLinkService.BuildHotelSubId(city);
    var click = new AffiliateClick
    {
        Provider = "hotellook",
        OriginCode = "",
        DestinationCode = city,
        SubId = subId,
        UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
        Referer = httpContext.Request.Headers.Referer.ToString(),
        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
    };
    db.AffiliateClicks.Add(click);
    await db.SaveChangesAsync();

    var affiliateUrl = affiliateService.GenerateHotellookLink(city, checkIn, checkOut, guestCount);
    return Results.Redirect(affiliateUrl);
});

// Car rental affiliate click redirect endpoint
app.MapGet("/go/rentalcars", async (
    string city,
    string pickup,
    string dropoff,
    HttpContext httpContext,
    TravelpayoutsAffiliateLinkService affiliateService,
    FlygioDbContext db) =>
{
    var pickUp = DateTime.Parse(pickup);
    var dropOff = DateTime.Parse(dropoff);

    var subId = TravelpayoutsAffiliateLinkService.BuildCarSubId(city);
    var click = new AffiliateClick
    {
        Provider = "rentalcars",
        OriginCode = "",
        DestinationCode = city,
        SubId = subId,
        UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
        Referer = httpContext.Request.Headers.Referer.ToString(),
        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
    };
    db.AffiliateClicks.Add(click);
    await db.SaveChangesAsync();

    var affiliateUrl = affiliateService.GenerateRentalcarsLink(city, pickUp, dropOff);
    return Results.Redirect(affiliateUrl);
});

app.MapGet("/go/economybookings", async (
    string city,
    string pickup,
    string dropoff,
    HttpContext httpContext,
    TravelpayoutsAffiliateLinkService affiliateService,
    FlygioDbContext db) =>
{
    var pickUp = DateTime.Parse(pickup);
    var dropOff = DateTime.Parse(dropoff);

    var subId = TravelpayoutsAffiliateLinkService.BuildCarSubId(city);
    var click = new AffiliateClick
    {
        Provider = "economybookings",
        OriginCode = "",
        DestinationCode = city,
        SubId = subId,
        UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
        Referer = httpContext.Request.Headers.Referer.ToString(),
        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
    };
    db.AffiliateClicks.Add(click);
    await db.SaveChangesAsync();

    var affiliateUrl = affiliateService.GenerateEconomybookingsLink(city, pickUp, dropOff);
    return Results.Redirect(affiliateUrl);
});

// Activity affiliate click redirect endpoint
app.MapGet("/go/getyourguide", async (
    string city,
    HttpContext httpContext,
    TravelpayoutsAffiliateLinkService affiliateService,
    FlygioDbContext db) =>
{
    var subId = TravelpayoutsAffiliateLinkService.BuildActivitySubId(city);
    var click = new AffiliateClick
    {
        Provider = "getyourguide",
        OriginCode = "",
        DestinationCode = city,
        SubId = subId,
        UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
        Referer = httpContext.Request.Headers.Referer.ToString(),
        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
    };
    db.AffiliateClicks.Add(click);
    await db.SaveChangesAsync();

    var affiliateUrl = affiliateService.GenerateGetYourGuideLink(city);
    return Results.Redirect(affiliateUrl);
});

app.MapGet("/go/viator", async (
    string city,
    HttpContext httpContext,
    TravelpayoutsAffiliateLinkService affiliateService,
    FlygioDbContext db) =>
{
    var subId = TravelpayoutsAffiliateLinkService.BuildActivitySubId(city);
    var click = new AffiliateClick
    {
        Provider = "viator",
        OriginCode = "",
        DestinationCode = city,
        SubId = subId,
        UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
        Referer = httpContext.Request.Headers.Referer.ToString(),
        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
    };
    db.AffiliateClicks.Add(click);
    await db.SaveChangesAsync();

    var affiliateUrl = affiliateService.GenerateViatorLink(city);
    return Results.Redirect(affiliateUrl);
});

// Auth: verify magic link token (GET so email links work)
app.MapGet("/auth/verify", async (
    string token,
    HttpContext httpContext,
    MagicLinkService magicLinkService) =>
{
    var user = await magicLinkService.ValidateTokenAsync(token);
    if (user is null)
        return Results.Redirect("/konto/logga-in?error=invalid");

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Name, user.DisplayName ?? user.Email)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Redirect("/konto");
});

// Auth: logout
app.MapGet("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

// XML Sitemap
app.MapGet("/sitemap.xml", async (FlygioDbContext db) =>
{
    var xml = await SitemapGenerator.GenerateAsync(db);
    return Results.Content(xml, "application/xml");
}).CacheOutput("SitemapCache");

// Robots.txt
app.MapGet("/robots.txt", () =>
{
    var content = """
        User-agent: *
        Allow: /
        Sitemap: https://flygio.se/sitemap.xml
        """;
    return Results.Content(content, "text/plain");
}).CacheOutput("StaticContent");

app.Run();
