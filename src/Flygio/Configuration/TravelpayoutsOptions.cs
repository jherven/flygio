namespace Flygio.Configuration;

public class TravelpayoutsOptions
{
    public string ApiToken { get; set; } = "";
    public string Marker { get; set; } = "503994";
    public string BaseUrl { get; set; } = "https://api.travelpayouts.com/v2/";
}
