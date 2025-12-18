namespace Users.Application.DTOs;

/// <summary>
/// Data Transfer Object for authentication tokens
/// </summary>
public record AuthTokenDto(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    Guid UserId,
    string Email
);