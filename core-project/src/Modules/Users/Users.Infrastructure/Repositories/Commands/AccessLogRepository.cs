using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class AccessLogRepository : IAccessLogRepository
{
    private readonly UsersDbContext _dbContext;

    public AccessLogRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AccessLog accessLog)
    {
        await _dbContext.AccessLogs.AddAsync(accessLog);
    }

    public async Task<IReadOnlyList<AccessLog>> GetUserAccessLogsAsync(Guid userId, int? limit = null, int daysBack = 30)
    {
        var since = DateTime.UtcNow.AddDays(-daysBack);
        var query = _dbContext.AccessLogs
            .Where(x => x.UserId == userId && x.AccessedAtUtc >= since)
            .OrderByDescending(x => x.AccessedAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<AccessLog>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<AccessLog>> GetResourceAccessLogsAsync(string resourceName, int? limit = null, int daysBack = 30)
    {
        var since = DateTime.UtcNow.AddDays(-daysBack);
        var query = _dbContext.AccessLogs
            .Where(x => x.ResourceName == resourceName && x.AccessedAtUtc >= since)
            .OrderByDescending(x => x.AccessedAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<AccessLog>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<AccessLog>> GetFailedAccessLogsAsync(Guid userId, int? limit = null, int minutesBack = 60)
    {
        var since = DateTime.UtcNow.AddMinutes(-minutesBack);
        var query = _dbContext.AccessLogs
            .Where(x => x.UserId == userId && !x.WasSuccessful && x.AccessedAtUtc >= since)
            .OrderByDescending(x => x.AccessedAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<AccessLog>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<int> GetAccessCountAsync(Guid userId, string resourceName, int hoursBack = 24)
    {
        var since = DateTime.UtcNow.AddHours(-hoursBack);
        return await _dbContext.AccessLogs
            .CountAsync(x => x.UserId == userId && x.ResourceName == resourceName && x.AccessedAtUtc >= since && x.WasSuccessful);
    }

    public async Task DeleteOldLogsAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldLogs = await _dbContext.AccessLogs
            .Where(x => x.AccessedAtUtc < cutoffDate)
            .ToListAsync();

        if (oldLogs.Any())
        {
            _dbContext.AccessLogs.RemoveRange(oldLogs);
        }
    }
}