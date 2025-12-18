using Microsoft.EntityFrameworkCore;
using Catalog.Domain.Products;
using Catalog.Domain.Categories;

namespace Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configurations
        modelBuilder.Entity<Product>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.Name).IsRequired().HasMaxLength(500);
            builder.Property(p => p.Description).HasMaxLength(2000);
            builder.Property(p => p.Status).IsRequired().HasMaxLength(50);
            builder.Property(p => p.PrimaryImagePath).HasMaxLength(500);
            builder.Property(p => p.CreatedAtUtc).IsRequired();

            // Own the Sku value object
            builder.OwnsOne(p => p.Sku, eo =>
            {
                eo.Property(s => s.Value).HasColumnName("Sku").IsRequired().HasMaxLength(50);
                eo.HasIndex(s => s.Value).IsUnique();
            });

            // Own the Price value object
            builder.OwnsOne(p => p.Price, eo =>
            {
                eo.Property(m => m.Amount).HasColumnName("Price").IsRequired().HasPrecision(18, 2);
            });

            // Own the CostPrice value object (optional)
            builder.OwnsOne(p => p.CostPrice, eo =>
            {
                eo.Property(m => m.Amount).HasColumnName("CostPrice").HasPrecision(18, 2);
            });

            // Own the Stock value object
            builder.OwnsOne(p => p.Stock, eo =>
            {
                eo.Property(s => s.Quantity).HasColumnName("StockQuantity").IsRequired();
            });

            // Add indexes (note: Sku index is handled via owned value object configuration)
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => new { p.IsDeleted, p.Status });
            builder.HasIndex(p => p.CreatedAtUtc);

            // Configure soft delete filter
            builder.HasQueryFilter(p => !p.IsDeleted);
        });

        // Category configurations
        modelBuilder.Entity<Category>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedNever();

            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Description).HasMaxLength(500);
            builder.Property(c => c.Slug).IsRequired().HasMaxLength(200);
            builder.Property(c => c.IsActive).HasDefaultValue(true);
            builder.Property(c => c.CreatedAtUtc).IsRequired();

            // Add indexes
            builder.HasIndex(c => c.Slug).IsUnique();
            builder.HasIndex(c => c.Name).IsUnique();
            builder.HasIndex(c => c.IsActive);

            // Configure soft delete filter
            builder.HasQueryFilter(c => !c.IsDeleted);
        });
    }
}