using FluentAssertions;
using Moq;
using BuildingBlocks.Abstractions;
using Users.Application.Commands.Users;
using Users.Application.Handlers;
using Users.Domain.Repositories;
using Shared.Testing.Fixtures;

namespace Identity.UnitTests.Handlers.Commands;

public class UpdateUserProfileCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateUserProfileCommandHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesUserProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newFirstName = "Jane";
        var newLastName = "Smith";
        
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateUserProfileCommand(userId, newFirstName, newLastName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().Be(newFirstName);
        result.Value.LastName.Should().Be(newLastName);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CallsRepositoryUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateUserProfileCommand(userId, "Jane", "Smith");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CallsUnitOfWorkSaveChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateUserProfileCommand(userId, "Jane", "Smith");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithWhitespaceNames_TrimsThemCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var firstNameWithSpaces = "  Jane  ";
        var lastNameWithSpaces = "  Smith  ";
        
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateUserProfileCommand(userId, firstNameWithSpaces, lastNameWithSpaces);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task Handle_WithMultipleUpdates_EachUpdateCallsRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command1 = new UpdateUserProfileCommand(userId, "Jane", "Smith");
        var command2 = new UpdateUserProfileCommand(userId, "John", "Doe");

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    #endregion

    #region Error Cases

    [Fact]
    public async Task Handle_WithNonExistentUserId_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Users.Domain.Entities.User?)null);

        var command = new UpdateUserProfileCommand(userId, "Jane", "Smith");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_DoesNotCallUpdateAsync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Users.Domain.Entities.User?)null);

        var command = new UpdateUserProfileCommand(userId, "Jane", "Smith");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var command = new UpdateUserProfileCommand(userId, "Jane", "Smith");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region Integration Cases

    [Fact]
    public async Task Handle_WithCancellationToken_PassesItToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync(testUser);

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Users.Domain.Entities.User>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var command = new UpdateUserProfileCommand(userId, "Jane", "Smith");

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(userId, cancellationToken),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    #endregion
}