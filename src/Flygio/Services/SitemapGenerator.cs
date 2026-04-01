using System.Text;
using Flygio.Data;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Services;

public static class SitemapGenerator
{
    public static async Task<string> GenerateAsync(FlygioDbContext db)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Static pages
        AddUrl(sb, "https://flygio.se/", "daily", "1.0");
        AddUrl(sb, "https://flygio.se/flyg-till", "daily", "0.9");
        AddUrl(sb, "https://flygio.se/flygrutter", "daily", "0.9");
        AddUrl(sb, "https://flygio.se/artiklar", "weekly", "0.8");

        // Destination pages
        var destinations = await db.Destinations
            .Where(d => d.IsPublished)
            .Select(d => new { d.Slug, d.UpdatedAt })
            .ToListAsync();

        foreach (var dest in destinations)
        {
            AddUrl(sb, $"https://flygio.se/flyg-till/{dest.Slug}", "daily", "0.8", dest.UpdatedAt);
            AddUrl(sb, $"https://flygio.se/hotell/{dest.Slug}", "weekly", "0.6", dest.UpdatedAt);
            AddUrl(sb, $"https://flygio.se/hyrbil/{dest.Slug}", "weekly", "0.5", dest.UpdatedAt);
            AddUrl(sb, $"https://flygio.se/aktiviteter/{dest.Slug}", "weekly", "0.5", dest.UpdatedAt);
        }

        // Route pages
        var routes = await db.FlightRoutes
            .Select(r => new { r.Origin, r.Destination, r.IsPopular, r.UpdatedAt })
            .ToListAsync();

        foreach (var route in routes)
        {
            var originSlug = SlugHelper.ToSlug(route.Origin);
            var destSlug = SlugHelper.ToSlug(route.Destination);
            var priority = route.IsPopular ? "0.7" : "0.6";
            AddUrl(sb, $"https://flygio.se/flyg/{originSlug}-till-{destSlug}", "daily", priority, route.UpdatedAt);
        }

        // Article pages
        var articles = await db.Articles
            .Where(a => a.IsPublished)
            .Select(a => new { a.Slug, a.UpdatedAt })
            .ToListAsync();

        foreach (var article in articles)
        {
            AddUrl(sb, $"https://flygio.se/artiklar/{article.Slug}", "weekly", "0.7", article.UpdatedAt);
        }

        sb.AppendLine("</urlset>");
        return sb.ToString();
    }

    private static void AddUrl(StringBuilder sb, string loc, string changefreq, string priority, DateTime? lastmod = null)
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{loc}</loc>");
        if (lastmod.HasValue)
            sb.AppendLine($"    <lastmod>{lastmod.Value:yyyy-MM-dd}</lastmod>");
        sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
        sb.AppendLine($"    <priority>{priority}</priority>");
        sb.AppendLine("  </url>");
    }
}
