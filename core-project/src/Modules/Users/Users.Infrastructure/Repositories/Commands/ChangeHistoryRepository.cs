using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class ChangeHistoryRepository : IChangeHistoryRepository
{
    private readonly UsersDbContext _dbContext;

    public ChangeHistoryRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ChangeHistory changeHistory)
    {
        await _dbContext.ChangeHistories.AddAsync(changeHistory);
    }

    public async Task<IReadOnlyList<ChangeHistory>> GetUserChangesAsync(Guid userId, int? limit = null, int daysBack = 90)
    {
        var since = DateTime.UtcNow.AddDays(-daysBack);
        var query = _dbContext.ChangeHistories
            .Where(x => x.UserId == userId && x.ChangedAtUtc >= since)
            .OrderByDescending(x => x.ChangedAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<ChangeHistory>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<ChangeHistory>> GetFieldChangesAsync(Guid userId, string fieldName, int? limit = null)
    {
        var query = _dbContext.ChangeHistories
            .Where(x => x.UserId == userId && x.FieldName == fieldName)
            .OrderByDescending(x => x.ChangedAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<ChangeHistory>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<ChangeHistory?> GetLatestChangeAsync(Guid userId, string fieldName)
    {
        return await _dbContext.ChangeHistories
            .Where(x => x.UserId == userId && x.FieldName == fieldName)
            .OrderByDescending(x => x.ChangedAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteOldChangesAsync(int daysToKeep = 365)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldChanges = await _dbContext.ChangeHistories
            .Where(x => x.ChangedAtUtc < cutoffDate)
            .ToListAsync();

        if (oldChanges.Any())
        {
            _dbContext.ChangeHistories.RemoveRange(oldChanges);
        }
    }
}