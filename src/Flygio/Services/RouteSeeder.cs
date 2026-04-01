using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Services;

public static class RouteSeeder
{
    public static async Task SeedAsync(FlygioDbContext db)
    {
        if (await db.FlightRoutes.CountAsync() >= 200)
            return;

        var routes = GetSeedRoutes();
        var existing = await db.FlightRoutes
            .Select(r => r.OriginCode + "-" + r.DestinationCode)
            .ToListAsync();
        var existingSet = new HashSet<string>(existing);

        var newRoutes = routes
            .Where(r => !existingSet.Contains(r.OriginCode + "-" + r.DestinationCode))
            .ToList();

        if (newRoutes.Count > 0)
        {
            db.FlightRoutes.AddRange(newRoutes);
            await db.SaveChangesAsync();
        }
    }

    private static List<FlightRoute> GetSeedRoutes()
    {
        // Swedish origin cities with airport codes
        var origins = new (string City, string Code)[]
        {
            ("Stockholm", "ARN"), ("Göteborg", "GOT"), ("Malmö", "MMX"),
            ("Umeå", "UME"), ("Luleå", "LLA"), ("Sundsvall", "SDL"),
            ("Växjö", "VXO"), ("Linköping", "LPI"), ("Karlstad", "KSD"),
            ("Visby", "VBY")
        };

        // Popular destination cities with airport codes
        var destinations = new (string City, string Code, bool IsPopular)[]
        {
            ("Barcelona", "BCN", true), ("London", "LHR", true), ("Bangkok", "BKK", true),
            ("Málaga", "AGP", true), ("Aten", "ATH", true), ("Alicante", "ALC", true),
            ("Palma de Mallorca", "PMI", true), ("Paris", "CDG", true), ("Rom", "FCO", true),
            ("Amsterdam", "AMS", true), ("Berlin", "BER", true), ("Istanbul", "IST", true),
            ("Köpenhamn", "CPH", true), ("Dublin", "DUB", false), ("Lissabon", "LIS", false),
            ("Prag", "PRG", false), ("Budapest", "BUD", false), ("Wien", "VIE", false),
            ("München", "MUC", false), ("Nice", "NCE", false), ("Oslo", "OSL", true),
            ("Helsingfors", "HEL", false), ("Warszawa", "WAW", false), ("New York", "JFK", true),
            ("Dubai", "DXB", true), ("Antalya", "AYT", true), ("Rhodos", "RHO", false),
            ("Kreta", "HER", false), ("Larnaca", "LCA", false), ("Split", "SPU", false),
            ("Dubrovnik", "DBV", false), ("Milano", "MXP", false), ("Bryssel", "BRU", false),
            ("Madrid", "MAD", false), ("Edinburgh", "EDI", false), ("Reykjavik", "KEF", false),
            ("Zürich", "ZRH", false), ("Genève", "GVA", false), ("Phuket", "HKT", false),
            ("Tokyo", "NRT", false), ("Marrakech", "RAK", false), ("Hurghada", "HRG", false),
        };

        var routes = new List<FlightRoute>();
        foreach (var origin in origins)
        {
            foreach (var dest in destinations)
            {
                // Skip domestic-like routes (e.g. Malmö to Copenhagen is very short)
                if (origin.Code == "MMX" && dest.Code == "CPH") continue;

                routes.Add(new FlightRoute
                {
                    Origin = origin.City,
                    Destination = dest.City,
                    OriginCode = origin.Code,
                    DestinationCode = dest.Code,
                    IsPopular = dest.IsPopular && (origin.Code == "ARN" || origin.Code == "GOT"),
                });

                if (routes.Count >= 200) return routes;
            }
        }

        return routes;
    }
}
