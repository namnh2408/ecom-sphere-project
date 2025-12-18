namespace Users.Domain.Entities;

/// <summary>
/// Records user activity history for audit trail
/// </summary>
public class UserActivityHistory
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string ActivityType { get; private set; } = null!; // LOGIN, LOGOUT, CREATE_ROLE, etc.
    public string Description { get; private set; } = null!;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; } // Additional context

    private UserActivityHistory() { }

    public UserActivityHistory(
        Guid userId,
        string activityType,
        string description,
        string? ipAddress = null,
        string? userAgent = null,
        Dictionary<string, object>? metadata = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ActivityType = activityType;
        Description = description;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Metadata = metadata;
        OccurredAtUtc = DateTime.UtcNow;
    }

    public static UserActivityHistory Create(
        Guid userId,
        string activityType,
        string description,
        string? ipAddress = null,
        string? userAgent = null,
        Dictionary<string, object>? metadata = null)
    {
        return new UserActivityHistory(userId, activityType, description, ipAddress, userAgent, metadata);
    }
}