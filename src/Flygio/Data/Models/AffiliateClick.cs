namespace Flygio.Data.Models;

public class AffiliateClick
{
    public int Id { get; set; }
    public required string Provider { get; set; }
    public required string OriginCode { get; set; }
    public required string DestinationCode { get; set; }
    public required string SubId { get; set; }
    public string? UserAgent { get; set; }
    public string? Referer { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
}
