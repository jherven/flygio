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
    public DbSet<PriceAlert> PriceAlerts => Set<PriceAlert>();
    public DbSet<Destination> Destinations => Set<Destination>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<MagicLink> MagicLinks => Set<MagicLink>();
    public DbSet<SavedRoute> SavedRoutes => Set<SavedRoute>();
    public DbSet<SavedSearch> SavedSearches => Set<SavedSearch>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();

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

        modelBuilder.Entity<PriceAlert>(entity =>
        {
            entity.HasIndex(e => new { e.OriginCode, e.DestinationCode });
            entity.HasIndex(e => e.UnsubscribeToken).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.Property(e => e.TargetPrice).HasPrecision(10, 2);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.PriceAlerts)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Destination>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.AirportCode);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<MagicLink>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.Email);
        });

        modelBuilder.Entity<SavedRoute>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.SavedRoutes)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SavedSearch>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.LastKnownPrice).HasPrecision(10, 2);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.SavedSearches)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NewsletterSubscriber>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UnsubscribeToken).IsUnique();
        });

    }
}
