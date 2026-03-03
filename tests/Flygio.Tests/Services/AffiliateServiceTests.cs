using Flygio.Configuration;
using Flygio.Services;
using Microsoft.Extensions.Options;

namespace Flygio.Tests.Services;

public class AffiliateServiceTests
{
    private readonly AffiliateService _service;

    public AffiliateServiceTests()
    {
        var options = Options.Create(new TravelpayoutsOptions { Marker = "503994" });
        _service = new AffiliateService(options);
    }

    [Fact]
    public void GenerateLink_WithDates_ReturnsCorrectFormat()
    {
        var departure = new DateTime(2026, 6, 15);
        var returnDate = new DateTime(2026, 6, 22);

        var link = _service.GenerateLink("ARN", "BCN", departure, returnDate);

        Assert.Contains("ARN", link);
        Assert.Contains("BCN", link);
        Assert.Contains("marker=503994", link);
        Assert.Contains("1506", link); // 15 June DDMM
        Assert.Contains("2206", link); // 22 June DDMM
    }

    [Fact]
    public void GenerateLink_WithoutDates_ReturnsGenericLink()
    {
        var link = _service.GenerateLink("ARN", "BCN");

        Assert.Contains("ARN", link);
        Assert.Contains("BCN", link);
        Assert.Contains("marker=503994", link);
        Assert.StartsWith("https://www.aviasales.com/search/", link);
    }

    [Fact]
    public void GenerateGenericLink_ReturnsCorrectFormat()
    {
        var link = _service.GenerateGenericLink("GOT", "LHR");

        Assert.Equal("https://www.aviasales.com/search/GOTLHR1?marker=503994", link);
    }
}
