using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShopHub.Services.UserService.Domain.Entities;

namespace ShopHub.Services.UserService.Tests.Domain;

/// <summary>
/// Unit tests cho User entity
/// </summary>
[TestClass]
public class UserTests
{
    private Guid _userId;
    private string _email;
    private string _fullName;
    private string _passwordHash;

    [TestInitialize]
    public void Setup()
    {
        _userId = Guid.NewGuid();
        _email = "user@example.com";
        _fullName = "John Doe";
        _passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123");
    }

    [TestMethod]
    public void User_Create_WithRequiredFields_CreatesUserSuccessfully()
    {
        // Act
        var user = User.Create(_email, _fullName, _passwordHash, "+84912345678");

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual(_email, user.Email);
        Assert.AreEqual(_fullName, user.FullName);
        Assert.AreEqual(_passwordHash, user.PasswordHash);
        Assert.AreEqual("+84912345678", user.PhoneNumber);
        Assert.IsTrue(user.IsActive);
        Assert.IsFalse(user.IsEmailVerified);
    }

    [TestMethod]
    public void User_Create_WithoutPhoneNumber_CreatesUserSuccessfully()
    {
        // Act
        var user = User.Create(_email, _fullName, _passwordHash);

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual(_email, user.Email);
        Assert.AreEqual(_fullName, user.FullName);
        Assert.IsTrue(user.IsActive);
    }

    [TestMethod]
    public void User_CreateFactory_SetsAllProperties()
    {
        // Act
        var user = User.Create(_email, _fullName, _passwordHash, "+84912345678");

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual(_email.ToLower(), user.Email);
        Assert.AreEqual(_fullName, user.FullName);
        Assert.AreEqual(_passwordHash, user.PasswordHash);
        Assert.AreEqual("+84912345678", user.PhoneNumber);
    }

    [TestMethod]
    public void Activate_WithDeactivatedUser_ActivatesUser()
    {
        // Arrange
        var user = User.Create(_email, _fullName, _passwordHash);
        user.Deactivate();
        Assert.IsFalse(user.IsActive);

        // Act
        user.Activate();

        // Assert
        Assert.IsTrue(user.IsActive);
    }

    [TestMethod]
    public void Deactivate_WithActiveUser_DeactivatesUser()
    {
        // Arrange
        var user = User.Create(_email, _fullName, _passwordHash);
        Assert.IsTrue(user.IsActive);

        // Act
        user.Deactivate();

        // Assert
        Assert.IsFalse(user.IsActive);
    }

    [TestMethod]
    public void VerifyEmail_SetsEmailVerifiedToTrue()
    {
        // Arrange
        var user = User.Create(_email, _fullName, _passwordHash);
        Assert.IsFalse(user.IsEmailVerified);

        // Act
        user.VerifyEmail();

        // Assert
        Assert.IsTrue(user.IsEmailVerified);
    }

    [TestMethod]
    public void UpdateProfile_UpdatesAllProfileFields()
    {
        // Arrange
        var user = User.Create(_email, _fullName, _passwordHash);
        var address = "123 Main Street";
        var city = "Ho Chi Minh";
        var country = "Vietnam";
        var avatarUrl = "https://example.com/avatar.jpg";

        // Act
        user.UpdateProfile(null, null, avatarUrl, address, city, country);

        // Assert
        Assert.AreEqual(address, user.Address);
        Assert.AreEqual(city, user.City);
        Assert.AreEqual(country, user.Country);
        Assert.AreEqual(avatarUrl, user.AvatarUrl);
    }

    [TestMethod]
    public void UpdateProfile_WithPartialData_UpdatesPartialFields()
    {
        // Arrange
        var user = User.Create(_email, _fullName, _passwordHash);

        // Act
        user.UpdateProfile(null, null, null, "Address 1", "Bangkok", null);

        // Assert
        Assert.AreEqual("Address 1", user.Address);
        Assert.AreEqual("Bangkok", user.City);
    }

    [TestMethod]
    public void SetRefreshToken_SetsTokenAndExpiry()
    {
        // Arrange
        var user = User.Create(_email, _fullName, _passwordHash);
        var token = "refresh_token_value";
        var expiryTime = DateTime.UtcNow.AddDays(7);

        // Act
        user.SetRefreshToken(token, expiryTime);

        // Assert
        Assert.AreEqual(token, user.RefreshToken);
        Assert.AreEqual(expiryTime, user.RefreshTokenExpiryTime);
    }

    [TestMethod]
    public void ClearRefreshToken_RemovesTokenAndExpiry()
    {
        // Arrange
        var user = User.Create(_email, _fullName, _passwordHash);
        user.SetRefreshToken("token", DateTime.UtcNow.AddDays(7));

        // Act
        user.ClearRefreshToken();

        // Assert
        Assert.IsNull(user.RefreshToken);
        Assert.IsNull(user.RefreshTokenExpiresAt);
    }

    [TestMethod]
    public void UpdateLastLogin_SetsLastLoginAtToCurrentTime()
    {
        // Arrange
        var user = User.Create(_email, _fullName, _passwordHash);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        user.UpdateLastLogin();

        // Assert
        Assert.IsNotNull(user.LastLoginAt);
        Assert.IsTrue(user.LastLoginAt >= beforeUpdate);
        Assert.IsTrue(user.LastLoginAt <= DateTime.UtcNow.AddSeconds(1));
    }

    [TestMethod]
    public void UserCreation_SetsTimestamps()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var user = User.Create(_email, _fullName, _passwordHash);

        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.IsNotNull(user.CreatedAt);
        Assert.IsTrue(user.CreatedAt >= beforeCreation);
        Assert.IsTrue(user.CreatedAt <= afterCreation);
        Assert.IsNotNull(user.UpdatedAt);
    }

    [TestMethod]
    public void User_ValidEmail_AcceptsEmailAddress()
    {
        // Act
        var user = User.Create("valid.email@domain.co.uk", _fullName, _passwordHash);

        // Assert
        Assert.AreEqual("valid.email@domain.co.uk", user.Email);
    }

    [TestMethod]
    public void User_Create_GeneratesUniqueIds()
    {
        // Act
        var user1 = User.Create(_email, _fullName, _passwordHash);
        var user2 = User.Create("other@example.com", _fullName, _passwordHash);

        // Assert
        Assert.AreNotEqual(user1.Id, user2.Id);
    }

    [TestMethod]
    public void User_Create_PreservesEmailCase()
    {
        // Act
        var email = "User@Example.COM";
        var user = User.Create(email, _fullName, _passwordHash);

        // Assert
        Assert.AreEqual(email, user.Email);
    }

    [TestMethod]
    public void User_PasswordHashIsStored()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act
        var user = User.Create(_email, _fullName, hash);

        // Assert
        Assert.AreEqual(hash, user.PasswordHash);
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
    }

    [TestMethod]
    public void SetRefreshToken_MultipleCalls_OverwritesPreviousToken()
    {
        // Arrange
        var user = User.Create(_email, _fullName, _passwordHash);
        var token1 = "first_token";
        var token2 = "second_token";
        var expiry1 = DateTime.UtcNow.AddDays(7);
        var expiry2 = DateTime.UtcNow.AddDays(14);

        // Act
        user.SetRefreshToken(token1, expiry1);
        user.SetRefreshToken(token2, expiry2);

        // Assert
        Assert.AreEqual(token2, user.RefreshToken);
        Assert.AreEqual(expiry2, user.RefreshTokenExpiresAt);
    }

    [TestMethod]
    public void User_PropertiesInitialState()
    {
        // Act
        var user = User.Create(_email, _fullName, _passwordHash);

        // Assert
        Assert.IsTrue(user.IsActive);
        Assert.IsFalse(user.IsEmailVerified);
        Assert.IsNull(user.RefreshToken);
        Assert.IsNull(user.RefreshTokenExpiresAt);
        Assert.IsNull(user.LastLoginAt);
        Assert.IsNull(user.Address);
        Assert.IsNull(user.City);
        Assert.IsNull(user.Country);
        Assert.IsNull(user.AvatarUrl);
    }
}