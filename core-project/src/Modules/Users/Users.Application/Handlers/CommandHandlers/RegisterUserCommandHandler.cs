using BuildingBlocks.Abstractions;
using MediatR;
using Users.Application.Commands.Auth;
using Users.Application.DTOs;
using Users.Domain.Entities;
using Users.Domain.Exceptions;
using Users.Domain.Repositories;
using Users.Domain.Services;

namespace Users.Application.Handlers.CommandHandlers;

/// <summary>
/// Handler for RegisterUserCommand
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthTokenDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerationService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        ITokenGenerationService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthTokenDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken);
        if (existingUser)
        {
            return Result<AuthTokenDto>.Fail("USER_EMAIL_ALREADY_EXISTS", $"User with email '{command.Email}' already exists");
        }

        // Create user
        var userResult = User.Create(command.Email, command.Password, command.FirstName, command.LastName);
        if (userResult.IsFailure)
        {
            return Result<AuthTokenDto>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var user = userResult.Value!;
        
        // Add user to repository
        await _userRepository.AddAsync(user, cancellationToken);
        
        // Commit transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate token
        var token = _tokenService.GenerateToken(user.Id, user.Email.Value, user.RoleIds);

        var result = new AuthTokenDto(token, "Bearer", 60, user.Id, user.Email.Value);
        return Result<AuthTokenDto>.Success(result);
    }
}
