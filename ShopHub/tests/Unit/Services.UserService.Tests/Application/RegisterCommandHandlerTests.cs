using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ShopHub.Common.Results;
using ShopHub.Domain.Abstractions;
using ShopHub.Services.UserService.Application.Commands;
using ShopHub.Services.UserService.Application.DTOs;
using ShopHub.Services.UserService.Application.Handlers;
using ShopHub.Services.UserService.Domain.Entities;
using ShopHub.Services.UserService.Domain.Repositories;

namespace ShopHub.Services.UserService.Tests.Application;

/// <summary>
/// Unit tests cho RegisterCommandHandler
/// </summary>
[TestClass]
public class RegisterCommandHandlerTests
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<IUnitOfWork> _mockUnitOfWork = null!;
    private Mock<ILogger<RegisterCommandHandler>> _mockLogger = null!;
    private RegisterCommandHandler _handler = null!;

    private string _email;
    private string _fullName;
    private string _validPassword;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<RegisterCommandHandler>>();
        _handler = new RegisterCommandHandler(_mockUserRepository.Object, _mockUnitOfWork.Object, _mockLogger.Object);

        _email = "newuser@example.com";
        _fullName = "John Doe";
        _validPassword = "ValidPass123";
    }

    [TestMethod]
    public async Task Handle_WithValidData_RegistersNewUserSuccessfully()
    {
        // Arrange
        var command = new RegisterCommand(
            _email,
            _fullName,
            _validPassword,
            _validPassword,
            "+84912345678");

        _mockUserRepository
            .Setup(x => x.EmailExistsAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(_email, result.Data.Email);
        Assert.AreEqual(_fullName, result.Data.FullName);

        _mockUserRepository.Verify(
            x => x.EmailExistsAsync(_email, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            _email,
            _fullName,
            _validPassword,
            _validPassword);

        _mockUserRepository
            .Setup(x => x.EmailExistsAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains($"Email {_email} is already registered"));

        _mockUserRepository.Verify(
            x => x.EmailExistsAsync(_email, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithoutPhoneNumber_RegistersSuccessfully()
    {
        // Arrange
        var command = new RegisterCommand(
            _email,
            _fullName,
            _validPassword,
            _validPassword,
            null);

        _mockUserRepository
            .Setup(x => x.EmailExistsAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(_email, result.Data.Email);
    }

    [TestMethod]
    public async Task Handle_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            _email,
            _fullName,
            _validPassword,
            _validPassword);

        _mockUserRepository
            .Setup(x => x.EmailExistsAsync(_email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("An error occurred during registration"));
    }

    [TestMethod]
    public async Task Handle_WhenSaveChangesThrowsException_ReturnsFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            _email,
            _fullName,
            _validPassword,
            _validPassword);

        _mockUserRepository
            .Setup(x => x.EmailExistsAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Message!.Contains("An error occurred during registration"));
    }

    [TestMethod]
    public async Task Handle_UserProfileDtoIsCreatedCorrectly()
    {
        // Arrange
        var command = new RegisterCommand(
            _email,
            _fullName,
            _validPassword,
            _validPassword,
            "+84987654321");

        _mockUserRepository
            .Setup(x => x.EmailExistsAsync(_email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var profile = result.Data;
        Assert.IsNotNull(profile);
        Assert.AreEqual(_email, profile.Email);
        Assert.AreEqual(_fullName, profile.FullName);
        Assert.AreEqual("+84987654321", profile.PhoneNumber);
        Assert.IsTrue(profile.IsActive);
        Assert.IsFalse(profile.IsEmailVerified);
    }
}