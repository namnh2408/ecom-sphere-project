using Users.Domain.Entities;

namespace Users.Domain.Repositories;

/// <summary>
/// Repository for Role entity
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Adds a new role to the repository
    /// </summary>
    Task AddAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role
    /// </summary>
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a role by ID
    /// </summary>
    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a role by name
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple roles by their IDs
    /// </summary>
    Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role by ID
    /// </summary>
    Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all roles
    /// </summary>
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active roles only
    /// </summary>
    Task<IEnumerable<Role>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role with the given name exists
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}