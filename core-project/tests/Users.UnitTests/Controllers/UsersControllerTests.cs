using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Gateway.WebApi.Controllers;
using Users.Application.Commands.Users;
using Users.Application.DTOs;
using Users.Application.Queries;
using Shared.Testing.Builders;
using Shared.Testing.Fixtures;
using BuildingBlocks.Abstractions;

namespace Identity.UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsersController(_mediatorMock.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var users = new List<UserDto>
        {
            UserDataBuilder.CreateDefault().Build(),
            UserDataBuilder.CreateDefault().Build()
        };

        var paginatedResult = new PaginatedResult<UserDto>(users, 2, 1, 10);
        var successResult = Result<PaginatedResult<UserDto>>.Success(paginatedResult);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(paginatedResult);
    }

    [Fact]
    public async Task GetAll_WithPagination_PassesParametersToMediator()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 20;
        var paginatedResult = new PaginatedResult<UserDto>([], 0, pageNumber, pageSize);
        var successResult = Result<PaginatedResult<UserDto>>.Success(paginatedResult);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        await _controller.GetAll(pageNumber, pageSize);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<GetAllUsersQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithSearchTerm_PassesSearchToMediator()
    {
        // Arrange
        var searchTerm = "john";
        var paginatedResult = new PaginatedResult<UserDto>([], 0, 1, 10);
        var successResult = Result<PaginatedResult<UserDto>>.Success(paginatedResult);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        await _controller.GetAll(searchTerm: searchTerm);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<GetAllUsersQuery>(q => q.SearchTerm == searchTerm),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_WithQueryFailure_ReturnsBadRequest()
    {
        // Arrange
        var failureResult = Result<PaginatedResult<UserDto>>.Fail("QUERY_ERROR", "Failed to retrieve users");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidUserId_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = UserDataBuilder.CreateDefault().WithId(userId).Build();
        var successResult = Result<UserDto>.Success(userDto);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(userDto);
    }

    [Fact]
    public async Task GetById_WithValidUserId_PassesUserIdToMediator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = UserDataBuilder.CreateDefault().Build();
        var successResult = Result<UserDto>.Success(userDto);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        await _controller.GetById(userId);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<GetUserByIdQuery>(q => q.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var failureResult = Result<UserDto>.Fail("USER_NOT_FOUND", "User not found");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetByEmail Tests

    [Fact]
    public async Task GetByEmail_WithValidEmail_ReturnsOkResult()
    {
        // Arrange
        var email = "test@example.com";
        var userDto = UserDataBuilder.CreateDefault().WithEmail(email).Build();
        var successResult = Result<UserDto>.Success(userDto);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.GetByEmail(email);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(userDto);
    }

    [Fact]
    public async Task GetByEmail_WithInvalidEmail_ReturnsNotFound()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var failureResult = Result<UserDto>.Fail("USER_NOT_FOUND", "User not found");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.GetByEmail(email);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region UpdateProfile Tests

    [Fact]
    public async Task UpdateProfile_WithValidCommand_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand(userId, "Jane", "Smith");
        var updatedUserDto = UserDataBuilder.CreateDefault()
            .WithId(userId)
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .Build();

        var successResult = Result<UserDto>.Success(updatedUserDto);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.UpdateProfile(userId, command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(updatedUserDto);
    }

    [Fact]
    public async Task UpdateProfile_MergesUserIdFromRoute()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand(Guid.NewGuid(), "Jane", "Smith");
        var successResult = Result<UserDto>.Success(UserDataBuilder.CreateDefault().Build());

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        await _controller.UpdateProfile(userId, command);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<UpdateUserProfileCommand>(c => c.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_WithNonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand(userId, "Jane", "Smith");
        var failureResult = Result<UserDto>.Fail("USER_NOT_FOUND", "User not found");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.UpdateProfile(userId, command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeactivateUser Tests

    [Fact]
    public async Task DeactivateUser_WithValidUserId_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var successResult = Result<Unit>.Success(Unit.Value);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<Users.Application.Commands.Users.DeactivateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.DeactivateUser(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeactivateUser_WithNonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var failureResult = Result<Unit>.Fail("USER_NOT_FOUND", "User not found");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<Users.Application.Commands.Users.DeactivateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.DeactivateUser(userId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region ActivateUser Tests

    [Fact]
    public async Task ActivateUser_WithValidUserId_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var successResult = Result<Unit>.Success(Unit.Value);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<Users.Application.Commands.Users.ActivateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.ActivateUser(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region AssignRole Tests

    [Fact]
    public async Task AssignRole_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var successResult = Result<Unit>.Success(Unit.Value);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<Users.Application.Commands.UserRoles.AssignRoleToUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.AssignRole(userId, roleId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AssignRole_PassesParametersToMediator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var successResult = Result<Unit>.Success(Unit.Value);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<Users.Application.Commands.UserRoles.AssignRoleToUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        await _controller.AssignRole(userId, roleId);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<Users.Application.Commands.UserRoles.AssignRoleToUserCommand>(
                    c => c.UserId == userId && c.RoleId == roleId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RemoveRole Tests

    [Fact]
    public async Task RemoveRole_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var successResult = Result<Unit>.Success(Unit.Value);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<Users.Application.Commands.UserRoles.RemoveRoleFromUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        // Act
        var result = await _controller.RemoveRole(userId, roleId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemoveRole_WithError_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var failureResult = Result<Unit>.Fail("ROLE_NOT_FOUND", "Role not found");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<Users.Application.Commands.UserRoles.RemoveRoleFromUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.RemoveRole(userId, roleId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}