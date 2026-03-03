using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Flygio.Components;
using Flygio.Data;
using Flygio.Models;
using Flygio.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configuration
var adminApiKey = Environment.GetEnvironmentVariable("ADMIN_API_KEY") ?? "";

builder.Services.Configure<Flygio.Configuration.ResendOptions>(options =>
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
    options.AddPolicy("CategoryPage", policy => policy.Expire(TimeSpan.FromHours(1)).SetVaryByRouteValue(["slug"]));
    options.AddPolicy("ServicePage", policy => policy.Expire(TimeSpan.FromHours(1)).SetVaryByRouteValue(["slug"]));
    options.AddPolicy("ArticlePage", policy => policy.Expire(TimeSpan.FromHours(1)).SetVaryByRouteValue(["Slug"]));
    options.AddPolicy("StaticPage", policy => policy.Expire(TimeSpan.FromHours(1)));
});

// Memory cache
builder.Services.AddMemoryCache();

// Health checks
builder.Services.AddHealthChecks();

// Services
builder.Services.AddHttpClient<IEmailService, EmailService>();

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
            existing.CategoryId = dto.CategoryId;
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
                CategoryId = dto.CategoryId,
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

// ─── Admin Service Import ───

app.MapPost("/admin/services/import", async (HttpContext ctx, IDbContextFactory<FlygioDbContext> dbFactory) =>
{
    if (ctx.Request.Headers["X-Admin-Key"].FirstOrDefault() != adminApiKey || string.IsNullOrEmpty(adminApiKey))
        return Results.Unauthorized();

    var services = await ctx.Request.ReadFromJsonAsync<List<TravelServiceImportDto>>();
    if (services is null) return Results.BadRequest("Invalid JSON");

    await using var db = await dbFactory.CreateDbContextAsync();

    foreach (var dto in services)
    {
        var existing = await db.TravelServices.FirstOrDefaultAsync(s => s.Slug == dto.Slug);
        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.LongDescription = dto.LongDescription;
            existing.LogoUrl = dto.LogoUrl;
            existing.WebsiteUrl = dto.WebsiteUrl;
            existing.AffiliateUrl = dto.AffiliateUrl;
            existing.Rating = dto.Rating;
            existing.Pros = dto.Pros;
            existing.Cons = dto.Cons;
            existing.IsFeatured = dto.IsFeatured;
            existing.IsPopular = dto.IsPopular;
            existing.IsPublished = dto.IsPublished;
        }
        else
        {
            db.TravelServices.Add(new TravelService
            {
                Name = dto.Name,
                Slug = dto.Slug,
                Description = dto.Description,
                LongDescription = dto.LongDescription,
                LogoUrl = dto.LogoUrl,
                WebsiteUrl = dto.WebsiteUrl,
                AffiliateUrl = dto.AffiliateUrl,
                Rating = dto.Rating,
                Pros = dto.Pros,
                Cons = dto.Cons,
                IsFeatured = dto.IsFeatured,
                IsPopular = dto.IsPopular,
                IsPublished = dto.IsPublished
            });
        }
    }

    await db.SaveChangesAsync();
    return Results.Ok(new { imported = services.Count });
});

// ─── Affiliate Click Tracking ───

app.MapGet("/go", async (HttpContext ctx, string? url, int? service, string? name, IDbContextFactory<FlygioDbContext> dbFactory) =>
{
    if (string.IsNullOrEmpty(url))
        return Results.BadRequest("Missing url parameter");

    // Validate URL is absolute to prevent open redirect
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "https" && uri.Scheme != "http"))
    {
        return Results.BadRequest("Invalid URL");
    }

    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.AffiliateClicks.Add(new AffiliateClick
        {
            TravelServiceId = service,
            ServiceName = name,
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

    var topServices = await db.AffiliateClicks
        .Where(c => c.ClickedAt >= monthAgo && c.ServiceName != null)
        .GroupBy(c => c.ServiceName)
        .Select(g => new { Service = g.Key, Clicks = g.Count() })
        .OrderByDescending(x => x.Clicks)
        .Take(10)
        .ToListAsync();

    var dailyClicks = await db.AffiliateClicks
        .Where(c => c.ClickedAt >= monthAgo)
        .GroupBy(c => c.ClickedAt.Date)
        .Select(g => new { Date = g.Key, Clicks = g.Count() })
        .OrderBy(x => x.Date)
        .ToListAsync();

    return Results.Ok(new { clicksToday, clicksWeek, clicksMonth, topServices, dailyClicks });
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
            new XElement(ns + "loc", "https://flygio.se/guider"),
            new XElement(ns + "changefreq", "weekly"),
            new XElement(ns + "priority", "0.8")),
    };

    var categories = await db.Categories.ToListAsync();
    foreach (var category in categories)
    {
        urls.Add(new XElement(ns + "url",
            new XElement(ns + "loc", $"https://flygio.se/kategori/{category.Slug}"),
            new XElement(ns + "changefreq", "weekly"),
            new XElement(ns + "priority", "0.8")));
    }

    var services = await db.TravelServices.Where(s => s.IsPublished).ToListAsync();
    foreach (var service in services)
    {
        urls.Add(new XElement(ns + "url",
            new XElement(ns + "loc", $"https://flygio.se/tjanst/{service.Slug}"),
            new XElement(ns + "changefreq", "weekly"),
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
    int? CategoryId,
    bool IsPublished = true);

public record TravelServiceImportDto(
    string Name,
    string Slug,
    string Description,
    string? LongDescription,
    string? LogoUrl,
    string WebsiteUrl,
    string? AffiliateUrl,
    decimal Rating,
    string? Pros,
    string? Cons,
    bool IsFeatured = false,
    bool IsPopular = false,
    bool IsPublished = true);
