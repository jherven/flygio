using Flygio.Data;

namespace Flygio.Tests.Services;

public class DatabaseUrlTests
{
    [Fact]
    public void ParseDatabaseUrl_RailwayFormat_ReturnsConnectionString()
    {
        var url = "postgresql://user:pass@host.railway.app:5432/dbname";
        var result = FlygioDbContext.ParseDatabaseUrl(url);

        Assert.Contains("Host=host.railway.app", result);
        Assert.Contains("Port=5432", result);
        Assert.Contains("Database=dbname", result);
        Assert.Contains("Username=user", result);
        Assert.Contains("Password=pass", result);
    }

    [Fact]
    public void ParseDatabaseUrl_EncodedPassword_DecodesCorrectly()
    {
        var url = "postgresql://user:p%40ss%23word@host:5432/db";
        var result = FlygioDbContext.ParseDatabaseUrl(url);

        Assert.Contains("Password=p@ss#word", result);
    }
}
