using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Queries;

/// <summary>
/// Query to get all roles
/// </summary>
public record GetAllRolesQuery : IRequest<Result<IEnumerable<RoleDto>>>;
