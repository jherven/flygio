using System.Text;
using System.Xml.Linq;
using Flygio.Components;
using Flygio.Configuration;
using Flygio.Data;
using Flygio.Models;
using Flygio.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configuration
var adminApiKey = Environment.GetEnvironmentVariable("ADMIN_API_KEY") ?? "";

builder.Services.Configure<TravelpayoutsOptions>(options =>
{
    options.ApiToken = Environment.GetEnvironmentVariable("TRAVELPAYOUTS_API_TOKEN") ?? "";
    options.Marker = Environment.GetEnvironmentVariable("TRAVELPAYOUTS_MARKER") ?? "503994";
});

builder.Services.Configure<ResendOptions>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";
});

// Database
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var connectionString = FlygioDbContext.ParseDatabaseUrl(databaseUrl);
    builder.Services.AddDbContextFactory<FlygioDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContextFactory<FlygioDbContext>(options =>
        options.UseNpgsql("Host=localhost;Database=flygio;Username=postgres;Password=postgres"));
}

// Output caching
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(10)));
    options.AddPolicy("RoutePage", policy => policy.Expire(TimeSpan.FromHours(1)).SetVaryByRouteValue(["RouteCode"]));
    options.AddPolicy("ArticlePage", policy => policy.Expire(TimeSpan.FromHours(1)).SetVaryByRouteValue(["Slug"]));
    options.AddPolicy("StaticPage", policy => policy.Expire(TimeSpan.FromHours(1)));
});

// Memory cache
builder.Services.AddMemoryCache();

// Health checks
builder.Services.AddHealthChecks();

// Flight search service (conditional)
var flightProvider = Environment.GetEnvironmentVariable("FLIGHT_API_PROVIDER") ?? "travelpayouts";
if (flightProvider == "amadeus")
{
    builder.Services.AddSingleton<IFlightSearchService, AmadeusService>();
}
else
{
    builder.Services.AddHttpClient<IFlightSearchService, TravelpayoutsService>();
}

// Services
builder.Services.AddSingleton<AffiliateService>();
builder.Services.AddHttpClient<IEmailService, EmailService>();

// Background services
builder.Services.AddHostedService<PriceTrackingService>();
builder.Services.AddHostedService<AlertService>();

// Railway uses PORT env variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Auto-migrate and seed database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
        await dbContext.Database.MigrateAsync();
        await DataSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database migration/seed failed - database may not be available");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseOutputCache();
app.UseAntiforgery();
app.MapStaticAssets();

// Health check
app.MapHealthChecks("/health");

// ─── Admin API Endpoints ───

app.MapPost("/admin/articles/import", async (HttpContext ctx, IDbContextFactory<FlygioDbContext> dbFactory) =>
{
    if (ctx.Request.Headers["X-Admin-Key"].FirstOrDefault() != adminApiKey || string.IsNullOrEmpty(adminApiKey))
        return Results.Unauthorized();

    var articles = await ctx.Request.ReadFromJsonAsync<List<ArticleImportDto>>();
    if (articles is null) return Results.BadRequest("Invalid JSON");

    await using var db = await dbFactory.CreateDbContextAsync();

    foreach (var dto in articles)
    {
        var existing = await db.Articles.FirstOrDefaultAsync(a => a.Slug == dto.Slug);
        if (existing is not null)
        {
            existing.Title = dto.Title;
            existing.MetaDescription = dto.MetaDescription;
            existing.Content = dto.Content;
            existing.DestinationIata = dto.DestinationIata;
            existing.DestinationCity = dto.DestinationCity;
            existing.IsPublished = dto.IsPublished;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            db.Articles.Add(new Article
            {
                Slug = dto.Slug,
                Title = dto.Title,
                MetaDescription = dto.MetaDescription,
                Content = dto.Content,
                DestinationIata = dto.DestinationIata,
                DestinationCity = dto.DestinationCity,
                IsPublished = dto.IsPublished
            });
        }
    }

    await db.SaveChangesAsync();
    return Results.Ok(new { imported = articles.Count });
});

app.MapGet("/admin/articles", async (HttpContext ctx, IDbContextFactory<FlygioDbContext> dbFactory) =>
{
    if (ctx.Request.Headers["X-Admin-Key"].FirstOrDefault() != adminApiKey || string.IsNullOrEmpty(adminApiKey))
        return Results.Unauthorized();

    await using var db = await dbFactory.CreateDbContextAsync();
    var articles = await db.Articles
        .Select(a => new { a.Id, a.Slug, a.Title, a.IsPublished, a.CreatedAt, a.UpdatedAt })
        .OrderByDescending(a => a.UpdatedAt)
        .ToListAsync();

    return Results.Ok(articles);
});

app.MapDelete("/admin/articles/{id:int}", async (int id, HttpContext ctx, IDbContextFactory<FlygioDbContext> dbFactory) =>
{
    if (ctx.Request.Headers["X-Admin-Key"].FirstOrDefault() != adminApiKey || string.IsNullOrEmpty(adminApiKey))
        return Results.Unauthorized();

    await using var db = await dbFactory.CreateDbContextAsync();
    var article = await db.Articles.FindAsync(id);
    if (article is null) return Results.NotFound();

    db.Articles.Remove(article);
    await db.SaveChangesAsync();
    return Results.Ok();
});

// ─── Affiliate Click Tracking ───

app.MapGet("/go", async (HttpContext ctx, string? url, int? route, string? dest, IDbContextFactory<FlygioDbContext> dbFactory) =>
{
    if (string.IsNullOrEmpty(url))
        return Results.BadRequest("Missing url parameter");

    // Validate URL domain to prevent open redirect
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
        (!uri.Host.EndsWith("aviasales.com") && !uri.Host.EndsWith("travelpayouts.com")))
    {
        return Results.BadRequest("Invalid affiliate URL");
    }

    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.AffiliateClicks.Add(new AffiliateClick
        {
            FlightRouteId = route,
            DestinationIata = dest,
            AffiliateUrl = url,
            ClickedAt = DateTime.UtcNow,
            UserAgent = ctx.Request.Headers.UserAgent.FirstOrDefault()
        });
        await db.SaveChangesAsync();
    }
    catch { /* Don't block redirect on tracking failure */ }

    return Results.Redirect(url);
});

// ─── Admin Stats ───

app.MapGet("/admin/stats", async (HttpContext ctx, IDbContextFactory<FlygioDbContext> dbFactory) =>
{
    var key = ctx.Request.Query["key"].FirstOrDefault();
    if (key != adminApiKey || string.IsNullOrEmpty(adminApiKey))
        return Results.Unauthorized();

    await using var db = await dbFactory.CreateDbContextAsync();
    var now = DateTime.UtcNow;
    var today = now.Date;
    var weekAgo = today.AddDays(-7);
    var monthAgo = today.AddDays(-30);

    var clicksToday = await db.AffiliateClicks.CountAsync(c => c.ClickedAt >= today);
    var clicksWeek = await db.AffiliateClicks.CountAsync(c => c.ClickedAt >= weekAgo);
    var clicksMonth = await db.AffiliateClicks.CountAsync(c => c.ClickedAt >= monthAgo);

    var topDestinations = await db.AffiliateClicks
        .Where(c => c.ClickedAt >= monthAgo && c.DestinationIata != null)
        .GroupBy(c => c.DestinationIata)
        .Select(g => new { Destination = g.Key, Clicks = g.Count() })
        .OrderByDescending(x => x.Clicks)
        .Take(10)
        .ToListAsync();

    var dailyClicks = await db.AffiliateClicks
        .Where(c => c.ClickedAt >= monthAgo)
        .GroupBy(c => c.ClickedAt.Date)
        .Select(g => new { Date = g.Key, Clicks = g.Count() })
        .OrderBy(x => x.Date)
        .ToListAsync();

    return Results.Ok(new { clicksToday, clicksWeek, clicksMonth, topDestinations, dailyClicks });
});

// ─── SEO Endpoints ───

app.MapGet("/sitemap.xml", async (IDbContextFactory<FlygioDbContext> dbFactory) =>

{
    XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
    await using var db = await dbFactory.CreateDbContextAsync();

    var urls = new List<XElement>
    {
        new(ns + "url",
            new XElement(ns + "loc", "https://flygio.se/"),
            new XElement(ns + "changefreq", "daily"),
            new XElement(ns + "priority", "1.0")),
        new(ns + "url",
            new XElement(ns + "loc", "https://flygio.se/rutter"),
            new XElement(ns + "changefreq", "daily"),
            new XElement(ns + "priority", "0.8")),
        new(ns + "url",
            new XElement(ns + "loc", "https://flygio.se/guider"),
            new XElement(ns + "changefreq", "weekly"),
            new XElement(ns + "priority", "0.8")),
        new(ns + "url",
            new XElement(ns + "loc", "https://flygio.se/bevakning"),
            new XElement(ns + "changefreq", "monthly"),
            new XElement(ns + "priority", "0.7")),
    };

    var routes = await db.FlightRoutes.Where(r => r.IsPopular).ToListAsync();
    foreach (var route in routes)
    {
        urls.Add(new XElement(ns + "url",
            new XElement(ns + "loc", $"https://flygio.se/flyg/{route.OriginIata}-{route.DestinationIata}"),
            new XElement(ns + "changefreq", "daily"),
            new XElement(ns + "priority", "0.7")));
    }

    var articles = await db.Articles.Where(a => a.IsPublished).ToListAsync();
    foreach (var article in articles)
    {
        urls.Add(new XElement(ns + "url",
            new XElement(ns + "loc", $"https://flygio.se/guider/{article.Slug}"),
            new XElement(ns + "changefreq", "weekly"),
            new XElement(ns + "priority", "0.6")));
    }

    var doc = new XDocument(
        new XDeclaration("1.0", "UTF-8", null),
        new XElement(ns + "urlset", urls));

    return Results.Text(doc.Declaration + doc.ToString(), "application/xml", Encoding.UTF8);
});

app.MapGet("/robots.txt", () => Results.Text("""
    User-agent: *
    Allow: /
    Disallow: /admin/
    Disallow: /api/

    Sitemap: https://flygio.se/sitemap.xml
    """, "text/plain"));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// ─── DTOs ───

public record ArticleImportDto(
    string Slug,
    string Title,
    string MetaDescription,
    string Content,
    string? DestinationIata,
    string? DestinationCity,
    bool IsPublished = true);
