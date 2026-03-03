using Microsoft.Extensions.Options;
using Flygio.Configuration;

namespace Flygio.Services;

public class AffiliateService
{
    private readonly string _marker;

    public AffiliateService(IOptions<TravelpayoutsOptions> options)
    {
        _marker = options.Value.Marker;
    }

    public string GenerateLink(string origin, string destination, DateTime? departureDate = null, DateTime? returnDate = null)
    {
        if (departureDate.HasValue)
        {
            var depart = departureDate.Value.ToString("ddMM");
            var ret = returnDate.HasValue ? returnDate.Value.ToString("ddMM") : "";
            return $"https://www.aviasales.com/search/{origin}{depart}{destination}{ret}1?marker={_marker}";
        }

        return $"https://www.aviasales.com/search/{origin}{destination}1?marker={_marker}";
    }

    public string GenerateGenericLink(string origin, string destination)
    {
        return $"https://www.aviasales.com/search/{origin}{destination}1?marker={_marker}";
    }
}
