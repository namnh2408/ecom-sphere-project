using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using Gateway.WebApi.Models;
using Users.Application.Commands.Auth;
using Users.Application.DTOs;
using Users.Application.Queries;

namespace Gateway.WebApi.Controllers;

/// <summary>
/// Authentication operations including user registration, login, password management, and token refresh
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
[Produces("application/json")]
[Consumes("application/json")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <remarks>
    /// Creates a new user account with the provided email and password.
    /// 
    /// **Requirements:**
    /// - Email must be valid and unique
    /// - Password must be at least 8 characters with uppercase, lowercase, and numeric characters
    /// 
    /// **Response:**
    /// Returns JWT access token and refresh token for immediate authentication
    /// </remarks>
    /// <param name="command">Registration details including email and password</param>
    /// <returns>Authentication tokens (accessToken, refreshToken, expiresIn)</returns>
    /// <response code="201">User registered successfully, returns JWT tokens</response>
    /// <response code="400">Invalid input or email already exists</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Created($"/api/users/{result.Value!.UserId}", result.Value);
    }

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <remarks>
    /// Validates user credentials and returns JWT tokens for authenticated access.
    /// 
    /// **Security Features:**
    /// - Failed login attempts are tracked and rate-limited
    /// - Account may be locked after multiple failed attempts
    /// - Password is never returned in response
    /// 
    /// **Response:**
    /// Returns JWT access token (short-lived) and refresh token (long-lived)
    /// </remarks>
    /// <param name="command">User email and password</param>
    /// <returns>Authentication tokens on successful login</returns>
    /// <response code="200">Login successful, returns JWT tokens</response>
    /// <response code="401">Invalid credentials or account locked</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return Unauthorized(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Change the current user's password
    /// </summary>
    /// <remarks>
    /// Allows authenticated users to change their password. Requires verification of the current password.
    /// 
    /// **Requirements:**
    /// - User must be authenticated (JWT token required)
    /// - Current password must be correct
    /// - New password must meet strength requirements (8+ chars, uppercase, lowercase, numeric)
    /// - New password must differ from current password
    /// 
    /// **Security:**
    /// - Old refresh tokens are invalidated after password change
    /// - User must re-login on all devices
    /// </remarks>
    /// <param name="command">Current password and new password</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password changed successfully</response>
    /// <response code="401">Unauthorized - invalid current password</response>
    /// <response code="400">Invalid new password or validation error</response>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Request email verification token
    /// </summary>
    /// <remarks>
    /// Generates an email verification token for the authenticated user.
    /// 
    /// **Features:**
    /// - Token is valid for 24 hours
    /// - Only one token can be active at a time
    /// - Previous tokens are automatically revoked
    /// - Token must be verified using the /verify-email endpoint
    /// 
    /// **Use Cases:**
    /// - User registered with unverified email
    /// - Email verification token expired
    /// - User changed email address
    /// </remarks>
    /// <param name="command">Email verification request</param>
    /// <returns>Verification token details</returns>
    /// <response code="200">Verification token generated successfully</response>
    /// <response code="400">Email already verified</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">User not found</response>
    [Authorize]
    [HttpPost("request-email-verification")]
    [ProducesResponseType(typeof(EmailVerificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestEmailVerification([FromBody] RequestEmailVerificationCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error!.Code == "EMAIL_ALREADY_VERIFIED")
                return BadRequest(new { error = result.Error.Code, message = result.Error.Message });
            
            return NotFound(new { error = result.Error.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Verify email address using verification token
    /// </summary>
    /// <remarks>
    /// Confirms user's email address by validating the verification token.
    /// 
    /// **Requirements:**
    /// - Valid verification token (received from /request-email-verification)
    /// - Token must not be expired (24 hours validity)
    /// - Each token can be used only once
    /// 
    /// **After Verification:**
    /// - User's email is marked as verified
    /// - No further verification needed for this email
    /// - If changing email later, verification will be required again
    /// </remarks>
    /// <param name="command">Verification token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Email verified successfully</response>
    /// <response code="400">Invalid or expired token</response>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Email verified successfully" });
    }

    /// <summary>
    /// Request password reset token
    /// </summary>
    /// <remarks>
    /// Initiates password reset flow by sending a reset token to the user's email.
    /// 
    /// **Security Features:**
    /// - Always returns success (doesn't confirm if email exists)
    /// - Token is valid for 60 minutes only
    /// - Token can only be used once
    /// - Previous tokens are automatically invalidated
    /// - Can be called multiple times (new token replaces old one)
    /// 
    /// **Flow:**
    /// 1. User requests reset with their email
    /// 2. Reset token is sent to email address
    /// 3. User uses token with /reset-password endpoint
    /// </remarks>
    /// <param name="command">Email address for password reset</param>
    /// <returns>Success message (always returned for security)</returns>
    /// <response code="200">Password reset email sent (if email exists)</response>
    [HttpPost("request-password-reset")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetCommand command)
    {
        var result = await mediator.Send(command);

        // Always return success for security reasons
        if (result.IsFailure)
        {
            return Ok(new { message = "Password reset link has been sent to your email" });
        }

        return Ok(new { message = result.Value, resetToken = result.Value });
    }

    /// <summary>
    /// Reset password using reset token
    /// </summary>
    /// <remarks>
    /// Completes the password reset process by applying a new password with a valid reset token.
    /// 
    /// **Requirements:**
    /// - Valid reset token (received from /request-password-reset)
    /// - Token must not be expired (60 minutes validity)
    /// - New password must meet strength requirements
    /// - Each token can be used only once
    /// 
    /// **After Reset:**
    /// - All existing refresh tokens are invalidated
    /// - User must re-login on all devices
    /// - New password takes effect immediately
    /// </remarks>
    /// <param name="command">Reset token and new password</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Invalid token, expired token, or password validation error</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordWithTokenCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(new { message = "Password reset successfully" });
    }

    /// <summary>
    /// Refresh expired JWT access token
    /// </summary>
    /// <remarks>
    /// Obtains a new access token using a valid refresh token.
    /// 
    /// **Token Features:**
    /// - Access Token: Short-lived (15-30 minutes typical)
    /// - Refresh Token: Long-lived (7-30 days typical)
    /// - Automatic Token Rotation: Old tokens are revoked when new ones are issued
    /// 
    /// **Typical Flow:**
    /// 1. User receives accessToken + refreshToken from /login
    /// 2. When accessToken expires, call /refresh-token with refreshToken
    /// 3. Receive new accessToken + refreshToken
    /// 4. Continue using new accessToken
    /// 
    /// **Security Considerations:**
    /// - Refresh tokens should be stored securely (HttpOnly cookies recommended)
    /// - Token rotation helps prevent token hijacking
    /// - Expired refresh tokens cannot be reused
    /// </remarks>
    /// <param name="command">Refresh token</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return Unauthorized(new { error = result.Error!.Code, message = result.Error.Message });
        }

        return Ok(result.Value);
    }
}
