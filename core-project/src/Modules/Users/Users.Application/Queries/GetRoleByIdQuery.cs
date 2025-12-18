using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Queries;

/// <summary>
/// Query to get role by ID
/// </summary>
public record GetRoleByIdQuery(Guid RoleId) : IRequest<Result<RoleDto>>;
