using MediatR;
using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Roles;

/// <summary>
/// Command to delete a role
/// </summary>
public record DeleteRoleCommand(
    Guid RoleId
) : IRequest<Result<Unit>>;