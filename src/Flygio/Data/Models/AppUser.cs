namespace Flygio.Data.Models;

public class AppUser
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }
    public bool IsPremium { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public List<SavedRoute> SavedRoutes { get; set; } = [];
    public List<SavedSearch> SavedSearches { get; set; } = [];
    public List<PriceAlert> PriceAlerts { get; set; } = [];
}
