using Users.Domain.Entities;

namespace Users.Domain.Repositories;

/// <summary>
/// Repository for User aggregate root
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Adds a new user to the repository
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by ID
    /// </summary>
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the given email exists
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user by ID
    /// </summary>
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all users (paginated)
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active users only
    /// </summary>
    Task<IEnumerable<User>> GetActiveAsync(CancellationToken cancellationToken = default);
}