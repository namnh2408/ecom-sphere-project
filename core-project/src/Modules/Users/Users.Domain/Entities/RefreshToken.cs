using BuildingBlocks.Abstractions;

namespace Users.Domain.Entities;

/// <summary>
/// Refresh token for extending JWT token lifetime
/// </summary>
public class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken() { }

    public RefreshToken(Guid id, Guid userId, string token, DateTime expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        IsRevoked = false;
    }

    public static RefreshToken Create(Guid userId, int expirationDays = 7)
    {
        var token = GenerateToken();
        var expiresAtUtc = DateTime.UtcNow.AddDays(expirationDays);
        return new RefreshToken(Guid.NewGuid(), userId, token, expiresAtUtc);
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAtUtc;

    public bool IsValid => !IsRevoked && !IsExpired;

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAtUtc = DateTime.UtcNow;
    }

    private static string GenerateToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}