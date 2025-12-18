using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.DTOs;
using Users.Application.Queries;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.QueryHandlers;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result<UserDto>.Fail("USER_NOT_FOUND", $"User with email '{request.Email}' was not found");
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