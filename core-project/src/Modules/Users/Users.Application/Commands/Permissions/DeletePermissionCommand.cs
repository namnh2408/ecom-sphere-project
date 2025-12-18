using MediatR;

using BuildingBlocks.Abstractions;

namespace Users.Application.Commands.Permissions;

/// <summary>
/// Command to delete a permission
/// </summary>
public record DeletePermissionCommand(
    Guid PermissionId
) : IRequest<Result<Unit>>;
