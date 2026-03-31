namespace Flygio.Services;

public class AmadeusSettings
{
    public const string SectionName = "Amadeus";

    public required string BaseUrl { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}
