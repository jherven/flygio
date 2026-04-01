using Microsoft.Extensions.Options;

namespace Flygio.Services;

public class TravelpayoutsAffiliateLinkService(IOptions<TravelpayoutsSettings> settings)
{
    private readonly TravelpayoutsSettings _settings = settings.Value;

    /// <summary>
    /// Generates an Aviasales affiliate search link.
    /// </summary>
    public string GenerateAviasalesLink(string originCode, string destinationCode, DateTime departureDate, DateTime? returnDate = null)
    {
        var subId = BuildSubId(originCode, destinationCode);
        var dateStr = departureDate.ToString("ddMM");
        var returnStr = returnDate?.ToString("ddMM") ?? "";
        var route = $"{originCode}{dateStr}{destinationCode}{returnStr}1";

        return $"https://www.aviasales.com/search/{route}?marker={_settings.MarkerId}&with_request=true&t=flygio_{subId}";
    }

    /// <summary>
    /// Generates a WayAway affiliate link for a route.
    /// </summary>
    public string GenerateWayAwayLink(string originCode, string destinationCode, DateTime departureDate, DateTime? returnDate = null)
    {
        var subId = BuildSubId(originCode, destinationCode);
        var dep = departureDate.ToString("yyyy-MM-dd");

        var url = $"https://www.wayaway.io/flights/{originCode}-{destinationCode}/{dep}";
        if (returnDate.HasValue)
            url += $"/{returnDate.Value:yyyy-MM-dd}";

        return $"{url}?marker={_settings.MarkerId}&utm_source=flygio&sub_id={subId}";
    }

    /// <summary>
    /// Generates a Kiwi.com (via Travelpayouts) affiliate link.
    /// </summary>
    public string GenerateKiwiLink(string originCode, string destinationCode, DateTime departureDate, DateTime? returnDate = null)
    {
        var subId = BuildSubId(originCode, destinationCode);
        var dep = departureDate.ToString("dd/MM/yyyy");

        var url = $"https://www.kiwi.com/deep?affilid={_settings.MarkerId}" +
                  $"&from={originCode}&to={destinationCode}" +
                  $"&departure={dep}" +
                  $"&lang=sv&currency=SEK";

        if (returnDate.HasValue)
            url += $"&return={returnDate.Value:dd/MM/yyyy}";

        return $"{url}&sub_id={subId}";
    }

    /// <summary>
    /// Returns the internal redirect URL that logs clicks before redirecting.
    /// </summary>
    public string GetRedirectUrl(string provider, string originCode, string destinationCode, DateTime departureDate, DateTime? returnDate = null)
    {
        var dep = departureDate.ToString("yyyy-MM-dd");
        var url = $"/go/{provider}?origin={originCode}&dest={destinationCode}&dep={dep}";
        if (returnDate.HasValue)
            url += $"&ret={returnDate.Value:yyyy-MM-dd}";
        return url;
    }

    /// <summary>
    /// Resolves the final affiliate URL for a given provider.
    /// </summary>
    public string ResolveAffiliateUrl(string provider, string originCode, string destinationCode, DateTime departureDate, DateTime? returnDate)
    {
        return provider.ToLowerInvariant() switch
        {
            "aviasales" => GenerateAviasalesLink(originCode, destinationCode, departureDate, returnDate),
            "wayaway" => GenerateWayAwayLink(originCode, destinationCode, departureDate, returnDate),
            "kiwi" => GenerateKiwiLink(originCode, destinationCode, departureDate, returnDate),
            _ => GenerateAviasalesLink(originCode, destinationCode, departureDate, returnDate)
        };
    }

    /// <summary>
    /// Generates a Hotellook affiliate search link for hotels.
    /// </summary>
    public string GenerateHotellookLink(string city, DateTime checkIn, DateTime checkOut, int adults = 2)
    {
        var subId = $"hotel_{city.ToLowerInvariant().Replace(" ", "_")}";
        var checkInStr = checkIn.ToString("yyyy-MM-dd");
        var checkOutStr = checkOut.ToString("yyyy-MM-dd");

        return $"https://search.hotellook.com/hotels?destination={Uri.EscapeDataString(city)}" +
               $"&checkIn={checkInStr}&checkOut={checkOutStr}&adults={adults}" +
               $"&marker={_settings.MarkerId}&locale=sv&sub_id={subId}";
    }

    /// <summary>
    /// Returns the internal redirect URL for hotel affiliate clicks.
    /// </summary>
    public string GetHotelRedirectUrl(string city, DateTime checkIn, DateTime checkOut, int adults = 2)
    {
        var checkInStr = checkIn.ToString("yyyy-MM-dd");
        var checkOutStr = checkOut.ToString("yyyy-MM-dd");
        return $"/go/hotellook?city={Uri.EscapeDataString(city)}&checkin={checkInStr}&checkout={checkOutStr}&adults={adults}";
    }

    public static string BuildSubId(string originCode, string destinationCode)
    {
        return $"{originCode}_{destinationCode}".ToLowerInvariant();
    }

    public static string BuildHotelSubId(string city)
    {
        return $"hotel_{city.ToLowerInvariant().Replace(" ", "_")}";
    }

    /// <summary>
    /// Generates a Rentalcars affiliate search link for car rentals via Travelpayouts.
    /// </summary>
    public string GenerateRentalcarsLink(string city, DateTime pickUp, DateTime dropOff)
    {
        var subId = BuildCarSubId(city);
        var pickUpStr = pickUp.ToString("yyyy-MM-dd");
        var dropOffStr = dropOff.ToString("yyyy-MM-dd");

        return $"https://www.rentalcars.com/search-results?location={Uri.EscapeDataString(city)}" +
               $"&puDay={pickUp.Day}&puMonth={pickUp.Month}&puYear={pickUp.Year}" +
               $"&doDay={dropOff.Day}&doMonth={dropOff.Month}&doYear={dropOff.Year}" +
               $"&marker={_settings.MarkerId}&sub_id={subId}";
    }

    /// <summary>
    /// Generates an Economybookings affiliate search link for car rentals via Travelpayouts.
    /// </summary>
    public string GenerateEconomybookingsLink(string city, DateTime pickUp, DateTime dropOff)
    {
        var subId = BuildCarSubId(city);
        var pickUpStr = pickUp.ToString("yyyy-MM-dd");
        var dropOffStr = dropOff.ToString("yyyy-MM-dd");

        return $"https://www.economybookings.com/search?location={Uri.EscapeDataString(city)}" +
               $"&pick_up_date={pickUpStr}&drop_off_date={dropOffStr}" +
               $"&marker={_settings.MarkerId}&sub_id={subId}";
    }

    /// <summary>
    /// Returns the internal redirect URL for car rental affiliate clicks.
    /// </summary>
    public string GetCarRentalRedirectUrl(string provider, string city, DateTime pickUp, DateTime dropOff)
    {
        var pickUpStr = pickUp.ToString("yyyy-MM-dd");
        var dropOffStr = dropOff.ToString("yyyy-MM-dd");
        return $"/go/{provider}?city={Uri.EscapeDataString(city)}&pickup={pickUpStr}&dropoff={dropOffStr}";
    }

    /// <summary>
    /// Resolves the final affiliate URL for a car rental provider.
    /// </summary>
    public string ResolveCarAffiliateUrl(string provider, string city, DateTime pickUp, DateTime dropOff)
    {
        return provider.ToLowerInvariant() switch
        {
            "rentalcars" => GenerateRentalcarsLink(city, pickUp, dropOff),
            "economybookings" => GenerateEconomybookingsLink(city, pickUp, dropOff),
            _ => GenerateRentalcarsLink(city, pickUp, dropOff)
        };
    }

    /// <summary>
    /// Generates a GetYourGuide affiliate link for activities via Travelpayouts.
    /// </summary>
    public string GenerateGetYourGuideLink(string city)
    {
        var subId = BuildActivitySubId(city);
        return $"https://www.getyourguide.com/s/?q={Uri.EscapeDataString(city)}" +
               $"&partner_id={_settings.MarkerId}&sub_id={subId}";
    }

    /// <summary>
    /// Generates a Viator affiliate link for activities via Travelpayouts.
    /// </summary>
    public string GenerateViatorLink(string city)
    {
        var subId = BuildActivitySubId(city);
        return $"https://www.viator.com/searchResults/all?text={Uri.EscapeDataString(city)}" +
               $"&pid={_settings.MarkerId}&sub_id={subId}";
    }

    /// <summary>
    /// Returns the internal redirect URL for activity affiliate clicks.
    /// </summary>
    public string GetActivityRedirectUrl(string provider, string city)
    {
        return $"/go/{provider}?city={Uri.EscapeDataString(city)}";
    }

    /// <summary>
    /// Resolves the final affiliate URL for an activity provider.
    /// </summary>
    public string ResolveActivityAffiliateUrl(string provider, string city)
    {
        return provider.ToLowerInvariant() switch
        {
            "getyourguide" => GenerateGetYourGuideLink(city),
            "viator" => GenerateViatorLink(city),
            _ => GenerateGetYourGuideLink(city)
        };
    }

    public static string BuildCarSubId(string city)
    {
        return $"car_{city.ToLowerInvariant().Replace(" ", "_")}";
    }

    public static string BuildActivitySubId(string city)
    {
        return $"activity_{city.ToLowerInvariant().Replace(" ", "_")}";
    }
}
