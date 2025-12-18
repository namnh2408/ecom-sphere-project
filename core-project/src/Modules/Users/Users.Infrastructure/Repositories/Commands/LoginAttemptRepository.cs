using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class LoginAttemptRepository : ILoginAttemptRepository
{
    private readonly UsersDbContext _dbContext;

    public LoginAttemptRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(LoginAttempt attempt, CancellationToken cancellationToken = default)
    {
        await _dbContext.LoginAttempts.AddAsync(attempt, cancellationToken);
    }

    public async Task<IReadOnlyList<LoginAttempt>> GetRecentFailedAttemptsAsync(Guid userId, int minutesBack = 15, CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutesBack);
        return await _dbContext.LoginAttempts
            .Where(a => a.UserId == userId && !a.IsSuccessful && a.AttemptedAtUtc >= since)
            .OrderByDescending(a => a.AttemptedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoginAttempt>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoginAttempts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AttemptedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoginAttempt>> GetUserLoginAttemptsAsync(Guid userId, int? limit = null, int daysBack = 30, CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddDays(-daysBack);
        var query = _dbContext.LoginAttempts
            .Where(a => a.UserId == userId && a.AttemptedAtUtc >= since)
            .OrderByDescending(a => a.AttemptedAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<LoginAttempt>)query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}