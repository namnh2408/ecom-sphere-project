namespace ShopHub.Services.UserService.Application.DTOs;

/// <summary>
/// DTO cho login
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Email người dùng
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu
    /// </summary>
    public string Password { get; set; } = string.Empty;
}