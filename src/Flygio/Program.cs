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
    .AddDbContextCheck<FlygioDbContext>();

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

// Newsletter service
builder.Services.AddSingleton<NewsletterService>();

// OG image generation
builder.Services.AddSingleton<OgImageService>();

// Stripe payments
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection(StripeSettings.SectionName));
builder.Services.AddSingleton<StripeService>();

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

    // Fix table ownership using superuser credentials before running migrations.
    // Derives admin connection from POSTGRES_USER/POSTGRES_PASSWORD env vars (already set for the postgres container).
    var adminConnStr = app.Configuration.GetConnectionString("AdminConnection")
                     ?? app.Configuration["ADMIN_DATABASE_URL"];
    if (string.IsNullOrEmpty(adminConnStr))
    {
        // Build admin connection from the postgres container's superuser env vars
        var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        if (!string.IsNullOrEmpty(pgUser) && !string.IsNullOrEmpty(pgPassword))
        {
            var appConnStr = app.Configuration.GetConnectionString("DefaultConnection")!;
            var csb = new Npgsql.NpgsqlConnectionStringBuilder(appConnStr)
            {
                Username = pgUser,
                Password = pgPassword
            };
            adminConnStr = csb.ConnectionString;
        }
    }
    if (!string.IsNullOrEmpty(adminConnStr))
    {
        try
        {
            using var adminConn = new Npgsql.NpgsqlConnection(adminConnStr);
            await adminConn.OpenAsync();
            // Get the app user from the regular connection
            var appConnStr = app.Configuration.GetConnectionString("DefaultConnection")!;
            var appUser = new Npgsql.NpgsqlConnectionStringBuilder(appConnStr).Username ?? "flygio";
            using var cmd = adminConn.CreateCommand();
            cmd.CommandText = $"""
                DO $$
                DECLARE r RECORD;
                BEGIN
                    FOR r IN SELECT tablename FROM pg_tables WHERE schemaname = 'public' LOOP
                        EXECUTE format('ALTER TABLE %I OWNER TO {0}', r.tablename);
                    END LOOP;
                    FOR r IN SELECT sequencename FROM pg_sequences WHERE schemaname = 'public' LOOP
                        EXECUTE format('ALTER SEQUENCE %I OWNER TO {0}', r.sequencename);
                    END LOOP;
                END $$;
                """.Replace("{0}", appUser);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not fix table ownership: {ex.Message}");
        }
    }

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

// Stripe webhook
app.MapPost("/api/webhooks/stripe", async (HttpContext httpContext, StripeService stripeService, ILogger<Program> logger) =>
{
    var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
    var signature = httpContext.Request.Headers["Stripe-Signature"].FirstOrDefault();

    if (string.IsNullOrEmpty(signature))
        return Results.BadRequest("Missing Stripe-Signature header");

    try
    {
        await stripeService.HandleWebhookEventAsync(json, signature);
        return Results.Ok();
    }
    catch (Stripe.StripeException ex)
    {
        logger.LogWarning(ex, "Stripe webhook signature verification failed");
        return Results.BadRequest("Invalid signature");
    }
});

// Stripe customer portal redirect
app.MapGet("/api/stripe/portal", async (HttpContext httpContext, StripeService stripeService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (userId is null || !int.TryParse(userId, out var id))
        return Results.Unauthorized();

    try
    {
        var portalUrl = await stripeService.CreateCustomerPortalSessionAsync(id, "https://flygio.se/konto");
        return Results.Redirect(portalUrl);
    }
    catch (InvalidOperationException)
    {
        return Results.BadRequest("No active subscription");
    }
}).RequireAuthorization();

// OG image endpoints
app.MapGet("/og-image/default", (OgImageService ogService) =>
{
    var bytes = ogService.GenerateDefaultImage();
    return Results.File(bytes, "image/png");
}).CacheOutput("StaticContent");

app.MapGet("/og-image/route/{originSlug}-till-{destSlug}", async (
    string originSlug,
    string destSlug,
    FlygioDbContext db,
    OgImageService ogService) =>
{
    var routes = await db.FlightRoutes.ToListAsync();
    var route = routes.FirstOrDefault(r =>
        SlugHelper.ToSlug(r.Origin) == originSlug &&
        SlugHelper.ToSlug(r.Destination) == destSlug);

    if (route is null)
        return Results.NotFound();

    var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
    var lowestPrice = await db.PricePoints
        .Where(p => p.FlightRoute.OriginCode == route.OriginCode
                    && p.FlightRoute.DestinationCode == route.DestinationCode
                    && p.ScrapedAt >= thirtyDaysAgo)
        .MinAsync(p => (decimal?)p.Price);

    var bytes = ogService.GenerateRouteImage(route.Origin, route.Destination, route.OriginCode, route.DestinationCode, lowestPrice);
    return Results.File(bytes, "image/png");
}).CacheOutput("StaticContent");

app.MapGet("/og-image/destination/{slug}", async (
    string slug,
    FlygioDbContext db,
    OgImageService ogService) =>
{
    var dest = await db.Destinations.FirstOrDefaultAsync(d => d.Slug == slug && d.IsPublished);
    if (dest is null)
        return Results.NotFound();

    var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
    var lowestPrice = await db.PricePoints
        .Where(p => p.FlightRoute.DestinationCode == dest.AirportCode && p.ScrapedAt >= thirtyDaysAgo)
        .MinAsync(p => (decimal?)p.Price);

    var bytes = ogService.GenerateDestinationImage(dest.City, dest.Country, dest.AirportCode, lowestPrice);
    return Results.File(bytes, "image/png");
}).CacheOutput("StaticContent");

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
