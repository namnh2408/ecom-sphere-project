using MediatR;
using ShopHub.Common.Results;
using ShopHub.Domain.Abstractions;
using ShopHub.Domain.CQRS;
using ShopHub.Services.UserService.Application.Commands;
using ShopHub.Services.UserService.Application.DTOs;
using ShopHub.Services.UserService.Domain.Entities;
using ShopHub.Services.UserService.Domain.Repositories;

namespace ShopHub.Services.UserService.Application.Handlers;

/// <summary>
/// Handler cho RegisterCommand
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registering new user with email: {Email}", request.Email);

            // Check if email already exists
            var emailExists = await _userRepository.EmailExistsAsync(request.Email, cancellationToken);
            if (emailExists)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
                return Result<UserProfileDto>.Failure($"Email {request.Email} is already registered");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = User.Create(
                request.Email,
                request.FullName,
                passwordHash,
                request.PhoneNumber);

            // Add to repository
            await _userRepository.AddAsync(user, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User registered successfully: {UserId}", user.Id);

            // Return profile
            var profile = new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Result<UserProfileDto>.Success(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return Result<UserProfileDto>.Failure($"An error occurred during registration: {ex.Message}");
        }
    }
}