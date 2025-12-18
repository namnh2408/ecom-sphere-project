using MediatR;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.UserRoles;

/// <summary>
/// Command to remove a role from a user
/// </summary>
public record RemoveRoleFromUserCommand(
    Guid UserId,
    Guid RoleId
) : IRequest<Result<Unit>>;
