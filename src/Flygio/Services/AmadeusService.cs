using Flygio.Services.DTOs;

namespace Flygio.Services;

public class AmadeusService : IFlightSearchService
{
    public Task<List<FlightOffer>> GetLatestPricesAsync(string origin, string destination)
        => throw new NotImplementedException("Amadeus integration is not yet implemented. Set FLIGHT_API_PROVIDER=travelpayouts.");

    public Task<List<FlightOffer>> GetCheapestAsync(string origin, string destination)
        => throw new NotImplementedException("Amadeus integration is not yet implemented.");

    public Task<List<CalendarPrice>> GetMonthMatrixAsync(string origin, string destination)
        => throw new NotImplementedException("Amadeus integration is not yet implemented.");

    public Task<List<CalendarPrice>> GetCalendarPricesAsync(string origin, string destination, int month, int year)
        => throw new NotImplementedException("Amadeus integration is not yet implemented.");

    public Task<List<PopularRoute>> GetPopularRoutesAsync(string origin = "ARN")
        => throw new NotImplementedException("Amadeus integration is not yet implemented.");
}
