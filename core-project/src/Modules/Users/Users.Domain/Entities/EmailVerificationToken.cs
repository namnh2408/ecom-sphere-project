using BuildingBlocks.Abstractions;

namespace Users.Domain.Entities;

/// <summary>
/// Email verification token for confirming user email address
/// </summary>
public class EmailVerificationToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? VerifiedAtUtc { get; private set; }
    public bool IsVerified { get; private set; }

    private EmailVerificationToken() { }

    public EmailVerificationToken(Guid id, Guid userId, string token, DateTime expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        IsVerified = false;
    }

    public static EmailVerificationToken Create(Guid userId, int expirationMinutes = 24 * 60)
    {
        var token = GenerateToken();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);
        return new EmailVerificationToken(Guid.NewGuid(), userId, token, expiresAtUtc);
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAtUtc;

    public bool IsValid => !IsVerified && !IsExpired;

    public void Verify()
    {
        if (IsExpired)
            throw new InvalidOperationException("Token has expired");

        if (IsVerified)
            throw new InvalidOperationException("Token already verified");

        IsVerified = true;
        VerifiedAtUtc = DateTime.UtcNow;
    }

    private static string GenerateToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}