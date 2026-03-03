using Flygio.Models;
using Microsoft.EntityFrameworkCore;

namespace Flygio.Data;

public class FlygioDbContext : DbContext
{
    public FlygioDbContext(DbContextOptions<FlygioDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<TravelService> TravelServices => Set<TravelService>();
    public DbSet<TravelServiceCategory> TravelServiceCategories => Set<TravelServiceCategory>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<AffiliateClick> AffiliateClicks => Set<AffiliateClick>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        modelBuilder.Entity<TravelService>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.IsFeatured);
            entity.HasIndex(e => e.IsPublished);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.WebsiteUrl).HasMaxLength(500);
            entity.Property(e => e.AffiliateUrl).HasMaxLength(500);
            entity.Property(e => e.Rating).HasPrecision(3, 1);
        });

        modelBuilder.Entity<TravelServiceCategory>(entity =>
        {
            entity.HasKey(e => new { e.TravelServiceId, e.CategoryId });
            entity.HasOne(e => e.TravelService)
                .WithMany(s => s.TravelServiceCategories)
                .HasForeignKey(e => e.TravelServiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.TravelServiceCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.CategoryId);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.MetaDescription).HasMaxLength(300);
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AffiliateClick>(entity =>
        {
            entity.HasIndex(e => e.ClickedAt);
            entity.HasIndex(e => e.ServiceName);
            entity.Property(e => e.ServiceName).HasMaxLength(200);
            entity.Property(e => e.AffiliateUrl).HasMaxLength(500);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.HasOne(e => e.TravelService)
                .WithMany()
                .HasForeignKey(e => e.TravelServiceId)
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
