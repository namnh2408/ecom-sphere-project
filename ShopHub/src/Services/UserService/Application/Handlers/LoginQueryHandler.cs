using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using ShopHub.Common.Results;
using ShopHub.Domain.CQRS;
using ShopHub.Services.UserService.Application.DTOs;
using ShopHub.Services.UserService.Application.Queries;
using ShopHub.Services.UserService.Domain.Repositories;

namespace ShopHub.Services.UserService.Application.Handlers;

/// <summary>
/// Handler cho LoginQuery
/// </summary>
public class LoginQueryHandler : IQueryHandler<LoginQuery, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginQueryHandler> _logger;

    public LoginQueryHandler(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<LoginQueryHandler> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("User login attempt: {Email}", request.Email);

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Email} not found", request.Email);
                return Result<LoginResponseDto>.Failure("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {Email} is inactive", request.Email);
                return Result<LoginResponseDto>.Failure("User account is inactive");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
                return Result<LoginResponseDto>.Failure("Invalid email or password");
            }

            // Generate tokens
            var accessToken = GenerateAccessToken(user.Id, user.Email);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // 7 days

            // Update user with refresh token and last login
            user.SetRefreshToken(refreshToken, refreshTokenExpiry);
            user.UpdateLastLogin();
            
            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            // Return login response
            var response = new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                TokenType = "Bearer"
            };

            return Result<LoginResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return Result<LoginResponseDto>.Failure($"An error occurred during login: {ex.Message}");
        }
    }

    /// <summary>
    /// Tạo Access Token (JWT)
    /// </summary>
    private string GenerateAccessToken(Guid userId, string email)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("UserId", userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Tạo Refresh Token (random string)
    /// </summary>
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}