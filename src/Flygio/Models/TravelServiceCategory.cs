namespace Flygio.Models;

public class TravelServiceCategory
{
    public int TravelServiceId { get; set; }
    public TravelService TravelService { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
