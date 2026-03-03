namespace Flygio.Models;

public class Article
{
    public int Id { get; set; }
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public required string MetaDescription { get; set; }
    public required string Content { get; set; } // HTML
    public string? DestinationIata { get; set; }
    public string? DestinationCity { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
