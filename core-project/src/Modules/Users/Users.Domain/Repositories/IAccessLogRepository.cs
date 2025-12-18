using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IAccessLogRepository
{
    Task AddAsync(AccessLog accessLog);
    Task<IReadOnlyList<AccessLog>> GetUserAccessLogsAsync(Guid userId, int? limit = null, int daysBack = 30);
    Task<IReadOnlyList<AccessLog>> GetResourceAccessLogsAsync(string resourceName, int? limit = null, int daysBack = 30);
    Task<IReadOnlyList<AccessLog>> GetFailedAccessLogsAsync(Guid userId, int? limit = null, int minutesBack = 60);
    Task<int> GetAccessCountAsync(Guid userId, string resourceName, int hoursBack = 24);
    Task DeleteOldLogsAsync(int daysToKeep = 30);
}