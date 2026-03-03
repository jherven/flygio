namespace Flygio.Models;

public class Subscriber
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public bool IsConfirmed { get; set; }
    public required string ConfirmationToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
