using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Auth;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Result<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            // For security reasons, don't reveal if email exists
            // Return success anyway
            return Result<string>.Success("Password reset link has been sent to your email");
        }

        if (!user.IsActive)
        {
            return Result<string>.Success("Password reset link has been sent to your email");
        }

        // Create new reset token
        var resetToken = Users.Domain.Entities.PasswordResetToken.Create(user.Id);
        await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);

        // Save changes (Unit of Work pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // In a real application, you would send an email here with the reset token
        // For now, we return the token
        return Result<string>.Success(resetToken.Token);
    }
}
