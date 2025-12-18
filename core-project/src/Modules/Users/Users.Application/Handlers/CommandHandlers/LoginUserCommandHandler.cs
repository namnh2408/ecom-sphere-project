using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Auth;
using Users.Application.DTOs;
using Users.Domain.Exceptions;
using Users.Domain.Repositories;
using Users.Domain.Services;

namespace Users.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for LoginUserCommand
/// </summary>
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthTokenDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerationService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        ITokenGenerationService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthTokenDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            return Result<AuthTokenDto>.Fail("INVALID_CREDENTIALS", "Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<AuthTokenDto>.Fail("USER_INACTIVE", "User account is inactive");
        }

        // Verify password
        if (!user.Password.VerifyPassword(command.Password))
        {
            return Result<AuthTokenDto>.Fail("INVALID_CREDENTIALS", "Invalid email or password");
        }

        // Record login
        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate token
        var token = _tokenService.GenerateToken(user.Id, user.Email.Value, user.RoleIds);

        var result = new AuthTokenDto(token, "Bearer", 60, user.Id, user.Email.Value);
        return Result<AuthTokenDto>.Success(result);
    }
}
