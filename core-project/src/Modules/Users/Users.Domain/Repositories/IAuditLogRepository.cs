using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<IReadOnlyList<AuditLog>> GetAuditLogsAsync(Guid userId, int? limit = null, int daysBack = 90);
    Task<IReadOnlyList<AuditLog>> GetEntityAuditLogsAsync(string entityName, Guid entityId, int? limit = null);
    Task<IReadOnlyList<AuditLog>> GetOperationAuditLogsAsync(string operationType, int? limit = null, int daysBack = 90);
    Task DeleteOldLogsAsync(int daysToKeep = 365);
}