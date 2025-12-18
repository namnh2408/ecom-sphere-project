using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly UsersDbContext _dbContext;

    public EmailVerificationTokenRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        await _dbContext.EmailVerificationTokens.AddAsync(token, cancellationToken);
    }

    public async Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailVerificationTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task<EmailVerificationToken?> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailVerificationTokens
            .Where(t => t.UserId == userId && !t.IsVerified && t.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        _dbContext.EmailVerificationTokens.Update(token);
        await Task.CompletedTask;
    }
}