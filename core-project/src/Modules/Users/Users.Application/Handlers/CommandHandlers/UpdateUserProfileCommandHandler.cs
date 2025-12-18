using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Users;
using Users.Application.DTOs;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<UserDto>.Fail("USER_NOT_FOUND", $"User with ID '{command.UserId}' was not found");
        }

        user.UpdateProfile(command.FirstName, command.LastName);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
