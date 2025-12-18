using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Gateway.WebApi.Controllers;
using Users.Application.Commands.Auth;
using Users.Application.DTOs;
using BuildingBlocks.Abstractions;

namespace Identity.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController(_mediatorMock.Object);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidCommand_ReturnsCreatedResult()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "Test@123456", "John", "Doe");
        var authTokenDto = new AuthTokenDto(
            Guid.NewGuid(),
            "access_token",
            "refresh_token",
            3600
        );
        var successResult = Result<AuthTokenDto>.Success(authTokenDto);

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        result.Should().BeOfType<CreatedResult>();
        var createdResult = result as CreatedResult;
        createdResult!.Value.Should().Be(authTokenDto);
    }

    [Fact]
    public async Task Register_WithValidCommand_ReturnsLocationHeader()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "Test@123456", "John", "Doe");
        var userId = Guid.NewGuid();
        var authTokenDto = new AuthTokenDto(userId, "access_token", "refresh_token", 3600);
        var successResult = Result<AuthTokenDto>.Success(authTokenDto);

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        var createdResult = result as CreatedResult;
        createdResult!.Location.Should().Contain($"/api/users/{userId}");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var command = new RegisterUserCommand("invalid-email", "Test@123456", "John", "Doe");
        var failureResult = Result<AuthTokenDto>.Fail("INVALID_EMAIL", "Email format is invalid");

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "weak", "John", "Doe");
        var failureResult = Result<AuthTokenDto>.Fail("WEAK_PASSWORD", "Password does not meet requirements");

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var command = new RegisterUserCommand("existing@example.com", "Test@123456", "John", "Doe");
        var failureResult = Result<AuthTokenDto>.Fail("EMAIL_ALREADY_EXISTS", "Email already registered");

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.Register(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "Test@123456");
        var authTokenDto = new AuthTokenDto(
            Guid.NewGuid(),
            "access_token",
            "refresh_token",
            3600
        );
        var successResult = Result<AuthTokenDto>.Success(authTokenDto);

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(authTokenDto);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "WrongPassword");
        var failureResult = Result<AuthTokenDto>.Fail("INVALID_CREDENTIALS", "Invalid email or password");

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var command = new LoginUserCommand("nonexistent@example.com", "Test@123456");
        var failureResult = Result<AuthTokenDto>.Fail("INVALID_CREDENTIALS", "Invalid email or password");

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var command = new LoginUserCommand("inactive@example.com", "Test@123456");
        var failureResult = Result<AuthTokenDto>.Fail("ACCOUNT_INACTIVE", "Account is inactive");

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithLockedAccount_ReturnsUnauthorized()
    {
        // Arrange
        var command = new LoginUserCommand("locked@example.com", "Test@123456");
        var failureResult = Result<AuthTokenDto>.Fail("ACCOUNT_LOCKED", "Account is locked due to multiple failed attempts");

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_SendsCommandToMediator()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "Test@123456");
        var authTokenDto = new AuthTokenDto(Guid.NewGuid(), "access_token", "refresh_token", 3600);
        var successResult = Result<AuthTokenDto>.Success(authTokenDto);

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        await _controller.Login(command);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOkResult()
    {
        // Arrange
        var command = new RefreshTokenCommand("refresh_token");
        var refreshTokenDto = new RefreshTokenDto("new_access_token", "new_refresh_token", 3600);
        var successResult = Result<RefreshTokenDto>.Success(refreshTokenDto);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.RefreshToken(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var command = new RefreshTokenCommand("invalid_token");
        var failureResult = Result<RefreshTokenDto>.Fail("INVALID_TOKEN", "Refresh token is invalid or expired");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.RefreshToken(command);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region ChangePassword Tests

    [Fact]
    public async Task ChangePassword_WithValidCommand_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "Test@123456", "NewTest@123456");
        var successResult = Result<Unit>.Success(Unit.Value);

        _mediatorMock
            .Setup(x => x.Send(It.Is<ChangePasswordCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.ChangePassword(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WithIncorrectOldPassword_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "WrongPassword", "NewTest@123456");
        var failureResult = Result<Unit>.Fail("INCORRECT_PASSWORD", "Current password is incorrect");

        _mediatorMock
            .Setup(x => x.Send(It.Is<ChangePasswordCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.ChangePassword(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WithWeakNewPassword_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand(userId, "Test@123456", "weak");
        var failureResult = Result<Unit>.Fail("WEAK_PASSWORD", "Password does not meet requirements");

        _mediatorMock
            .Setup(x => x.Send(It.Is<ChangePasswordCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.ChangePassword(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region RequestPasswordReset Tests

    [Fact]
    public async Task RequestPasswordReset_WithValidEmail_ReturnsOkResult()
    {
        // Arrange
        var command = new RequestPasswordResetCommand("test@example.com");
        var successResult = Result<string>.Success("Password reset email sent");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RequestPasswordResetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.RequestPasswordReset(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RequestPasswordReset_WithNonExistentEmail_ReturnsOkResult()
    {
        // Arrange
        var command = new RequestPasswordResetCommand("nonexistent@example.com");
        var successResult = Result<string>.Success("Password reset email sent");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RequestPasswordResetCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.RequestPasswordReset(command);

        // Assert
        // Even if email doesn't exist, return OK for security (don't reveal if email exists)
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region ResetPassword Tests

    [Fact]
    public async Task ResetPassword_WithValidToken_ReturnsOkResult()
    {
        // Arrange
        var command = new ResetPasswordWithTokenCommand("valid_token", "NewTest@123456");
        var successResult = Result<bool>.Success(true);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ResetPasswordWithTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.ResetPassword(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var command = new ResetPasswordWithTokenCommand("invalid_token", "NewTest@123456");
        var failureResult = Result<bool>.Fail("INVALID_TOKEN", "Reset token is invalid or expired");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ResetPasswordWithTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.ResetPassword(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region VerifyEmail Tests

    [Fact]
    public async Task VerifyEmail_WithValidToken_ReturnsOkResult()
    {
        // Arrange
        var command = new VerifyEmailCommand("valid_token");
        var successResult = Result<bool>.Success(true);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.VerifyEmail(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var command = new VerifyEmailCommand("invalid_token");
        var failureResult = Result<bool>.Fail("INVALID_TOKEN", "Verification token is invalid or expired");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.VerifyEmail(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}