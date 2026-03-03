using System.Text;
using System.Xml.Linq;
using Flygio.Components;
using Flygio.Data;
using Flygio.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configuration
builder.Services.Configure<Flygio.Configuration.ResendOptions>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";
});

// In-memory data
builder.Services.AddSingleton<SiteData>();

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseOutputCache();
app.UseAntiforgery();
app.MapStaticAssets();

// Health check
app.MapHealthChecks("/health");

// ─── Affiliate Click Tracking ───

app.MapGet("/go", (HttpContext ctx, string? url, int? service, string? name, ILogger<Program> logger) =>
{
    if (string.IsNullOrEmpty(url))
        return Results.BadRequest("Missing url parameter");

    // Validate URL is absolute to prevent open redirect
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "https" && uri.Scheme != "http"))
    {
        return Results.BadRequest("Invalid URL");
    }

    logger.LogInformation("Affiliate click: service={ServiceName}, url={Url}", name, url);

    return Results.Redirect(url);
});

// ─── SEO Endpoints ───

app.MapGet("/sitemap.xml", (SiteData data) =>
{
    XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

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

    foreach (var category in data.GetCategories())
    {
        urls.Add(new XElement(ns + "url",
            new XElement(ns + "loc", $"https://flygio.se/kategori/{category.Slug}"),
            new XElement(ns + "changefreq", "weekly"),
            new XElement(ns + "priority", "0.8")));
    }

    foreach (var service in data.GetPublishedServices())
    {
        urls.Add(new XElement(ns + "url",
            new XElement(ns + "loc", $"https://flygio.se/tjanst/{service.Slug}"),
            new XElement(ns + "changefreq", "weekly"),
            new XElement(ns + "priority", "0.7")));
    }

    foreach (var article in data.GetArticles())
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
