namespace Flygio.Models;

public class AffiliateClick
{
    public int Id { get; set; }
    public int? FlightRouteId { get; set; }
    public string? DestinationIata { get; set; }
    public required string AffiliateUrl { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    public string? UserAgent { get; set; }

    public FlightRoute? FlightRoute { get; set; }
}
