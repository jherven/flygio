using Flygio.Data;
using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Services;

public class UserSessionService(FlygioDbContext db)
{
    public async Task<AppUser?> GetUserByIdAsync(int userId)
    {
        return await db.Users.FindAsync(userId);
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<PriceAlert>> GetUserAlertsAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return [];
        return await db.PriceAlerts
            .Where(a => a.Email == user.Email && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SavedRoute>> GetUserSavedRoutesAsync(int userId)
    {
        return await db.SavedRoutes
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SavedSearch>> GetUserSavedSearchesAsync(int userId)
    {
        return await db.SavedSearches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<SavedRoute> SaveRouteAsync(int userId, string originCode, string destinationCode, string? label)
    {
        var existing = await db.SavedRoutes.FirstOrDefaultAsync(r =>
            r.UserId == userId && r.OriginCode == originCode && r.DestinationCode == destinationCode);

        if (existing is not null) return existing;

        var route = new SavedRoute
        {
            UserId = userId,
            OriginCode = originCode,
            DestinationCode = destinationCode,
            Label = label
        };
        db.SavedRoutes.Add(route);
        await db.SaveChangesAsync();
        return route;
    }

    public async Task<bool> RemoveSavedRouteAsync(int userId, int routeId)
    {
        var route = await db.SavedRoutes
            .FirstOrDefaultAsync(r => r.Id == routeId && r.UserId == userId);
        if (route is null) return false;
        db.SavedRoutes.Remove(route);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<SavedSearch> SaveSearchAsync(int userId, string originCode, string destinationCode,
        DateTime? departureDate, DateTime? returnDate, int? maxPrice)
    {
        var search = new SavedSearch
        {
            UserId = userId,
            OriginCode = originCode,
            DestinationCode = destinationCode,
            DepartureDate = departureDate,
            ReturnDate = returnDate,
            MaxPrice = maxPrice
        };
        db.SavedSearches.Add(search);
        await db.SaveChangesAsync();
        return search;
    }

    public async Task<bool> RemoveSavedSearchAsync(int userId, int searchId)
    {
        var search = await db.SavedSearches
            .FirstOrDefaultAsync(s => s.Id == searchId && s.UserId == userId);
        if (search is null) return false;
        db.SavedSearches.Remove(search);
        await db.SaveChangesAsync();
        return true;
    }
}
