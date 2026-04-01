using System.Security.Cryptography;

namespace Flygio.Data.Models;

public class MagicLink
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public string Token { get; set; } = GenerateToken();
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private static string GenerateToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
}
