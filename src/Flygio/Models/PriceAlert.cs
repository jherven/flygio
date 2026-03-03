namespace Flygio.Models;

public class PriceAlert
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public int FlightRouteId { get; set; }
    public int MaxPrice { get; set; } // SEK
    public bool IsActive { get; set; } = true;
    public bool IsConfirmed { get; set; }
    public required string ConfirmationToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FlightRoute FlightRoute { get; set; } = null!;
}
