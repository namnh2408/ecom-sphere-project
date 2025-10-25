using Microsoft.EntityFrameworkCore;
using ShopHub.Services.ProductService.Domain.Entities;

namespace ShopHub.Services.ProductService.Infrastructure.Persistence;

/// <summary>
/// DbContext cho Product Service
/// </summary>
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet cho Product
    /// </summary>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// Cấu hình models
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(builder =>
        {
            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(p => p.Description)
                .HasMaxLength(1024);

            builder.Property(p => p.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Stock)
                .IsRequired();

            builder.Property(p => p.Sku)
                .HasMaxLength(50);

            builder.Property(p => p.ImageUrl)
                .HasMaxLength(512);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.UpdatedAt);

            // Indexes
            builder.HasIndex(p => p.Sku)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(p => p.CategoryId);

            builder.HasIndex(p => p.IsActive)
                .HasFilter("[IsDeleted] = 0");

            // Foreign Keys (nếu có)
            // builder.HasOne<Category>()
            //     .WithMany()
            //     .HasForeignKey(p => p.CategoryId)
            //     .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Override SaveChangesAsync để publish domain events
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Publish domain events trước khi save
        // var domainEvents = ChangeTracker
        //     .Entries<IAggregateRoot>()
        //     .SelectMany(x => x.Entity.DomainEvents)
        //     .ToList();

        // await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }
}