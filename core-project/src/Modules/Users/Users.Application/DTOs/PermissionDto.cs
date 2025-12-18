namespace Users.Application.DTOs;

/// <summary>
/// Data Transfer Object for Permission
/// </summary>
public record PermissionDto(
    Guid Id,
    string Name,
    string Description,
    string Resource,
    string Action,
    bool IsActive,
    DateTime CreatedAtUtc
);