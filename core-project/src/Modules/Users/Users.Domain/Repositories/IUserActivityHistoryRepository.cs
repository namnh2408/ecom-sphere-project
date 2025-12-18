using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IUserActivityHistoryRepository
{
    Task AddAsync(UserActivityHistory activity);
    Task<IReadOnlyList<UserActivityHistory>> GetUserActivitiesAsync(Guid userId, int? limit = null, int daysBack = 30);
    Task<IReadOnlyList<UserActivityHistory>> GetActivitiesByTypeAsync(string activityType, int? limit = null, int daysBack = 30);
    Task<int> GetActivityCountByTypeAsync(Guid userId, string activityType, int daysBack = 30);
    Task DeleteOldActivitiesAsync(int daysToKeep = 365);
}