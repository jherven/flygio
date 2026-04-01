namespace Flygio.Data.Models;

public class SavedRoute
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public required string OriginCode { get; set; }
    public required string DestinationCode { get; set; }
    public string? Label { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
