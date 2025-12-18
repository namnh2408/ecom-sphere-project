using MediatR;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Roles;

/// <summary>
/// Command to remove a permission from a role
/// </summary>
public record RemovePermissionFromRoleCommand(
    Guid RoleId,
    Guid PermissionId
) : IRequest<Result<Unit>>;
