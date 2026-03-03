using Flygio.Models;

namespace Flygio.Data;

public class SiteData
{
    private readonly List<Category> _categories;
    private readonly List<TravelService> _services;
    private readonly List<TravelServiceCategory> _serviceCategories;
    private readonly List<Article> _articles;

    public SiteData()
    {
        // Load categories
        _categories = CategorySeedData.GetCategories();
        for (var i = 0; i < _categories.Count; i++)
            _categories[i].Id = i + 1;

        var categoryBySlug = _categories.ToDictionary(c => c.Slug);

        // Load services
        var seedServices = TravelServiceSeedData.GetServices();
        _services = new List<TravelService>(seedServices.Count);
        _serviceCategories = [];

        for (var i = 0; i < seedServices.Count; i++)
        {
            var (service, categorySlugs) = seedServices[i];
            service.Id = i + 1;
            _services.Add(service);

            foreach (var slug in categorySlugs)
            {
                if (categoryBySlug.TryGetValue(slug, out var cat))
                {
                    var link = new TravelServiceCategory
                    {
                        TravelServiceId = service.Id,
                        TravelService = service,
                        CategoryId = cat.Id,
                        Category = cat
                    };
                    _serviceCategories.Add(link);
                    service.TravelServiceCategories.Add(link);
                    cat.TravelServiceCategories.Add(link);
                }
            }
        }

        // Load articles
        var seedArticles = ArticleSeedData.GetArticles();
        _articles = new List<Article>(seedArticles.Count);

        for (var i = 0; i < seedArticles.Count; i++)
        {
            var (slug, title, metaDescription, categorySlug, content) = seedArticles[i];
            var article = new Article
            {
                Id = i + 1,
                Slug = slug,
                Title = title,
                MetaDescription = metaDescription,
                Content = content,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (categorySlug is not null && categoryBySlug.TryGetValue(categorySlug, out var articleCat))
            {
                article.CategoryId = articleCat.Id;
                article.Category = articleCat;
            }

            _articles.Add(article);
        }
    }

    public List<Category> GetCategories() =>
        _categories.OrderBy(c => c.SortOrder).ToList();

    public Category? GetCategoryBySlug(string slug) =>
        _categories.FirstOrDefault(c => c.Slug == slug);

    public List<TravelService> GetServicesByCategory(int categoryId) =>
        _serviceCategories
            .Where(tsc => tsc.CategoryId == categoryId && tsc.TravelService.IsPublished)
            .OrderByDescending(tsc => tsc.TravelService.Rating)
            .Select(tsc => tsc.TravelService)
            .ToList();

    public TravelService? GetServiceBySlug(string slug) =>
        _services.FirstOrDefault(s => s.Slug == slug && s.IsPublished);

    public List<TravelService> GetFeaturedServices(int count) =>
        _services
            .Where(s => s.IsPublished && s.IsFeatured)
            .OrderByDescending(s => s.Rating)
            .Take(count)
            .ToList();

    public List<Article> GetArticles() =>
        _articles.Where(a => a.IsPublished).OrderByDescending(a => a.CreatedAt).ToList();

    public List<Article> GetPublishedArticles() => GetArticles();

    public Article? GetArticleBySlug(string slug) =>
        _articles.FirstOrDefault(a => a.Slug == slug && a.IsPublished);

    public int GetCategoryServiceCount(int categoryId) =>
        _serviceCategories.Count(tsc => tsc.CategoryId == categoryId && tsc.TravelService.IsPublished);

    public List<TravelService> GetRelatedServices(int serviceId, int categoryId, int count) =>
        _serviceCategories
            .Where(tsc => tsc.CategoryId == categoryId
                          && tsc.TravelServiceId != serviceId
                          && tsc.TravelService.IsPublished)
            .OrderByDescending(tsc => tsc.TravelService.Rating)
            .Take(count)
            .Select(tsc => tsc.TravelService)
            .ToList();

    public Category? GetPrimaryCategory(int serviceId) =>
        _serviceCategories
            .FirstOrDefault(tsc => tsc.TravelServiceId == serviceId)
            ?.Category;

    public List<TravelService> GetPublishedServices() =>
        _services.Where(s => s.IsPublished).ToList();
}
