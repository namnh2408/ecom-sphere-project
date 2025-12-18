namespace Users.Application.DTOs;

public record LoginHistoryDto(
    Guid Id,
    string Email,
    bool IsSuccessful,
    string? FailureReason,
    string? IpAddress,
    string? UserAgent,
    DateTime AttemptedAtUtc);