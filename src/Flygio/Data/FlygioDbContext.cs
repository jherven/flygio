using Flygio.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Data;

public class FlygioDbContext : DbContext
{
    public FlygioDbContext(DbContextOptions<FlygioDbContext> options) : base(options) { }

    public DbSet<FlightRoute> FlightRoutes => Set<FlightRoute>();
    public DbSet<PricePoint> PricePoints => Set<PricePoint>();
    public DbSet<PriceAlert> PriceAlerts => Set<PriceAlert>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<AffiliateClick> AffiliateClicks => Set<AffiliateClick>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlightRoute>(entity =>
        {
            entity.HasIndex(e => new { e.OriginIata, e.DestinationIata }).IsUnique();
            entity.HasIndex(e => e.IsPopular);
            entity.Property(e => e.OriginIata).HasMaxLength(3);
            entity.Property(e => e.DestinationIata).HasMaxLength(3);
            entity.Property(e => e.OriginCity).HasMaxLength(100);
            entity.Property(e => e.DestinationCity).HasMaxLength(100);
        });

        modelBuilder.Entity<PricePoint>(entity =>
        {
            entity.HasIndex(e => e.FlightRouteId);
            entity.HasIndex(e => e.FoundAt);
            entity.HasIndex(e => e.DepartureDate);
            entity.Property(e => e.Airline).HasMaxLength(100);
            entity.HasOne(e => e.FlightRoute)
                .WithMany(r => r.PricePoints)
                .HasForeignKey(e => e.FlightRouteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PriceAlert>(entity =>
        {
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.ConfirmationToken).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.IsConfirmed });
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.ConfirmationToken).HasMaxLength(64);
            entity.HasOne(e => e.FlightRoute)
                .WithMany(r => r.PriceAlerts)
                .HasForeignKey(e => e.FlightRouteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.DestinationIata);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.MetaDescription).HasMaxLength(300);
            entity.Property(e => e.DestinationIata).HasMaxLength(3);
            entity.Property(e => e.DestinationCity).HasMaxLength(100);
        });

        modelBuilder.Entity<AffiliateClick>(entity =>
        {
            entity.HasIndex(e => e.ClickedAt);
            entity.HasIndex(e => e.DestinationIata);
            entity.Property(e => e.DestinationIata).HasMaxLength(3);
            entity.Property(e => e.AffiliateUrl).HasMaxLength(500);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.HasOne(e => e.FlightRoute)
                .WithMany()
                .HasForeignKey(e => e.FlightRouteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Subscriber>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.ConfirmationToken).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.ConfirmationToken).HasMaxLength(64);
        });
    }

    public static string ParseDatabaseUrl(string databaseUrl)
    {
        // Railway format: postgresql://user:pass@host:port/dbname
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
    }
}
