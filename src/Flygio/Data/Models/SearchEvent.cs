namespace Flygio.Data.Models;

public class SearchEvent
{
    public int Id { get; set; }
    public required string OriginCode { get; set; }
    public required string DestinationCode { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Passengers { get; set; }
    public int ResultCount { get; set; }
    public string? SessionId { get; set; }
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
}
