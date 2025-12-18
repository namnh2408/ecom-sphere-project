namespace Users.Application.DTOs;

/// <summary>
/// Email verification request DTO
/// </summary>
public record EmailVerificationDto(
    string Email,
    string VerificationToken
);