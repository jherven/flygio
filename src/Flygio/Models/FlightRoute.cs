namespace Flygio.Models;

public class FlightRoute
{
    public int Id { get; set; }
    public required string OriginIata { get; set; }
    public required string DestinationIata { get; set; }
    public required string OriginCity { get; set; }
    public required string DestinationCity { get; set; }
    public bool IsPopular { get; set; }

    public ICollection<PricePoint> PricePoints { get; set; } = [];
    public ICollection<PriceAlert> PriceAlerts { get; set; } = [];
}
