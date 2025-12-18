using Users.Domain.Entities;

namespace Users.Domain.Repositories;

/// <summary>
/// Repository for Permission entity
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Adds a new permission to the repository
    /// </summary>
    Task AddAsync(Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing permission
    /// </summary>
    Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a permission by ID
    /// </summary>
    Task<Permission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a permission by resource and action
    /// </summary>
    Task<Permission?> GetByResourceAndActionAsync(string resource, string action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple permissions by their IDs
    /// </summary>
    Task<IEnumerable<Permission>> GetByIdsAsync(IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a permission by ID
    /// </summary>
    Task DeleteAsync(Guid permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all permissions
    /// </summary>
    Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active permissions only
    /// </summary>
    Task<IEnumerable<Permission>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all permissions for a specific role
    /// </summary>
    Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a permission with the given resource and action exists
    /// </summary>
    Task<bool> ExistsByResourceAndActionAsync(string resource, string action, CancellationToken cancellationToken = default);
}