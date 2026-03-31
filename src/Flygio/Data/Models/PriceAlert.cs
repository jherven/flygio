using System.Security.Cryptography;

namespace Flygio.Data.Models;

public class PriceAlert
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string OriginCode { get; set; }
    public required string DestinationCode { get; set; }
    public decimal TargetPrice { get; set; }
    public required string Currency { get; set; }
    public bool IsActive { get; set; } = true;
    public string UnsubscribeToken { get; set; } = GenerateToken();
    public DateTime? LastNotifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private static string GenerateToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
}
