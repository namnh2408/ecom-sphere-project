using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Services.UserService.Application.Commands;
using ShopHub.Services.UserService.Application.DTOs;
using ShopHub.Services.UserService.Application.Queries;
using System.Security.Claims;

namespace ShopHub.Services.UserService.Presentation.Controllers;

/// <summary>
/// Controller cho User operations (Authentication & Profile)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Đăng ký người dùng mới
    /// </summary>
    /// <param name="dto">Thông tin đăng ký</param>
    /// <returns>Profile của người dùng vừa tạo</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileDto>> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("Register request for email: {Email}", dto.Email);

            var command = new RegisterCommand(
                dto.Email,
                dto.FullName,
                dto.Password,
                dto.ConfirmPassword,
                dto.PhoneNumber);

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Registration failed: {Message}", result.Message);
                return Conflict(new { message = result.Message });
            }

            _logger.LogInformation("User registered successfully: {UserId}", result.Data!.UserId);

            return CreatedAtAction(nameof(GetProfile), new { userId = result.Data.UserId }, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Login người dùng
    /// </summary>
    /// <param name="dto">Email và password</param>
    /// <returns>JWT token và refresh token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var query = new LoginQuery(dto.Email, dto.Password);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login failed: {Message}", result.Message);
                return Unauthorized(new { message = result.Message });
            }

            _logger.LogInformation("User logged in successfully: {UserId}", result.Data!.UserId);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Logout người dùng
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Logout()
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Logout failed: Invalid or missing user ID claim");
                return Unauthorized(new { message = "Invalid user ID" });
            }

            _logger.LogInformation("Logout request for user: {UserId}", userId);

            var command = new LogoutCommand(userId);
            var result = await _mediator.Send(command);

            _logger.LogInformation("User logged out successfully: {UserId}", userId);

            return Ok(new { message = "Logged out successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Logout failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Lấy profile của người dùng hiện tại
    /// </summary>
    /// <returns>Profile của người dùng</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetCurrentProfile()
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Get profile failed: Invalid or missing user ID claim");
                return Unauthorized(new { message = "Invalid user ID" });
            }

            return await GetProfile(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current profile");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Lấy profile của một người dùng cụ thể
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Profile của người dùng</returns>
    [HttpGet("{userId}")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetProfile([FromRoute] Guid userId)
    {
        try
        {
            _logger.LogInformation("Fetch profile for user: {UserId}", userId);

            var query = new GetUserProfileQuery(userId);
            var result = await _mediator.Send(query);

            _logger.LogInformation("User profile fetched successfully: {UserId}", userId);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Get profile failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching profile for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error" });
        }
    }
}