using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Roles;

/// <summary>
/// Command to update an existing role
/// </summary>
public record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string Description
) : IRequest<Result<RoleDto>>;
