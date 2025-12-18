using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IChangeHistoryRepository
{
    Task AddAsync(ChangeHistory changeHistory);
    Task<IReadOnlyList<ChangeHistory>> GetUserChangesAsync(Guid userId, int? limit = null, int daysBack = 90);
    Task<IReadOnlyList<ChangeHistory>> GetFieldChangesAsync(Guid userId, string fieldName, int? limit = null);
    Task<ChangeHistory?> GetLatestChangeAsync(Guid userId, string fieldName);
    Task DeleteOldChangesAsync(int daysToKeep = 365);
}