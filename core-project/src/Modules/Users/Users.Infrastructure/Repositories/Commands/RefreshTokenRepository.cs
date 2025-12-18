using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly UsersDbContext _dbContext;

    public RefreshTokenRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(token, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Update(token);
        await Task.CompletedTask;
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}