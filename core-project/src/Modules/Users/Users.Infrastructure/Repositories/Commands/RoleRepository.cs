using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class RoleRepository : IRoleRepository
{
    private readonly UsersDbContext _context;

    public RoleRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(role, cancellationToken);
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(role);
        await Task.CompletedTask;
    }

    public async Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await GetByIdAsync(roleId, cancellationToken);
        if (role is not null)
        {
            _context.Roles.Remove(role);
        }
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles.Where(r => r.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.AnyAsync(r => r.Name == name, cancellationToken);
    }
}