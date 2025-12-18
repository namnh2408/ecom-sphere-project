using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Queries;

/// <summary>
/// Query to get user by ID
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;
