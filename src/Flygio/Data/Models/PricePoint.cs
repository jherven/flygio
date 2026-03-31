namespace Flygio.Data.Models;

public class PricePoint
{
    public int Id { get; set; }
    public int FlightRouteId { get; set; }
    public required string Provider { get; set; }
    public decimal Price { get; set; }
    public required string Currency { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;

    public FlightRoute FlightRoute { get; set; } = null!;
}
