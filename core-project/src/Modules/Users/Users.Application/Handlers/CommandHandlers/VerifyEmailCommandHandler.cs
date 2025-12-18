using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Auth;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result<bool>>
{
    private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(
        IEmailVerificationTokenRepository emailVerificationTokenRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _emailVerificationTokenRepository = emailVerificationTokenRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var token = await _emailVerificationTokenRepository.GetByTokenAsync(request.Token, cancellationToken);
        if (token == null)
        {
            return Result<bool>.Fail("INVALID_TOKEN", "Verification token is invalid or not found");
        }

        if (!token.IsValid)
        {
            if (token.IsExpired)
            {
                return Result<bool>.Fail("TOKEN_EXPIRED", "Verification token has expired");
            }
            if (token.IsVerified)
            {
                return Result<bool>.Fail("TOKEN_ALREADY_USED", "Verification token has already been used");
            }
        }

        var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
        if (user == null)
        {
            return Result<bool>.Fail("USER_NOT_FOUND", "User not found");
        }

        // Verify email
        user.VerifyEmail();
        token.Verify();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
