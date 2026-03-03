namespace Flygio.Configuration;

public class ResendOptions
{
    public string ApiKey { get; set; } = "";
    public string FromEmail { get; set; } = "noreply@flygio.se";
    public string FromName { get; set; } = "Flygio.se";
}
