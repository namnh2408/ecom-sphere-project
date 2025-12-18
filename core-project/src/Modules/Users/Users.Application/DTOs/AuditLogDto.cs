namespace Users.Application.DTOs;

public record AuditLogDto(
    Guid Id,
    Guid UserId,
    string EntityName,
    Guid EntityId,
    string OperationType,
    string? OldValues,
    string? NewValues,
    string? Description,
    string? IpAddress,
    DateTime OccurredAtUtc);