using Moq;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.ValueObjects;

namespace Shared.Testing.Fixtures;

/// <summary>
/// Fixture for setting up mock user repositories for testing
/// </summary>
public class UserRepositoryFixture
{
    public Mock<IUserRepository> UserRepositoryMock { get; private set; }

    public UserRepositoryFixture()
    {
        UserRepositoryMock = new Mock<IUserRepository>();
    }

    public UserRepositoryFixture SetupGetByIdAsync(Guid userId, User user)
    {
        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return this;
    }

    public UserRepositoryFixture SetupGetByIdAsyncReturnsNull(Guid userId)
    {
        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        return this;
    }

    public UserRepositoryFixture SetupGetByEmailAsync(string email, User user)
    {
        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return this;
    }

    public UserRepositoryFixture SetupGetByEmailAsyncReturnsNull(string email)
    {
        UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        return this;
    }

    public UserRepositoryFixture SetupUpdateAsync()
    {
        UserRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return this;
    }

    public UserRepositoryFixture SetupAddAsync()
    {
        UserRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return this;
    }

    public UserRepositoryFixture SetupGetAllAsync(List<User> users, int pageNumber = 1, int pageSize = 10)
    {
        UserRepositoryMock
            .Setup(x => x.GetAllAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
        return this;
    }

    public static User CreateTestUser(
        Guid? id = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null)
    {
        var testId = id ?? Guid.NewGuid();
        var testEmail = email ?? "test@example.com";
        var testFirstName = firstName ?? "John";
        var testLastName = lastName ?? "Doe";

        var result = User.Create(testEmail, "Test@123456", testFirstName, testLastName);
        
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create test user: {result.Error?.Message}");

        return result.Value!;
    }
}