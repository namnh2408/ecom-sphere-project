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
/// Unit tests cho LoginQueryHandler
/// </summary>
[TestClass]
public class LoginQueryHandlerTests
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<IConfiguration> _mockConfiguration = null!;
    private Mock<ILogger<LoginQueryHandler>> _mockLogger = null!;
    private LoginQueryHandler _handler = null!;

    private Guid _userId;
    private string _email;
    private string _password;
    private string _passwordHash;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<LoginQueryHandler>>();
        _handler = new LoginQueryHandler(_mockUserRepository.Object, _mockConfiguration.Object, _mockLogger.Object);

        _userId = Guid.NewGuid();
        _email = "user@example.com";
        _password = "ValidPass123";
        _passwordHash = BCrypt.Net.BCrypt.HashPassword(_password);

        SetupDefaultJwtConfiguration();
    }

    private void SetupDefaultJwtConfiguration()
    {
        var jwtSettings = new Mock<IConfigurationSection>();
        jwtSettings.Setup(x => x["SecretKey"]).Returns("ThisIsAVeryLongSecretKeyForJwtTokenGeneration123456789");
        jwtSettings.Setup(x => x["Issuer"]).Returns("ShopHub");
        jwtSettings.Setup(x => x["Audience"]).Returns("ShopHubUsers");
        jwtSettings.Setup(x => x["ExpirationMinutes"]).Returns("60");

        _mockConfiguration
            .Setup(x => x.GetSection("JwtSettings"))
            .Returns(jwtSettings.Object);
    }

    [TestMethod]
    public async Task Handle_WithValidCredentials_ReturnsLoginResponseSuccessfully()
    {
        // Arrange
        var query = new LoginQuery(_email, _password);
        var user = User.Create(_email, "John Doe", _passwordHash, "+84912345678");
        user.SetRefreshToken("refresh_token_old", DateTime.UtcNow.AddDays(-1));

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(_email, result.Data.Email);
        Assert.IsNotNull(result.Data.AccessToken);
        Assert.IsNotNull(result.Data.RefreshToken);
        Assert.AreEqual("Bearer", result.Data.TokenType);
        Assert.AreEqual(3600, result.Data.ExpiresIn);

        _mockUserRepository.Verify(
            x => x.GetByEmailAsync(_email, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithNonExistentEmail_ReturnsFailure()
    {
        // Arrange
        var query = new LoginQuery("nonexistent@example.com", _password);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("Invalid email or password"));

        _mockUserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithInactiveUser_ReturnsFailure()
    {
        // Arrange
        var query = new LoginQuery(_email, _password);
        var user = User.Create(_email, "John Doe", _passwordHash, "+84912345678");
        user.Deactivate(); // Deactivate user

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("User account is inactive"));

        _mockUserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        var query = new LoginQuery(_email, "WrongPassword123");
        var user = User.Create(_email, "John Doe", _passwordHash, "+84912345678");

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("Invalid email or password"));

        _mockUserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_UpdatesUserWithRefreshTokenAndLastLogin()
    {
        // Arrange
        var query = new LoginQuery(_email, _password);
        var user = User.Create(_email, "John Doe", _passwordHash, "+84912345678");

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        User? updatedUser = null;
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => updatedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(updatedUser);
        Assert.IsNotNull(updatedUser.RefreshToken);
        Assert.IsNotNull(updatedUser.RefreshTokenExpiresAt);
        Assert.IsNotNull(updatedUser.LastLoginAt);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var query = new LoginQuery(_email, _password);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(_email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("An error occurred during login"));
    }

    [TestMethod]
    public async Task Handle_GeneratesValidJwtToken()
    {
        // Arrange
        var query = new LoginQuery(_email, _password);
        var user = User.Create(_email, "John Doe", _passwordHash, "+84912345678");

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data?.AccessToken);
        Assert.IsTrue(result.Data.AccessToken.Contains("."));
        Assert.IsTrue(result.Data.AccessToken.Split('.').Length == 3); // JWT has 3 parts
    }

    [TestMethod]
    public async Task Handle_GeneratesValidRefreshToken()
    {
        // Arrange
        var query = new LoginQuery(_email, _password);
        var user = User.Create(_email, "John Doe", _passwordHash, "+84912345678");

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data?.RefreshToken);
        Assert.IsTrue(result.Data.RefreshToken.Length > 20); // Should be a reasonably long token
    }
}