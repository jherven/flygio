namespace Flygio.Data.Models;

public class FlightRoute
{
    public int Id { get; set; }
    public required string Origin { get; set; }
    public required string Destination { get; set; }
    public required string OriginCode { get; set; }
    public required string DestinationCode { get; set; }
    public string? Airline { get; set; }
    public bool IsPopular { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PricePoint> PricePoints { get; set; } = [];
}
