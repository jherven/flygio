namespace Flygio.Data.Models;

public class SavedSearch
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public required string OriginCode { get; set; }
    public required string DestinationCode { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int? MaxPrice { get; set; }
    public bool NotifyOnPriceDrop { get; set; } = true;
    public decimal? LastKnownPrice { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
