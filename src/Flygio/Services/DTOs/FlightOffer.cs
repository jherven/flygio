namespace Flygio.Services.DTOs;

public class FlightOffer
{
    public required string Origin { get; set; }
    public required string Destination { get; set; }
    public int Price { get; set; } // SEK
    public string? Airline { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int NumberOfChanges { get; set; }
    public DateTime FoundAt { get; set; }
}

public class PopularRoute
{
    public required string Origin { get; set; }
    public required string Destination { get; set; }
    public string OriginCity { get; set; } = "";
    public string DestinationCity { get; set; } = "";
    public int? CheapestPrice { get; set; }
    public int Popularity { get; set; }
}

public class CalendarPrice
{
    public DateTime Date { get; set; }
    public int Price { get; set; } // SEK
    public string? Airline { get; set; }
    public int NumberOfChanges { get; set; }
}
