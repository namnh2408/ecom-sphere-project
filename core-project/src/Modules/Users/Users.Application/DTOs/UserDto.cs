namespace Users.Application.DTOs;

/// <summary>
/// Data Transfer Object for User
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    bool IsEmailVerified,
    IReadOnlyList<Guid> RoleIds,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? LastLoginAtUtc
);