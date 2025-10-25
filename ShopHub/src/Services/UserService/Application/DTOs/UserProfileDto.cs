namespace ShopHub.Services.UserService.Application.DTOs;

/// <summary>
/// DTO cho profile người dùng
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Tên đầy đủ
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// URL ảnh đại diện
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Địa chỉ
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Thành phố
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Quốc gia
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Trạng thái kích hoạt
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Email đã xác minh
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Thời gian login lần cuối
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}