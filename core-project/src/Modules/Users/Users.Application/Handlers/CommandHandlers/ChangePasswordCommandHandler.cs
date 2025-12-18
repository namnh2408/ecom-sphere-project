using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Auth;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<Unit>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Fail("USER_NOT_FOUND", $"User with ID '{command.UserId}' was not found");
        }

        var result = user.ChangePassword(command.OldPassword, command.NewPassword);
        if (result.IsFailure)
        {
            return Result<Unit>.Fail(result.Error!.Code, result.Error.Message);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
