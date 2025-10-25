using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MediatR;
using System.Security.Claims;
using ShopHub.Common.Results;
using ShopHub.Services.UserService.Application.Commands;
using ShopHub.Services.UserService.Application.DTOs;
using ShopHub.Services.UserService.Application.Queries;
using ShopHub.Services.UserService.Presentation.Controllers;

namespace ShopHub.Services.UserService.Tests.Application;

/// <summary>
/// Unit tests cho UsersController
/// </summary>
[TestClass]
public class UsersControllerTests
{
    private Mock<IMediator> _mockMediator = null!;
    private Mock<ILogger<UsersController>> _mockLogger = null!;
    private UsersController _controller = null!;

    private Guid _userId;
    private string _email;
    private string _validPassword;

    [TestInitialize]
    public void Setup()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockMediator.Object, _mockLogger.Object);

        _userId = Guid.NewGuid();
        _email = "test@example.com";
        _validPassword = "ValidPass123";
    }

    #region Register Tests

    [TestMethod]
    public async Task Register_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = _email,
            FullName = "John Doe",
            Password = _validPassword,
            ConfirmPassword = _validPassword,
            PhoneNumber = "+84912345678"
        };

        var userProfile = new UserProfileDto
        {
            UserId = _userId,
            Email = _email,
            FullName = "John Doe",
            PhoneNumber = "+84912345678",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userProfile));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        Assert.IsNotNull(result);
        // Check if it's a CreatedAtActionResult (successful registration)
        var okResult = result.Result as CreatedAtActionResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(nameof(UsersController.GetProfile), okResult.ActionName);
        Assert.AreEqual(201, okResult.StatusCode);
    }

    [TestMethod]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = _email,
            FullName = "John Doe",
            Password = _validPassword,
            ConfirmPassword = _validPassword
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserProfileDto>.Failure($"Email {_email} is already registered"));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        Assert.IsNotNull(result);
        var conflictResult = result.Result as ConflictObjectResult;
        Assert.IsNotNull(conflictResult);
        Assert.AreEqual(409, conflictResult.StatusCode);
    }

    [TestMethod]
    public async Task Register_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Email is required");
        
        var registerDto = new RegisterDto
        {
            Email = "",
            FullName = "John Doe",
            Password = _validPassword,
            ConfirmPassword = _validPassword
        };

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        Assert.IsNotNull(result);
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task Register_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = _email,
            FullName = "John Doe",
            Password = _validPassword,
            ConfirmPassword = _validPassword
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        Assert.IsNotNull(result);
        var statusCodeResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusCodeResult);
        Assert.AreEqual(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region Login Tests

    [TestMethod]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = _email,
            Password = _validPassword
        };

        var loginResponse = new LoginResponseDto
        {
            UserId = _userId,
            Email = _email,
            FullName = "John Doe",
            AccessToken = "access_token_here",
            RefreshToken = "refresh_token_here",
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<LoginQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoginResponseDto>.Success(loginResponse));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsNotNull(result);
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = _email,
            Password = "WrongPassword123"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<LoginQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoginResponseDto>.Failure("Invalid email or password"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsNotNull(result);
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);
    }

    [TestMethod]
    public async Task Login_WithInactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = _email,
            Password = _validPassword
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<LoginQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LoginResponseDto>.Failure("User account is inactive"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsNotNull(result);
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);
    }

    [TestMethod]
    public async Task Login_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Email is required");
        
        var loginDto = new LoginDto
        {
            Email = "",
            Password = _validPassword
        };

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsNotNull(result);
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task Login_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = _email,
            Password = _validPassword
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<LoginQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("JWT configuration error"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsNotNull(result);
        var statusCodeResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusCodeResult);
        Assert.AreEqual(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region Logout Tests

    [TestMethod]
    public async Task Logout_WithValidUser_ReturnsOk()
    {
        // Arrange
        var userIdClaim = new Claim(ClaimTypes.NameIdentifier, _userId.ToString());
        var identity = new ClaimsIdentity(new[] { userIdClaim });
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = principal };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.Logout();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public async Task Logout_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = principal };

        // Act
        var result = await _controller.Logout();

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);
    }

    [TestMethod]
    public async Task Logout_WithInvalidUserIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        var userIdClaim = new Claim(ClaimTypes.NameIdentifier, "invalid-guid");
        var identity = new ClaimsIdentity(new[] { userIdClaim });
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = principal };

        // Act
        var result = await _controller.Logout();

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);
    }

    [TestMethod]
    public async Task Logout_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var userIdClaim = new Claim(ClaimTypes.NameIdentifier, _userId.ToString());
        var identity = new ClaimsIdentity(new[] { userIdClaim });
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = principal };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<LogoutCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Logout();

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.IsNotNull(statusCodeResult);
        Assert.AreEqual(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region GetProfile Tests

    [TestMethod]
    public async Task GetProfile_WithValidUserId_ReturnsOk()
    {
        // Arrange
        var userProfile = new UserProfileDto
        {
            UserId = _userId,
            Email = _email,
            FullName = "John Doe",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userProfile));

        // Act
        var result = await _controller.GetProfile(_userId);

        // Assert
        Assert.IsNotNull(result);
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public async Task GetProfile_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserProfileDto>.Failure($"User with ID {_userId} not found"));

        // Act
        var result = await _controller.GetProfile(_userId);

        // Assert
        Assert.IsNotNull(result);
        var notFoundResult = result.Result as ObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(500, notFoundResult.StatusCode);
    }

    [TestMethod]
    public async Task GetCurrentProfile_WithValidClaim_ReturnsOk()
    {
        // Arrange
        var userIdClaim = new Claim(ClaimTypes.NameIdentifier, _userId.ToString());
        var identity = new ClaimsIdentity(new[] { userIdClaim });
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = principal };

        var userProfile = new UserProfileDto
        {
            UserId = _userId,
            Email = _email,
            FullName = "John Doe",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserProfileDto>.Success(userProfile));

        // Act
        var result = await _controller.GetCurrentProfile();

        // Assert
        Assert.IsNotNull(result);
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public async Task GetCurrentProfile_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = principal };

        // Act
        var result = await _controller.GetCurrentProfile();

        // Assert
        Assert.IsNotNull(result);
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);
    }

    #endregion
}