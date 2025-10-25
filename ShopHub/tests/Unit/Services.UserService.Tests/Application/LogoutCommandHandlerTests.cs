using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ShopHub.Common.Results;
using ShopHub.Domain.Abstractions;
using ShopHub.Services.UserService.Application.Commands;
using ShopHub.Services.UserService.Application.Handlers;
using ShopHub.Services.UserService.Domain.Entities;
using ShopHub.Services.UserService.Domain.Repositories;

namespace ShopHub.Services.UserService.Tests.Application;

/// <summary>
/// Unit tests cho LogoutCommandHandler
/// </summary>
[TestClass]
public class LogoutCommandHandlerTests
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<IUnitOfWork> _mockUnitOfWork = null!;
    private Mock<ILogger<LogoutCommandHandler>> _mockLogger = null!;
    private LogoutCommandHandler _handler = null!;

    private Guid _userId;
    private string _email;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<LogoutCommandHandler>>();
        _handler = new LogoutCommandHandler(_mockUserRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);

        _userId = Guid.NewGuid();
        _email = "user@example.com";
    }

    [TestMethod]
    public async Task Handle_WithValidUserId_ClearsRefreshTokenSuccessfully()
    {
        // Arrange
        var command = new LogoutCommand(_userId);
        var user = User.Create(_email, "John Doe", BCrypt.Net.BCrypt.HashPassword("Password123"));
        user.SetRefreshToken("refresh_token", DateTime.UtcNow.AddDays(7));

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data);

        _mockUserRepository.Verify(
            x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithNonExistentUserId_ReturnsFailure()
    {
        // Arrange
        var command = new LogoutCommand(_userId);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains($"User with ID {_userId} not found"));

        _mockUserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_ClearsRefreshTokenFromUser()
    {
        // Arrange
        var command = new LogoutCommand(_userId);
        var user = User.Create(_email, "John Doe", BCrypt.Net.BCrypt.HashPassword("Password123"));
        var refreshToken = "refresh_token_value";
        var expiryTime = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(refreshToken, expiryTime);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        User? updatedUser = null;
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => updatedUser = u)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(updatedUser);
        Assert.IsNull(updatedUser.RefreshToken);
        Assert.IsNull(updatedUser.RefreshTokenExpiresAt);
    }

    [TestMethod]
    public async Task Handle_WithUserWithoutRefreshToken_StillSucceeds()
    {
        // Arrange
        var command = new LogoutCommand(_userId);
        var user = User.Create(_email, "John Doe", BCrypt.Net.BCrypt.HashPassword("Password123"));
        // User doesn't have a refresh token set

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data);
    }

    [TestMethod]
    public async Task Handle_WhenUpdateAsyncThrowsException_ReturnsFailure()
    {
        // Arrange
        var command = new LogoutCommand(_userId);
        var user = User.Create(_email, "John Doe", BCrypt.Net.BCrypt.HashPassword("Password123"));
        user.SetRefreshToken("token", DateTime.UtcNow.AddDays(7));

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database update error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("An error occurred during logout"));

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenSaveChangesThrowsException_ReturnsFailure()
    {
        // Arrange
        var command = new LogoutCommand(_userId);
        var user = User.Create(_email, "John Doe", BCrypt.Net.BCrypt.HashPassword("Password123"));
        user.SetRefreshToken("token", DateTime.UtcNow.AddDays(7));

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save changes error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("An error occurred during logout"));
    }

    [TestMethod]
    public async Task Handle_WhenGetByIdThrowsException_ReturnsFailure()
    {
        // Arrange
        var command = new LogoutCommand(_userId);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("An error occurred during logout"));

        _mockUserRepository.Verify(
            x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_ReturnsSuccessWithTrueValue()
    {
        // Arrange
        var command = new LogoutCommand(_userId);
        var user = User.Create(_email, "John Doe", BCrypt.Net.BCrypt.HashPassword("Password123"));

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data);
    }
}