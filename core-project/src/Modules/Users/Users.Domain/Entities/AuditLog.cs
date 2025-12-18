namespace Users.Domain.Entities;

/// <summary>
/// Records all changes (Create, Update, Delete) to entities for audit trail
/// </summary>
public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; } // User performing the change
    public string EntityName { get; private set; } = null!; // User, Role, Permission, etc.
    public Guid EntityId { get; private set; } // ID of the changed entity
    public string OperationType { get; private set; } = null!; // CREATE, UPDATE, DELETE, RESTORE
    public string? OldValues { get; private set; } // JSON string of previous values
    public string? NewValues { get; private set; } // JSON string of new values
    public string? Description { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private AuditLog() { }

    public AuditLog(
        Guid userId,
        string entityName,
        Guid entityId,
        string operationType,
        string? oldValues = null,
        string? newValues = null,
        string? description = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        EntityName = entityName;
        EntityId = entityId;
        OperationType = operationType;
        OldValues = oldValues;
        NewValues = newValues;
        Description = description;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        OccurredAtUtc = DateTime.UtcNow;
    }

    public static AuditLog Create(
        Guid userId,
        string entityName,
        Guid entityId,
        string operationType,
        string? oldValues = null,
        string? newValues = null,
        string? description = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new AuditLog(userId, entityName, entityId, operationType, oldValues, newValues, description, ipAddress, userAgent);
    }
}