using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence;

public class UsersDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserActivityHistory> UserActivityHistories => Set<UserActivityHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ChangeHistory> ChangeHistories => Set<ChangeHistory>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configurations
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).ValueGeneratedNever();
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.IsActive).HasDefaultValue(true);
            builder.Property(u => u.IsEmailVerified).HasDefaultValue(false);
            builder.Property(u => u.CreatedAtUtc).IsRequired();
            
            // Own the Email value object
            builder.OwnsOne(u => u.Email, eo =>
            {
                eo.Property(e => e.Value).HasColumnName("Email").IsRequired().HasMaxLength(255);
            });

            // Own the Password value object
            builder.OwnsOne(u => u.Password, po =>
            {
                po.Property(p => p.Hash).HasColumnName("PasswordHash").IsRequired();
                po.Property(p => p.Salt).HasColumnName("PasswordSalt").IsRequired();
            });

            // Configure roles collection as owned
            builder.Property<List<Guid>>("_roleIds")
                .HasColumnName("RoleIds")
                .HasConversion(
                    v => string.Join(",", v),
                    v => ConvertStringToGuids(v)
                );

            // Additional columns for soft delete and profile picture
            builder.Property(u => u.IsDeleted).HasDefaultValue(false);
            builder.Property(u => u.ProfilePicturePath).HasMaxLength(500);

            // Add index for soft delete queries
            builder.HasIndex(u => new { u.IsActive, u.IsDeleted });
        });

        // Role configurations
        modelBuilder.Entity<Role>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedNever();
            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Description).IsRequired().HasMaxLength(500);
            builder.Property(r => r.IsActive).HasDefaultValue(true);
            builder.Property(r => r.CreatedAtUtc).IsRequired();

            // Configure permissions collection as owned
            builder.Property<List<Guid>>("_permissionIds")
                .HasColumnName("PermissionIds")
                .HasConversion(
                    v => string.Join(",", v),
                    v => ConvertStringToGuids(v)
                );
        });

        // Permission configurations
        modelBuilder.Entity<Permission>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Description).IsRequired().HasMaxLength(500);
            builder.Property(p => p.Resource).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Action).IsRequired().HasMaxLength(100);
            builder.Property(p => p.IsActive).HasDefaultValue(true);
            builder.Property(p => p.CreatedAtUtc).IsRequired();

            builder.HasIndex(p => new { p.Resource, p.Action }).IsUnique();
        });

        // EmailVerificationToken configurations
        modelBuilder.Entity<EmailVerificationToken>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Token).IsRequired().HasMaxLength(500);
            builder.Property(e => e.ExpiresAtUtc).IsRequired();
            builder.Property(e => e.CreatedAtUtc).IsRequired();
            builder.Property(e => e.IsVerified).HasDefaultValue(false);

            builder.HasIndex(e => e.Token).IsUnique();
            builder.HasIndex(e => e.UserId);
        });

        // PasswordResetToken configurations
        modelBuilder.Entity<PasswordResetToken>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();
            builder.Property(p => p.UserId).IsRequired();
            builder.Property(p => p.Token).IsRequired().HasMaxLength(500);
            builder.Property(p => p.ExpiresAtUtc).IsRequired();
            builder.Property(p => p.CreatedAtUtc).IsRequired();
            builder.Property(p => p.IsUsed).HasDefaultValue(false);

            builder.HasIndex(p => p.Token).IsUnique();
            builder.HasIndex(p => p.UserId);
        });

        // LoginAttempt configurations
        modelBuilder.Entity<LoginAttempt>(builder =>
        {
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).ValueGeneratedNever();
            builder.Property(l => l.UserId).IsRequired();
            builder.Property(l => l.Email).IsRequired().HasMaxLength(255);
            builder.Property(l => l.IsSuccessful).IsRequired();
            builder.Property(l => l.FailureReason).HasMaxLength(500);
            builder.Property(l => l.IpAddress).HasMaxLength(50);
            builder.Property(l => l.UserAgent).HasMaxLength(500);
            builder.Property(l => l.AttemptedAtUtc).IsRequired();

            builder.HasIndex(l => l.UserId);
            builder.HasIndex(l => l.AttemptedAtUtc);
            builder.HasIndex(l => new { l.UserId, l.IsSuccessful });
        });

        // RefreshToken configurations
        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedNever();
            builder.Property(r => r.UserId).IsRequired();
            builder.Property(r => r.Token).IsRequired().HasMaxLength(500);
            builder.Property(r => r.ExpiresAtUtc).IsRequired();
            builder.Property(r => r.CreatedAtUtc).IsRequired();
            builder.Property(r => r.IsRevoked).HasDefaultValue(false);

            builder.HasIndex(r => r.Token).IsUnique();
            builder.HasIndex(r => r.UserId);
        });

        // UserActivityHistory configurations
        modelBuilder.Entity<UserActivityHistory>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.ActivityType).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
            builder.Property(x => x.IpAddress).HasMaxLength(50);
            builder.Property(x => x.UserAgent).HasMaxLength(500);
            builder.Property(x => x.OccurredAtUtc).IsRequired();
            builder.Property(x => x.Metadata)
                .HasConversion(
                    v => SerializeMetadata(v),
                    v => DeserializeMetadata(v)
                );

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ActivityType);
            builder.HasIndex(x => x.OccurredAtUtc);
        });

        // AuditLog configurations
        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.EntityName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.EntityId).IsRequired();
            builder.Property(x => x.OperationType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.OldValues).HasColumnType("nvarchar(max)");
            builder.Property(x => x.NewValues).HasColumnType("nvarchar(max)");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.IpAddress).HasMaxLength(50);
            builder.Property(x => x.OccurredAtUtc).IsRequired();

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => new { x.EntityName, x.EntityId });
            builder.HasIndex(x => x.OperationType);
            builder.HasIndex(x => x.OccurredAtUtc);
        });

        // ChangeHistory configurations
        modelBuilder.Entity<ChangeHistory>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.FieldName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.OldValue).HasMaxLength(500);
            builder.Property(x => x.NewValue).HasMaxLength(500);
            builder.Property(x => x.ChangeReason).IsRequired().HasMaxLength(100);
            builder.Property(x => x.IpAddress).HasMaxLength(50);
            builder.Property(x => x.ChangedAtUtc).IsRequired();
            builder.Property(x => x.IsReversible).HasDefaultValue(false);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => new { x.UserId, x.FieldName });
            builder.HasIndex(x => x.ChangedAtUtc);
        });

        // AccessLog configurations
        modelBuilder.Entity<AccessLog>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.ResourceName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.HttpMethod).IsRequired().HasMaxLength(10);
            builder.Property(x => x.ResourceId).HasMaxLength(100);
            builder.Property(x => x.WasSuccessful).IsRequired();
            builder.Property(x => x.FailureReason).HasMaxLength(500);
            builder.Property(x => x.IpAddress).HasMaxLength(50);
            builder.Property(x => x.UserAgent).HasMaxLength(500);
            builder.Property(x => x.AccessedAtUtc).IsRequired();

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.ResourceName);
            builder.HasIndex(x => new { x.UserId, x.WasSuccessful });
            builder.HasIndex(x => x.AccessedAtUtc);
        });
    }

    private static List<Guid> ConvertStringToGuids(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new List<Guid>();

        return value.Split(',')
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => Guid.Parse(s.Trim()))
            .ToList();
    }

    private static string? SerializeMetadata(Dictionary<string, object>? value)
    {
        if (value == null)
            return null;
        
        return System.Text.Json.JsonSerializer.Serialize(value);
    }

    private static Dictionary<string, object>? DeserializeMetadata(string? value)
    {
        if (value == null)
            return null;
        
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value);
    }
}