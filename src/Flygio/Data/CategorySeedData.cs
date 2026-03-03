using Flygio.Models;

namespace Flygio.Data;

public static class CategorySeedData
{
    public static List<Category> GetCategories() =>
    [
        new() { Name = "Flyg", Slug = "flyg", Description = "Jämför flygsökmotorer och flygbolag för att hitta de billigaste flygbiljetterna.", Icon = "plane", SortOrder = 1 },
        new() { Name = "Hotell", Slug = "hotell", Description = "Hitta de bästa hotellbokningssajterna och jämför priser på boende världen över.", Icon = "hotel", SortOrder = 2 },
        new() { Name = "Hyrbil", Slug = "hyrbil", Description = "Jämför hyrbilstjänster och hitta bästa pris på biluthyrning.", Icon = "car", SortOrder = 3 },
        new() { Name = "Reseförsäkring", Slug = "reseforsakring", Description = "Jämför reseförsäkringar och hitta rätt skydd för din resa.", Icon = "shield", SortOrder = 4 },
        new() { Name = "Paketresor", Slug = "paketresor", Description = "Jämför researrangörer och hitta de bästa paketresorna från Sverige.", Icon = "package", SortOrder = 5 },
        new() { Name = "Aktiviteter", Slug = "aktiviteter", Description = "Boka upplevelser, turer och aktiviteter på ditt resmål.", Icon = "compass", SortOrder = 6 },
    ];
}
