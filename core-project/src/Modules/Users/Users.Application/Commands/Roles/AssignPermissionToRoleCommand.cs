using MediatR;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Roles;

/// <summary>
/// Command to assign a permission to a role
/// </summary>
public record AssignPermissionToRoleCommand(
    Guid RoleId,
    Guid PermissionId
) : IRequest<Result<Unit>>;
