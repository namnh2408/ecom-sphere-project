using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Queries;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.QueryHandlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PaginatedResult<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PaginatedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);

        // Apply filters
        var filtered = users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            filtered = filtered.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Email.Value.ToLower().Contains(searchTerm)
            );
        }

        if (request.IsActive.HasValue)
        {
            filtered = filtered.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (request.IsEmailVerified.HasValue)
        {
            filtered = filtered.Where(u => u.IsEmailVerified == request.IsEmailVerified.Value);
        }

        var totalCount = filtered.Count();

        // Apply pagination
        var paginatedUsers = filtered
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to DTOs
        var userDtos = paginatedUsers.Select(u => new UserDto(
            u.Id,
            u.Email.Value,
            u.FirstName,
            u.LastName,
            u.IsActive,
            u.IsEmailVerified,
            u.RoleIds,
            u.CreatedAtUtc,
            u.UpdatedAtUtc,
            u.LastLoginAtUtc
        )).ToList();

        var paginatedResult = PaginatedResult<UserDto>.Create(
            userDtos,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedResult<UserDto>>.Success(paginatedResult);
    }
}