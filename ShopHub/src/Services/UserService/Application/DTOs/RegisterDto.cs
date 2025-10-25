namespace ShopHub.Services.UserService.Application.DTOs;

/// <summary>
/// DTO cho đăng ký người dùng
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Email người dùng
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Tên đầy đủ
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu (plain text)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Xác nhận mật khẩu
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }
}