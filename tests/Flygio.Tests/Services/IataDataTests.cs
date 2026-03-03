using Flygio.Configuration;

namespace Flygio.Tests.Services;

public class IataDataTests
{
    [Fact]
    public void GetCityName_KnownIata_ReturnsCity()
    {
        Assert.Equal("Stockholm Arlanda", IataData.GetCityName("ARN"));
        Assert.Equal("Barcelona", IataData.GetCityName("BCN"));
    }

    [Fact]
    public void GetCityName_UnknownIata_ReturnsIataCode()
    {
        Assert.Equal("XYZ", IataData.GetCityName("XYZ"));
    }

    [Fact]
    public void GetCityName_IsCaseInsensitive()
    {
        Assert.Equal("Stockholm Arlanda", IataData.GetCityName("arn"));
    }

    [Fact]
    public void IsValidIata_KnownCode_ReturnsTrue()
    {
        Assert.True(IataData.IsValidIata("ARN"));
        Assert.True(IataData.IsValidIata("BCN"));
    }

    [Fact]
    public void IsValidIata_UnknownCode_ReturnsFalse()
    {
        Assert.False(IataData.IsValidIata("XYZ"));
    }

    [Fact]
    public void SwedishAirports_ContainsExpectedAirports()
    {
        Assert.True(IataData.SwedishAirports.ContainsKey("ARN"));
        Assert.True(IataData.SwedishAirports.ContainsKey("GOT"));
        Assert.True(IataData.SwedishAirports.ContainsKey("MMX"));
    }

    [Fact]
    public void PopularDestinations_ContainsExpectedCities()
    {
        Assert.True(IataData.PopularDestinations.ContainsKey("BCN"));
        Assert.True(IataData.PopularDestinations.ContainsKey("BKK"));
        Assert.True(IataData.PopularDestinations.ContainsKey("LHR"));
    }
}
