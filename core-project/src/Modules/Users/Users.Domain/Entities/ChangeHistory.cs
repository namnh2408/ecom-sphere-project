namespace Users.Domain.Entities;

/// <summary>
/// Tracks specific changes to user data (password, email, profile, etc.)
/// </summary>
public class ChangeHistory
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string FieldName { get; private set; } = null!; // PASSWORD, EMAIL, FIRST_NAME, etc.
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string ChangeReason { get; private set; } = null!; // USER_REQUEST, ADMIN_ACTION, etc.
    public string? IpAddress { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }
    public bool IsReversible { get; private set; } // Can this change be reverted

    private ChangeHistory() { }

    public ChangeHistory(
        Guid userId,
        string fieldName,
        string? oldValue,
        string? newValue,
        string changeReason,
        string? ipAddress = null,
        bool isReversible = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        FieldName = fieldName;
        OldValue = oldValue;
        NewValue = newValue;
        ChangeReason = changeReason;
        IpAddress = ipAddress;
        IsReversible = isReversible;
        ChangedAtUtc = DateTime.UtcNow;
    }

    public static ChangeHistory Create(
        Guid userId,
        string fieldName,
        string? oldValue,
        string? newValue,
        string changeReason,
        string? ipAddress = null,
        bool isReversible = false)
    {
        return new ChangeHistory(userId, fieldName, oldValue, newValue, changeReason, ipAddress, isReversible);
    }
}