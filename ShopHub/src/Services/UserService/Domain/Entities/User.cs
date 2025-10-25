using ShopHub.Domain.Base;
using ShopHub.Domain.Abstractions;

namespace ShopHub.Services.UserService.Domain.Entities;

/// <summary>
/// Aggregate Root: User
/// Quản lý thông tin người dùng và xác thực
/// </summary>
public class User : BaseAggregateRoot
{
    /// <summary>
    /// Email người dùng (unique)
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Tên người dùng
    /// </summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>
    /// Password hash (BCrypt)
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// URL ảnh đại diện
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Địa chỉ
    /// </summary>
    public string? Address { get; private set; }

    /// <summary>
    /// Thành phố
    /// </summary>
    public string? City { get; private set; }

    /// <summary>
    /// Quốc gia
    /// </summary>
    public string? Country { get; private set; }

    /// <summary>
    /// Trạng thái kích hoạt
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Email đã được xác minh
    /// </summary>
    public bool IsEmailVerified { get; private set; }

    /// <summary>
    /// Thời gian login lần cuối
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Refresh token hiện tại
    /// </summary>
    public string? RefreshToken { get; private set; }

    /// <summary>
    /// Thời gian hết hạn refresh token
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    /// <summary>
    /// Constructor private
    /// </summary>
    private User()
    {
    }

    /// <summary>
    /// Factory method để tạo người dùng mới
    /// </summary>
    public static User Create(
        string email,
        string fullName,
        string passwordHash,
        string? phoneNumber = null,
        string? avatarUrl = null)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty", nameof(fullName));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLower(),
            FullName = fullName,
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber,
            AvatarUrl = avatarUrl,
            IsActive = true,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            RefreshToken = null,
            RefreshTokenExpiresAt = null
        };

        // Add domain event
        user.AddDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.Email,
            user.FullName));

        return user;
    }

    /// <summary>
    /// Cập nhật profil người dùng
    /// </summary>
    public void UpdateProfile(
        string? fullName = null,
        string? phoneNumber = null,
        string? avatarUrl = null,
        string? address = null,
        string? city = null,
        string? country = null)
    {
        if (fullName != null && string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty", nameof(fullName));

        if (fullName != null)
            FullName = fullName;

        if (phoneNumber != null)
            PhoneNumber = phoneNumber;

        if (avatarUrl != null)
            AvatarUrl = avatarUrl;

        if (address != null)
            Address = address;

        if (city != null)
            City = city;

        if (country != null)
            Country = country;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật refresh token
    /// </summary>
    public void SetRefreshToken(string token, DateTime expiresAt)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cập nhật thời gian login lần cuối
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Xác minh email
    /// </summary>
    public void VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate tài khoản
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate tài khoản
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Xóa refresh token (logout)
    /// </summary>
    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Domain Event: User Registered
/// </summary>
public class UserRegisteredEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public int Version => 1;

    public Guid UserId { get; }
    public string Email { get; }
    public string FullName { get; }

    public UserRegisteredEvent(Guid userId, string email, string fullName)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        UserId = userId;
        Email = email;
        FullName = fullName;
    }
}