namespace Users.Application.DTOs;

/// <summary>
/// Data Transfer Object for Role
/// </summary>
public record RoleDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    IReadOnlyList<Guid> PermissionIds,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);