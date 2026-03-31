namespace Flygio.Services;

public class ResendSettings
{
    public const string SectionName = "Resend";

    public string ApiKey { get; set; } = "";
    public string FromEmail { get; set; } = "noreply@flygio.se";
    public string FromName { get; set; } = "Flygio";
}
