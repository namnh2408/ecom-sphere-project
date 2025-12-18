using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class UserActivityHistoryRepository : IUserActivityHistoryRepository
{
    private readonly UsersDbContext _dbContext;

    public UserActivityHistoryRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UserActivityHistory activity)
    {
        await _dbContext.UserActivityHistories.AddAsync(activity);
    }

    public async Task<IReadOnlyList<UserActivityHistory>> GetUserActivitiesAsync(Guid userId, int? limit = null, int daysBack = 30)
    {
        var since = DateTime.UtcNow.AddDays(-daysBack);
        var query = _dbContext.UserActivityHistories
            .Where(x => x.UserId == userId && x.OccurredAtUtc >= since)
            .OrderByDescending(x => x.OccurredAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<UserActivityHistory>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<UserActivityHistory>> GetActivitiesByTypeAsync(string activityType, int? limit = null, int daysBack = 30)
    {
        var since = DateTime.UtcNow.AddDays(-daysBack);
        var query = _dbContext.UserActivityHistories
            .Where(x => x.ActivityType == activityType && x.OccurredAtUtc >= since)
            .OrderByDescending(x => x.OccurredAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<UserActivityHistory>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<int> GetActivityCountByTypeAsync(Guid userId, string activityType, int daysBack = 30)
    {
        var since = DateTime.UtcNow.AddDays(-daysBack);
        return await _dbContext.UserActivityHistories
            .CountAsync(x => x.UserId == userId && x.ActivityType == activityType && x.OccurredAtUtc >= since);
    }

    public async Task DeleteOldActivitiesAsync(int daysToKeep = 365)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldActivities = await _dbContext.UserActivityHistories
            .Where(x => x.OccurredAtUtc < cutoffDate)
            .ToListAsync();

        if (oldActivities.Any())
        {
            _dbContext.UserActivityHistories.RemoveRange(oldActivities);
        }
    }
}