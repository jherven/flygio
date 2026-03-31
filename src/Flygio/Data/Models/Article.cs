namespace Flygio.Data.Models;

public class Article
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string MetaDescription { get; set; }
    public required string Body { get; set; }
    public string? CategorySlug { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
