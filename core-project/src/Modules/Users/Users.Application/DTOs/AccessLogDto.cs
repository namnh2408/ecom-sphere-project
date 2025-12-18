namespace Users.Application.DTOs;

public record AccessLogDto(
    Guid Id,
    Guid UserId,
    string ResourceName,
    string HttpMethod,
    string? ResourceId,
    bool WasSuccessful,
    int? HttpStatusCode,
    string? FailureReason,
    string? IpAddress,
    long? ResponseTimeMs,
    DateTime AccessedAtUtc);