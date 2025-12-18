namespace Users.Application.DTOs;

public record ChangeHistoryDto(
    Guid Id,
    Guid UserId,
    string FieldName,
    string? OldValue,
    string? NewValue,
    string ChangeReason,
    string? IpAddress,
    DateTime ChangedAtUtc,
    bool IsReversible);