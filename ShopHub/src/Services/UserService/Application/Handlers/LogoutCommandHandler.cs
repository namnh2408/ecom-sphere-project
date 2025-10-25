using MediatR;
using ShopHub.Common.Results;
using ShopHub.Domain.Abstractions;
using ShopHub.Domain.CQRS;
using ShopHub.Services.UserService.Application.Commands;
using ShopHub.Services.UserService.Domain.Repositories;

namespace ShopHub.Services.UserService.Application.Handlers;

/// <summary>
/// Handler cho LogoutCommand
/// </summary>
public class LogoutCommandHandler : ICommandHandler<LogoutCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<LogoutCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("User logout: {UserId}", request.UserId);

            // Get user
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Logout failed: User {UserId} not found", request.UserId);
                return Result<bool>.Failure($"User with ID {request.UserId} not found");
            }

            // Clear refresh token (logout)
            user.ClearRefreshToken();

            // Update user
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User logged out successfully: {UserId}", request.UserId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Result<bool>.Failure($"An error occurred during logout: {ex.Message}");
        }
    }
}