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
}
