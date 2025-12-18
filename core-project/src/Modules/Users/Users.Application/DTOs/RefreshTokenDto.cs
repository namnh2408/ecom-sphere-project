namespace Users.Application.DTOs;

/// <summary>
/// Refresh token response DTO
/// </summary>
public record RefreshTokenDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);