using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Auth;
using Users.Application.DTOs;
using Users.Domain.Repositories;

namespace Users.Application.Handlers.CommandHandlers;

public class RequestEmailVerificationCommandHandler : IRequestHandler<RequestEmailVerificationCommand, Result<EmailVerificationDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestEmailVerificationCommandHandler(
        IUserRepository userRepository,
        IEmailVerificationTokenRepository emailVerificationTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailVerificationTokenRepository = emailVerificationTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EmailVerificationDto>> Handle(RequestEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<EmailVerificationDto>.Fail("USER_NOT_FOUND", "User not found");
        }

        if (user.IsEmailVerified)
        {
            return Result<EmailVerificationDto>.Fail("EMAIL_ALREADY_VERIFIED", "Email is already verified");
        }

        // Invalidate previous tokens
        var previousToken = await _emailVerificationTokenRepository.GetPendingByUserIdAsync(user.Id, cancellationToken);
        
        // Create new verification token
        var verificationToken = Users.Domain.Entities.EmailVerificationToken.Create(user.Id);
        await _emailVerificationTokenRepository.AddAsync(verificationToken, cancellationToken);

        // Save changes (Unit of Work pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new EmailVerificationDto(user.Email.Value, verificationToken.Token);
        return Result<EmailVerificationDto>.Success(dto);
    }
}
