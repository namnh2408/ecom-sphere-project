using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure.Repositories.Commands;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly UsersDbContext _dbContext;

    public AuditLogRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog);
    }

    public async Task<IReadOnlyList<AuditLog>> GetAuditLogsAsync(Guid userId, int? limit = null, int daysBack = 90)
    {
        var since = DateTime.UtcNow.AddDays(-daysBack);
        var query = _dbContext.AuditLogs
            .Where(x => x.UserId == userId && x.OccurredAtUtc >= since)
            .OrderByDescending(x => x.OccurredAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<AuditLog>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetEntityAuditLogsAsync(string entityName, Guid entityId, int? limit = null)
    {
        var query = _dbContext.AuditLogs
            .Where(x => x.EntityName == entityName && x.EntityId == entityId)
            .OrderByDescending(x => x.OccurredAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<AuditLog>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetOperationAuditLogsAsync(string operationType, int? limit = null, int daysBack = 90)
    {
        var since = DateTime.UtcNow.AddDays(-daysBack);
        var query = _dbContext.AuditLogs
            .Where(x => x.OperationType == operationType && x.OccurredAtUtc >= since)
            .OrderByDescending(x => x.OccurredAtUtc);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<AuditLog>)query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    public async Task DeleteOldLogsAsync(int daysToKeep = 365)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var oldLogs = await _dbContext.AuditLogs
            .Where(x => x.OccurredAtUtc < cutoffDate)
            .ToListAsync();

        if (oldLogs.Any())
        {
            _dbContext.AuditLogs.RemoveRange(oldLogs);
        }
    }
}