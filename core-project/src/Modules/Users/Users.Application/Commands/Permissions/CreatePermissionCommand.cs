using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Permissions;

/// <summary>
/// Command to create a new permission
/// </summary>
public record CreatePermissionCommand(
    string Name,
    string Description,
    string Resource,
    string Action
) : IRequest<Result<PermissionDto>>;
