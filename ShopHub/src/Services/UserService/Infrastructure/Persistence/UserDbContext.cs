using Microsoft.EntityFrameworkCore;
using ShopHub.Services.UserService.Domain.Entities;

namespace ShopHub.Services.UserService.Infrastructure.Persistence;

/// <summary>
/// Database Context cho User Service
/// </summary>
public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet cho Users
    /// </summary>
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            builder.Property(u => u.Address)
                .HasMaxLength(255);

            builder.Property(u => u.City)
                .HasMaxLength(100);

            builder.Property(u => u.Country)
                .HasMaxLength(100);

            builder.Property(u => u.RefreshToken)
                .HasMaxLength(500);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValue(DateTime.UtcNow);

            // Create unique constraint on Email
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            // Ignore DomainEvents
            builder.Ignore(u => u.DomainEvents);
        });
    }
}