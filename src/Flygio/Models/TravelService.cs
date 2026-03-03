namespace Flygio.Models;

public class TravelService
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string Description { get; set; }
    public string? LongDescription { get; set; }
    public string? LogoUrl { get; set; }
    public required string WebsiteUrl { get; set; }
    public string? AffiliateUrl { get; set; }
    public decimal Rating { get; set; }
    public string? Pros { get; set; } // JSON array of strings
    public string? Cons { get; set; } // JSON array of strings
    public bool IsFeatured { get; set; }
    public bool IsPopular { get; set; }
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; }

    public List<TravelServiceCategory> TravelServiceCategories { get; set; } = [];
}
