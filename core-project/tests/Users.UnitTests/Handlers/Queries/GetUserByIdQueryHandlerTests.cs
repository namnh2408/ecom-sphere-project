using FluentAssertions;
using Moq;
using Users.Application.DTOs;
using Users.Application.Handlers;
using Users.Application.Queries;
using Users.Domain.Repositories;
using Shared.Testing.Builders;
using Shared.Testing.Fixtures;

namespace Identity.UnitTests.Handlers.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidUserId_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.Email.Should().Be(testUser.Email.Value);
        result.Value.FirstName.Should().Be(testUser.FirstName);
        result.Value.LastName.Should().Be(testUser.LastName);
        result.Value.IsActive.Should().Be(testUser.IsActive);
        result.Value.IsEmailVerified.Should().Be(testUser.IsEmailVerified);
    }

    [Fact]
    public async Task Handle_WithVerifiedUser_IncludesVerificationStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        testUser.VerifyEmail();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInactiveUser_IncludesInactiveStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        testUser.Deactivate();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithUserRoles_IncludesRoleIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();
        
        var testUser = UserRepositoryFixture.CreateTestUser(id: userId);
        testUser.AssignRole(roleId1, "Admin");
        testUser.AssignRole(roleId2, "Editor");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RoleIds.Should().HaveCount(2);
        result.Value.RoleIds.Should().Contain(roleId1);
        result.Value.RoleIds.Should().Contain(roleId2);
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

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("USER_NOT_FOUND");
        result.Error.Message.Should().Contain(userId.ToString());
    }

    [Fact]
    public async Task Handle_WithMultipleRequests_CallsRepositoryEachTime()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var testUser1 = UserRepositoryFixture.CreateTestUser(id: userId1, email: "user1@example.com");
        var testUser2 = UserRepositoryFixture.CreateTestUser(id: userId2, email: "user2@example.com");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser1);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUser2);

        // Act
        var result1 = await _handler.Handle(new GetUserByIdQuery(userId1), CancellationToken.None);
        var result2 = await _handler.Handle(new GetUserByIdQuery(userId2), CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value!.Id.Should().NotBe(result2.Value!.Id);
        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
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

        var query = new GetUserByIdQuery(userId);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(userId, cancellationToken),
            Times.Once);
    }

    #endregion
}