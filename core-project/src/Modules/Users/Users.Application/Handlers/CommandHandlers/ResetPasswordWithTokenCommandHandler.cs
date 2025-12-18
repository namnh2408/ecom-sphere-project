using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Auth;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class ResetPasswordWithTokenCommandHandler : IRequestHandler<ResetPasswordWithTokenCommand, Result<bool>>
{
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordWithTokenCommandHandler(
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ResetPasswordWithTokenCommand request, CancellationToken cancellationToken)
    {
        var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(request.Token, cancellationToken);
        if (resetToken == null)
        {
            return Result<bool>.Fail("INVALID_TOKEN", "Reset token is invalid or not found");
        }

        if (!resetToken.IsValid)
        {
            if (resetToken.IsExpired)
            {
                return Result<bool>.Fail("TOKEN_EXPIRED", "Reset token has expired");
            }
            if (resetToken.IsUsed)
            {
                return Result<bool>.Fail("TOKEN_ALREADY_USED", "Reset token has already been used");
            }
        }

        var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken);
        if (user == null)
        {
            return Result<bool>.Fail("USER_NOT_FOUND", "User not found");
        }

        // Reset password
        var passwordResult = user.ResetPassword(request.NewPassword);
        if (passwordResult.IsFailure)
        {
            return Result<bool>.Fail(passwordResult.Error!.Code, passwordResult.Error.Message);
        }

        resetToken.MarkAsUsed();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
