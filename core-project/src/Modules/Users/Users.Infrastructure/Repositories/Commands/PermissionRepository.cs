using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class PermissionRepository : IPermissionRepository
{
    private readonly UsersDbContext _context;

    public PermissionRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await _context.Permissions.AddAsync(permission, cancellationToken);
    }

    public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _context.Permissions.Update(permission);
        await Task.CompletedTask;
    }

    public async Task<Permission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permissionId, cancellationToken);
    }

    public async Task<Permission?> GetByResourceAndActionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.FirstOrDefaultAsync(
            p => p.Resource == resource && p.Action == action, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByIdsAsync(IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.Where(p => permissionIds.Contains(p.Id)).ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        var permission = await GetByIdAsync(permissionId, cancellationToken);
        if (permission is not null)
        {
            _context.Permissions.Remove(permission);
        }
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.Where(p => p.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
        if (role is null) return Enumerable.Empty<Permission>();

        var permissionIds = role.PermissionIds;
        return await GetByIdsAsync(permissionIds, cancellationToken);
    }

    public async Task<bool> ExistsByResourceAndActionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.AnyAsync(
            p => p.Resource == resource && p.Action == action, cancellationToken);
    }
}