using BuildingBlocks.Abstractions;

namespace Users.Domain.Entities;

/// <summary>
/// Password reset token for secure password recovery
/// </summary>
public class PasswordResetToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public bool IsUsed { get; private set; }

    private PasswordResetToken() { }

    public PasswordResetToken(Guid id, Guid userId, string token, DateTime expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        IsUsed = false;
    }

    public static PasswordResetToken Create(Guid userId, int expirationMinutes = 60)
    {
        var token = GenerateToken();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);
        return new PasswordResetToken(Guid.NewGuid(), userId, token, expiresAtUtc);
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAtUtc;

    public bool IsValid => !IsUsed && !IsExpired;

    public void MarkAsUsed()
    {
        if (IsExpired)
            throw new InvalidOperationException("Token has expired");

        if (IsUsed)
            throw new InvalidOperationException("Token already used");

        IsUsed = true;
        UsedAtUtc = DateTime.UtcNow;
    }

    private static string GenerateToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}