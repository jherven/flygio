using Flygio.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(FlygioDbContext db)
    {
        // Seed categories
        var categories = CategorySeedData.GetCategories();
        foreach (var category in categories)
        {
            var exists = await db.Categories.AnyAsync(c => c.Slug == category.Slug);
            if (!exists)
            {
                db.Categories.Add(category);
            }
        }
        await db.SaveChangesAsync();

        // Seed travel services
        var serviceData = TravelServiceSeedData.GetServices();
        foreach (var (service, categorySlugs) in serviceData)
        {
            var exists = await db.TravelServices.AnyAsync(s => s.Slug == service.Slug);
            if (!exists)
            {
                db.TravelServices.Add(service);
                await db.SaveChangesAsync();

                foreach (var slug in categorySlugs)
                {
                    var category = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
                    if (category is not null)
                    {
                        db.TravelServiceCategories.Add(new TravelServiceCategory
                        {
                            TravelServiceId = service.Id,
                            CategoryId = category.Id
                        });
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        // Seed articles
        foreach (var article in ArticleSeedData.GetArticles())
        {
            var (slug, title, metaDescription, categorySlug, content) = article;
            var exists = await db.Articles.AnyAsync(a => a.Slug == slug);
            if (!exists)
            {
                var category = !string.IsNullOrEmpty(categorySlug)
                    ? await db.Categories.FirstOrDefaultAsync(c => c.Slug == categorySlug)
                    : null;

                db.Articles.Add(new Article
                {
                    Slug = slug,
                    Title = title,
                    MetaDescription = metaDescription,
                    Content = content,
                    CategoryId = category?.Id
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
