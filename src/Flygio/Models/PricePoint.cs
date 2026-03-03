namespace Flygio.Models;

public class PricePoint
{
    public int Id { get; set; }
    public int FlightRouteId { get; set; }
    public int Price { get; set; } // SEK
    public string? Airline { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int NumberOfChanges { get; set; }
    public DateTime FoundAt { get; set; } = DateTime.UtcNow;

    public FlightRoute FlightRoute { get; set; } = null!;
}
