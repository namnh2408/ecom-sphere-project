using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Queries;

/// <summary>
/// Query to get all permissions
/// </summary>
public record GetAllPermissionsQuery : IRequest<Result<IEnumerable<PermissionDto>>>;
