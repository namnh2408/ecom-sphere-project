using MediatR;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.UserRoles;

/// <summary>
/// Command to assign a role to a user
/// </summary>
public record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId
) : IRequest<Result<Unit>>;
