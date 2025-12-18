using BuildingBlocks.Abstractions;
using MediatR;
using System.Security.Claims;
using Users.Application.Commands.Auth;
using Users.Application.DTOs;
using Users.Domain.Repositories;
using Users.Domain.Services;

namespace Users.Application.Handlers.CommandHandlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenDto>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerationService _tokenGenerationService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenGenerationService tokenGenerationService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenGenerationService = tokenGenerationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshTokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken == null)
        {
            return Result<RefreshTokenDto>.Fail("INVALID_TOKEN", "Refresh token is invalid or not found");
        }

        if (!refreshToken.IsValid)
        {
            if (refreshToken.IsExpired)
            {
                return Result<RefreshTokenDto>.Fail("TOKEN_EXPIRED", "Refresh token has expired");
            }
            if (refreshToken.IsRevoked)
            {
                return Result<RefreshTokenDto>.Fail("TOKEN_REVOKED", "Refresh token has been revoked");
            }
        }

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user == null)
        {
            return Result<RefreshTokenDto>.Fail("USER_NOT_FOUND", "User not found");
        }

        if (!user.IsActive)
        {
            return Result<RefreshTokenDto>.Fail("USER_INACTIVE", "User account is inactive");
        }

        // Generate new access token
        var newAccessToken = _tokenGenerationService.GenerateToken(user.Id, user.Email.Value, user.RoleIds, expiresIn: 60);

        // Generate new refresh token
        var newRefreshToken = Users.Domain.Entities.RefreshToken.Create(user.Id);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        // Revoke old refresh token
        refreshToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new RefreshTokenDto(newAccessToken, newRefreshToken.Token, 60);
        return Result<RefreshTokenDto>.Success(dto);
    }
}
