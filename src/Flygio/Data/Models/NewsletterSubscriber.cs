using System.Security.Cryptography;

namespace Flygio.Data.Models;

public class NewsletterSubscriber
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public bool IsActive { get; set; } = true;
    public string UnsubscribeToken { get; set; } = GenerateToken();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSentAt { get; set; }

    private static string GenerateToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
}
