using Flygio.Services.DTOs;

namespace Flygio.Services;

public interface IFlightSearchService
{
    Task<List<FlightOffer>> GetLatestPricesAsync(string origin, string destination);
    Task<List<FlightOffer>> GetCheapestAsync(string origin, string destination);
    Task<List<CalendarPrice>> GetMonthMatrixAsync(string origin, string destination);
    Task<List<CalendarPrice>> GetCalendarPricesAsync(string origin, string destination, int month, int year);
    Task<List<PopularRoute>> GetPopularRoutesAsync(string origin = "ARN");
}
