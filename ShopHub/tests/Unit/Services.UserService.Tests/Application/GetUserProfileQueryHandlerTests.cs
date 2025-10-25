using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ShopHub.Common.Results;
using ShopHub.Services.UserService.Application.DTOs;
using ShopHub.Services.UserService.Application.Handlers;
using ShopHub.Services.UserService.Application.Queries;
using ShopHub.Services.UserService.Domain.Entities;
using ShopHub.Services.UserService.Domain.Repositories;

namespace ShopHub.Services.UserService.Tests.Application;

/// <summary>
/// Unit tests cho GetUserProfileQueryHandler
/// </summary>
[TestClass]
public class GetUserProfileQueryHandlerTests
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<ILogger<GetUserProfileQueryHandler>> _mockLogger = null!;
    private GetUserProfileQueryHandler _handler = null!;

    private Guid _userId;
    private string _email;
    private string _fullName;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<GetUserProfileQueryHandler>>();
        _handler = new GetUserProfileQueryHandler(_mockUserRepository.Object, _mockLogger.Object);

        _userId = Guid.NewGuid();
        _email = "user@example.com";
        _fullName = "John Doe";
    }

    [TestMethod]
    public async Task Handle_WithValidUserId_ReturnsUserProfile()
    {
        // Arrange
        var query = new GetUserProfileQuery(_userId);
        var user = User.Create(_email, _fullName, BCrypt.Net.BCrypt.HashPassword("Password123"), "+84912345678");
        user.UpdateProfile(null, null, "avatar.jpg", "123 Street", "Ho Chi Minh", "Vietnam");

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(_userId, result.Data.UserId);
        Assert.AreEqual(_email, result.Data.Email);
        Assert.AreEqual(_fullName, result.Data.FullName);
        Assert.AreEqual("+84912345678", result.Data.PhoneNumber);
        Assert.AreEqual("123 Street", result.Data.Address);
        Assert.AreEqual("Ho Chi Minh", result.Data.City);
        Assert.AreEqual("Vietnam", result.Data.Country);
        Assert.AreEqual("avatar.jpg", result.Data.AvatarUrl);

        _mockUserRepository.Verify(
            x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithNonExistentUserId_ReturnsFailure()
    {
        // Arrange
        var query = new GetUserProfileQuery(_userId);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains($"User with ID {_userId} not found"));
    }

    [TestMethod]
    public async Task Handle_WithInactiveUser_ReturnsProfile()
    {
        // Arrange
        var query = new GetUserProfileQuery(_userId);
        var user = User.Create(_email, _fullName, BCrypt.Net.BCrypt.HashPassword("Password123"));
        user.Deactivate();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.IsFalse(result.Data.IsActive);
    }

    [TestMethod]
    public async Task Handle_ReturnsMappedUserProfileDto()
    {
        // Arrange
        var query = new GetUserProfileQuery(_userId);
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;
        var lastLoginAt = DateTime.UtcNow;

        var user = User.Create(_email, _fullName, BCrypt.Net.BCrypt.HashPassword("Password123"), "+84987654321");
        user.UpdateProfile(null, null, "avatar.url", "Address", "City", "Country");
        user.VerifyEmail();
        user.UpdateLastLogin();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var profile = result.Data;
        Assert.IsNotNull(profile);
        Assert.AreEqual(_userId, profile.UserId);
        Assert.AreEqual(_email, profile.Email);
        Assert.AreEqual(_fullName, profile.FullName);
        Assert.AreEqual("+84987654321", profile.PhoneNumber);
        Assert.AreEqual("Address", profile.Address);
        Assert.AreEqual("City", profile.City);
        Assert.AreEqual("Country", profile.Country);
        Assert.AreEqual("avatar.url", profile.AvatarUrl);
        Assert.IsTrue(profile.IsActive);
        Assert.IsTrue(profile.IsEmailVerified);
    }

    [TestMethod]
    public async Task Handle_WithMultipleUsers_ReturnsCorrectUser()
    {
        // Arrange
        var query = new GetUserProfileQuery(_userId);
        var user = User.Create(_email, _fullName, BCrypt.Net.BCrypt.HashPassword("Password123"));
        var otherUserId = Guid.NewGuid();
        var otherUser = User.Create("other@example.com", "Other User", BCrypt.Net.BCrypt.HashPassword("Password123"));

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUser);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(_userId, result.Data.UserId);
        Assert.AreEqual(_email, result.Data.Email);
        Assert.AreEqual(_fullName, result.Data.FullName);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var query = new GetUserProfileQuery(_userId);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("An error occurred"));
    }

    [TestMethod]
    public async Task Handle_WithCompleteUserData_ReturnsAllProperties()
    {
        // Arrange
        var query = new GetUserProfileQuery(_userId);
        var user = new User(
            _userId,
            _email,
            _fullName,
            BCrypt.Net.BCrypt.HashPassword("Password123"),
            "+84912345678");

        user.UpdateProfile("123 Main St", "Bangkok", "Thailand", "https://example.com/avatar.jpg");
        user.VerifyEmail();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var profile = result.Data!;
        
        Assert.AreEqual(_userId, profile.UserId);
        Assert.AreEqual(_email, profile.Email);
        Assert.AreEqual(_fullName, profile.FullName);
        Assert.AreEqual("+84912345678", profile.PhoneNumber);
        Assert.AreEqual("123 Main St", profile.Address);
        Assert.AreEqual("Bangkok", profile.City);
        Assert.AreEqual("Thailand", profile.Country);
        Assert.AreEqual("https://example.com/avatar.jpg", profile.AvatarUrl);
        Assert.IsTrue(profile.IsActive);
        Assert.IsTrue(profile.IsEmailVerified);
        Assert.IsNotNull(profile.CreatedAt);
        Assert.IsNotNull(profile.UpdatedAt);
    }
}