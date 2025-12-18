using BuildingBlocks.Abstractions;

namespace Users.Domain.Entities;

/// <summary>
/// Tracks login attempts for security and throttling
/// </summary>
public class LoginAttempt : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = null!;
    public bool IsSuccessful { get; private set; }
    public string? FailureReason { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime AttemptedAtUtc { get; private set; }

    private LoginAttempt() { }

    public LoginAttempt(Guid id, Guid userId, string email, bool isSuccessful, 
        string? failureReason = null, string? ipAddress = null, string? userAgent = null)
        : base(id)
    {
        UserId = userId;
        Email = email;
        IsSuccessful = isSuccessful;
        FailureReason = failureReason;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        AttemptedAtUtc = DateTime.UtcNow;
    }

    public static LoginAttempt CreateSuccess(Guid userId, string email, string? ipAddress = null, string? userAgent = null)
    {
        return new LoginAttempt(Guid.NewGuid(), userId, email, true, null, ipAddress, userAgent);
    }

    public static LoginAttempt CreateFailure(Guid userId, string email, string failureReason, 
        string? ipAddress = null, string? userAgent = null)
    {
        return new LoginAttempt(Guid.NewGuid(), userId, email, false, failureReason, ipAddress, userAgent);
    }
}