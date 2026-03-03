namespace Flygio.Models;

public class AffiliateClick
{
    public int Id { get; set; }
    public int? TravelServiceId { get; set; }
    public string? ServiceName { get; set; }
    public required string AffiliateUrl { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    public string? UserAgent { get; set; }

    public TravelService? TravelService { get; set; }
}
