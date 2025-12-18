namespace Users.Domain.Entities;

/// <summary>
/// Records access to resources and API endpoints for security audit
/// </summary>
public class AccessLog
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string ResourceName { get; private set; } = null!; // Endpoint or Resource name
    public string HttpMethod { get; private set; } = null!; // GET, POST, PUT, DELETE
    public string? ResourceId { get; private set; } // ID of resource being accessed
    public bool WasSuccessful { get; private set; }
    public int? HttpStatusCode { get; private set; }
    public string? FailureReason { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public long? ResponseTimeMs { get; private set; } // Response time in milliseconds
    public DateTime AccessedAtUtc { get; private set; }

    private AccessLog() { }

    public AccessLog(
        Guid userId,
        string resourceName,
        string httpMethod,
        bool wasSuccessful,
        string? resourceId = null,
        int? httpStatusCode = null,
        string? failureReason = null,
        string? ipAddress = null,
        string? userAgent = null,
        long? responseTimeMs = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ResourceName = resourceName;
        HttpMethod = httpMethod;
        WasSuccessful = wasSuccessful;
        ResourceId = resourceId;
        HttpStatusCode = httpStatusCode;
        FailureReason = failureReason;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ResponseTimeMs = responseTimeMs;
        AccessedAtUtc = DateTime.UtcNow;
    }

    public static AccessLog Create(
        Guid userId,
        string resourceName,
        string httpMethod,
        bool wasSuccessful,
        string? resourceId = null,
        int? httpStatusCode = null,
        string? failureReason = null,
        string? ipAddress = null,
        string? userAgent = null,
        long? responseTimeMs = null)
    {
        return new AccessLog(userId, resourceName, httpMethod, wasSuccessful, resourceId, httpStatusCode, failureReason, ipAddress, userAgent, responseTimeMs);
    }
}