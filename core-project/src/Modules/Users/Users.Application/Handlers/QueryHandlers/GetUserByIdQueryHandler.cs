using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Queries;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<UserDto>.Fail("USER_NOT_FOUND", $"User with ID '{request.UserId}' was not found");
        }

        var userDto = new UserDto(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.IsEmailVerified,
            user.RoleIds,
            user.CreatedAtUtc,
            user.UpdatedAtUtc,
            user.LastLoginAtUtc
        );

        return Result<UserDto>.Success(userDto);
    }
}