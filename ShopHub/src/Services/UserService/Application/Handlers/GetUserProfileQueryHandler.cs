using MediatR;
using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Services.UserService.Application.DTOs;
using ShopHub.Services.UserService.Application.Queries;
using ShopHub.Services.UserService.Domain.Repositories;

namespace ShopHub.Services.UserService.Application.Handlers;

/// <summary>
/// Handler cho GetUserProfileQuery
/// </summary>
public class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching user profile for User ID: {UserId}", request.UserId);

            // Get user from repository
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User profile not found: {UserId}", request.UserId);
                return Result<UserProfileDto>.Failure($"User with ID {request.UserId} not found");
            }

            _logger.LogInformation("User profile fetched successfully: {UserId}", request.UserId);

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
            _logger.LogError(ex, "Error fetching user profile");
            return Result<UserProfileDto>.Failure($"An error occurred: {ex.Message}");
        }
    }
}