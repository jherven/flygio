using Flygio.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Data;

public class FlygioDbContext(DbContextOptions<FlygioDbContext> options) : DbContext(options)
{
    public DbSet<FlightRoute> FlightRoutes => Set<FlightRoute>();
    public DbSet<PricePoint> PricePoints => Set<PricePoint>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<AffiliateClick> AffiliateClicks => Set<AffiliateClick>();
    public DbSet<SearchEvent> SearchEvents => Set<SearchEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlightRoute>(entity =>
        {
            entity.HasIndex(e => new { e.OriginCode, e.DestinationCode });
            entity.HasMany(e => e.PricePoints)
                  .WithOne(p => p.FlightRoute)
                  .HasForeignKey(p => p.FlightRouteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PricePoint>(entity =>
        {
            entity.HasIndex(e => e.DepartureDate);
            entity.Property(e => e.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<AffiliateClick>(entity =>
        {
            entity.HasIndex(e => e.ClickedAt);
            entity.HasIndex(e => new { e.Provider, e.SubId });
        });

        modelBuilder.Entity<SearchEvent>(entity =>
        {
            entity.HasIndex(e => e.SearchedAt);
            entity.HasIndex(e => new { e.OriginCode, e.DestinationCode });
        });
    }
}
