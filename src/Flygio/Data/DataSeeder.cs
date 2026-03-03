using Flygio.Configuration;
using Flygio.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Data;

public static class DataSeeder
{
    private static readonly (string Origin, string Destination)[] PopularRoutes =
    [
        // ARN -> 15 destinations
        ("ARN", "BCN"), ("ARN", "LHR"), ("ARN", "BKK"), ("ARN", "AGP"), ("ARN", "ATH"),
        ("ARN", "PMI"), ("ARN", "FCO"), ("ARN", "CDG"), ("ARN", "IST"), ("ARN", "TFS"),
        ("ARN", "LPA"), ("ARN", "DXB"), ("ARN", "AYT"), ("ARN", "ALC"), ("ARN", "FAO"),
        // GOT -> 7 destinations
        ("GOT", "BCN"), ("GOT", "LHR"), ("GOT", "AGP"), ("GOT", "PMI"), ("GOT", "ALC"),
        ("GOT", "AYT"), ("GOT", "TFS"),
        // MMX -> 3 destinations
        ("MMX", "BCN"), ("MMX", "LHR"), ("MMX", "AGP"),
    ];

    public static async Task SeedAsync(FlygioDbContext db)
    {
        foreach (var (origin, destination) in PopularRoutes)
        {
            var exists = await db.FlightRoutes
                .AnyAsync(r => r.OriginIata == origin && r.DestinationIata == destination);

            if (!exists)
            {
                db.FlightRoutes.Add(new FlightRoute
                {
                    OriginIata = origin,
                    DestinationIata = destination,
                    OriginCity = IataData.GetCityName(origin),
                    DestinationCity = IataData.GetCityName(destination),
                    IsPopular = true
                });
            }
        }

        await db.SaveChangesAsync();

        // Seed articles
        foreach (var article in ArticleSeedData.GetArticles())
        {
            var exists = await db.Articles.AnyAsync(a => a.Slug == article.Slug);
            if (!exists)
            {
                db.Articles.Add(article);
            }
        }

        await db.SaveChangesAsync();
    }
}
