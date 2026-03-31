namespace Flygio.Services;

public record FlightSearchResult
{
    public required string Origin { get; init; }
    public required string Destination { get; init; }
    public required string OriginCode { get; init; }
    public required string DestinationCode { get; init; }
    public required List<FlightOffer> Offers { get; init; }
}

public record FlightOffer
{
    public required string Provider { get; init; }
    public decimal Price { get; init; }
    public required string Currency { get; init; }
    public DateTime DepartureDate { get; init; }
    public DateTime? ReturnDate { get; init; }
    public required string Airline { get; init; }
    public int Stops { get; init; }
    public required string Duration { get; init; }
}
