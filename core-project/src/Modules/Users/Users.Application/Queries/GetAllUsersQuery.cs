using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;

namespace Users.Application.Queries;

/// <summary>
/// Query to get all users with pagination
/// </summary>
public record GetAllUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null,
    bool? IsEmailVerified = null
) : IRequest<Result<PaginatedResult<UserDto>>>;