using Flygio.Models;

namespace Flygio.Tests;

public class SmokeTests
{
    [Fact]
    public void Project_Compiles_And_Tests_Run()
    {
        Assert.True(true);
    }

    [Fact]
    public void Category_Model_Has_Required_Properties()
    {
        var category = new Category { Name = "Flyg", Slug = "flyg" };
        Assert.Equal("Flyg", category.Name);
        Assert.Equal("flyg", category.Slug);
    }

    [Fact]
    public void TravelService_Model_Has_Required_Properties()
    {
        var service = new TravelService
        {
            Name = "Skyscanner",
            Slug = "skyscanner",
            Description = "Test",
            WebsiteUrl = "https://skyscanner.se"
        };
        Assert.Equal("Skyscanner", service.Name);
        Assert.True(service.IsPublished);
    }

    [Fact]
    public void TravelServiceCategory_Links_Service_And_Category()
    {
        var category = new Category { Id = 1, Name = "Flyg", Slug = "flyg" };
        var service = new TravelService { Id = 1, Name = "Test", Slug = "test", Description = "D", WebsiteUrl = "https://test.com" };
        var link = new TravelServiceCategory { TravelServiceId = 1, CategoryId = 1, TravelService = service, Category = category };

        Assert.Equal(1, link.TravelServiceId);
        Assert.Equal(1, link.CategoryId);
    }

    [Fact]
    public void AffiliateClick_Uses_TravelServiceId()
    {
        var click = new AffiliateClick
        {
            TravelServiceId = 5,
            ServiceName = "Skyscanner",
            AffiliateUrl = "https://skyscanner.se"
        };
        Assert.Equal(5, click.TravelServiceId);
        Assert.Equal("Skyscanner", click.ServiceName);
    }

    [Fact]
    public void Article_Uses_CategoryId()
    {
        var article = new Article
        {
            Slug = "test",
            Title = "Test",
            MetaDescription = "Test",
            Content = "<p>Test</p>",
            CategoryId = 1
        };
        Assert.Equal(1, article.CategoryId);
    }
}
