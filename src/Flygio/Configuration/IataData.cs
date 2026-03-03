namespace Flygio.Configuration;

public static class IataData
{
    public static readonly Dictionary<string, string> SwedishAirports = new()
    {
        ["ARN"] = "Stockholm Arlanda",
        ["GOT"] = "Göteborg Landvetter",
        ["MMX"] = "Malmö Sturup",
        ["BMA"] = "Stockholm Bromma",
        ["NYO"] = "Stockholm Skavsta",
        ["VST"] = "Stockholm Västerås",
        ["LLA"] = "Luleå",
        ["UME"] = "Umeå",
        ["OSD"] = "Östersund",
        ["VBY"] = "Visby",
        ["KRN"] = "Kiruna",
        ["RNB"] = "Ronneby",
        ["SDL"] = "Sundsvall",
    };

    public static readonly Dictionary<string, string> PopularDestinations = new()
    {
        ["BCN"] = "Barcelona",
        ["LHR"] = "London",
        ["BKK"] = "Bangkok",
        ["AGP"] = "Malaga",
        ["ATH"] = "Aten",
        ["PMI"] = "Palma de Mallorca",
        ["FCO"] = "Rom",
        ["CDG"] = "Paris",
        ["IST"] = "Istanbul",
        ["TFS"] = "Teneriffa",
        ["LPA"] = "Gran Canaria",
        ["DXB"] = "Dubai",
        ["AYT"] = "Antalya",
        ["ALC"] = "Alicante",
        ["FAO"] = "Faro",
        ["SPU"] = "Split",
        ["HER"] = "Heraklion",
        ["TLL"] = "Tallinn",
        ["RIX"] = "Riga",
        ["CPH"] = "Köpenhamn",
    };

    public static readonly Dictionary<string, string> DestinationImages = new()
    {
        ["BCN"] = "https://images.unsplash.com/photo-1583422409516-2895a77efded?w=600&h=400&fit=crop",
        ["LHR"] = "https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?w=600&h=400&fit=crop",
        ["BKK"] = "https://images.unsplash.com/photo-1508009603885-50cf7c579365?w=600&h=400&fit=crop",
        ["AGP"] = "https://images.unsplash.com/photo-1509840841025-9088ba78a826?w=600&h=400&fit=crop",
        ["ATH"] = "https://images.unsplash.com/photo-1555993539-1732b0258235?w=600&h=400&fit=crop",
        ["PMI"] = "https://images.unsplash.com/photo-1581889470536-467bdbe30cd0?w=600&h=400&fit=crop",
        ["FCO"] = "https://images.unsplash.com/photo-1552832230-c0197dd311b5?w=600&h=400&fit=crop",
        ["CDG"] = "https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=600&h=400&fit=crop",
        ["IST"] = "https://images.unsplash.com/photo-1524231757912-21f4fe3a7200?w=600&h=400&fit=crop",
        ["TFS"] = "https://images.unsplash.com/photo-1540202404-a2f29016b523?w=600&h=400&fit=crop",
        ["LPA"] = "https://images.unsplash.com/photo-1585208798174-6cedd86e019a?w=600&h=400&fit=crop",
        ["DXB"] = "https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=600&h=400&fit=crop",
        ["AYT"] = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=600&h=400&fit=crop",
        ["ALC"] = "https://images.unsplash.com/photo-1558642452-9d2a7deb7f62?w=600&h=400&fit=crop",
        ["FAO"] = "https://images.unsplash.com/photo-1555881400-74d7acaacd8b?w=600&h=400&fit=crop",
        ["SPU"] = "https://images.unsplash.com/photo-1555990793-da11153b2473?w=600&h=400&fit=crop",
        ["HER"] = "https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff?w=600&h=400&fit=crop",
        ["TLL"] = "https://images.unsplash.com/photo-1560969184-10fe8719e047?w=600&h=400&fit=crop",
        ["RIX"] = "https://images.unsplash.com/photo-1591634616938-1dfa7ee2e617?w=600&h=400&fit=crop",
        ["CPH"] = "https://images.unsplash.com/photo-1513622470522-26c3c8a854bc?w=600&h=400&fit=crop",
    };

    public static readonly Dictionary<string, string> HeroImages = new()
    {
        ["default"] = "https://images.unsplash.com/photo-1436491865332-7a61a109db05?w=1920&h=800&fit=crop",
        ["beach"] = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=1920&h=800&fit=crop",
        ["city"] = "https://images.unsplash.com/photo-1477959858617-67f85cf4f1df?w=1920&h=800&fit=crop",
    };

    public static readonly Dictionary<string, string> AllAirports;

    static IataData()
    {
        AllAirports = new Dictionary<string, string>(SwedishAirports);
        foreach (var dest in PopularDestinations)
        {
            AllAirports[dest.Key] = dest.Value;
        }
    }

    public static string GetCityName(string iata)
    {
        return AllAirports.TryGetValue(iata.ToUpperInvariant(), out var city) ? city : iata;
    }

    public static bool IsValidIata(string iata)
    {
        return AllAirports.ContainsKey(iata.ToUpperInvariant());
    }

    public static string GetDestinationImage(string iata)
    {
        return DestinationImages.TryGetValue(iata.ToUpperInvariant(), out var url)
            ? url
            : HeroImages["default"];
    }
}
