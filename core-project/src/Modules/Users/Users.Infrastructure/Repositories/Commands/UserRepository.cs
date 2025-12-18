using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user is not null)
        {
            _context.Users.Remove(user);
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.Where(u => u.IsActive).ToListAsync(cancellationToken);
    }
}