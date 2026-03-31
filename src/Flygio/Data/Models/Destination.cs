namespace Flygio.Data.Models;

public class Destination
{
    public int Id { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public required string Slug { get; set; }
    public required string AirportCode { get; set; }
    public required string MetaDescription { get; set; }
    public required string Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
