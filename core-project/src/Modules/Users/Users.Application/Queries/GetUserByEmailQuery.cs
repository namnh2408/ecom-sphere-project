using MediatR;
using Users.Application.DTOs;

using BuildingBlocks.Abstractions;

namespace Users.Application.Queries;

/// <summary>
/// Query to get user by email
/// </summary>
public record GetUserByEmailQuery(string Email) : IRequest<Result<UserDto>>;
