namespace Users.Application.DTOs;

public record UserActivityHistoryDto(
    Guid Id,
    Guid UserId,
    string ActivityType,
    string Description,
    string? IpAddress,
    string? UserAgent,
    DateTime OccurredAtUtc,
    Dictionary<string, object>? Metadata);