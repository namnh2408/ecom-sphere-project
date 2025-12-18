using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Roles;

/// <summary>
/// Command to create a new role
/// </summary>
public record CreateRoleCommand(
    string Name,
    string Description
) : IRequest<Result<RoleDto>>;
