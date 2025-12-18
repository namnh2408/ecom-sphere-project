using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly UsersDbContext _dbContext;

    public PasswordResetTokenRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await _dbContext.PasswordResetTokens.AddAsync(token, cancellationToken);
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        _dbContext.PasswordResetTokens.Update(token);
        await Task.CompletedTask;
    }
}