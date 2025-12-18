using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Queries;

/// <summary>
/// Query to get all permissions for a specific role
/// </summary>
public record GetPermissionsByRoleIdQuery(Guid RoleId) : IRequest<Result<IEnumerable<PermissionDto>>>;
