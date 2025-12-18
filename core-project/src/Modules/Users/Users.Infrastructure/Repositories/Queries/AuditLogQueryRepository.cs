using BuildingBlocks.Infrastructure.Shared.Dapper;
using Users.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Users.Infrastructure.Repositories.Queries;

/// <summary>
/// Read-only repository for Audit Log queries using Dapper
/// Optimized for compliance and audit trail retrieval
/// </summary>
public class AuditLogQueryRepository : DapperRepository<AuditLogDto>
{
    public AuditLogQueryRepository(IDapperConnectionProvider connectionProvider, ILogger<AuditLogQueryRepository> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Get audit logs for a specific user with pagination
    /// </summary>
    public async Task<(IEnumerable<AuditLogDto> Logs, int TotalCount)> GetUserAuditLogsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? operationType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var whereConditions = new List<string> { "UserId = @UserId" };
        var parameters = new Dictionary<string, object>
        {
            { "UserId", userId },
            { "PageSize", pageSize },
            { "Offset", (pageNumber - 1) * pageSize }
        };

        if (!string.IsNullOrWhiteSpace(operationType))
        {
            whereConditions.Add("OperationType = @OperationType");
            parameters["OperationType"] = operationType;
        }

        if (fromDate.HasValue)
        {
            whereConditions.Add("OccurredAtUtc >= @FromDate");
            parameters["FromDate"] = fromDate.Value;
        }

        if (toDate.HasValue)
        {
            whereConditions.Add("OccurredAtUtc <= @ToDate");
            parameters["ToDate"] = toDate.Value;
        }

        var whereClause = string.Join(" AND ", whereConditions);

        // Get total count
        var countSql = $"SELECT COUNT(*) FROM AuditLogs WHERE {whereClause}";
        var totalCount = await QueryScalarAsync(countSql, parameters, cancellationToken);

        // Get paginated data
        const string sql = @"
            SELECT 
                Id,
                UserId,
                EntityName,
                EntityId,
                OperationType,
                OldValues,
                NewValues,
                Description,
                IpAddress,
                OccurredAtUtc
            FROM AuditLogs
            WHERE {whereClause}
            ORDER BY OccurredAtUtc DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var logs = await QueryAsync<AuditLogDto>(
            sql.Replace("{whereClause}", whereClause),
            parameters,
            cancellationToken);

        return (logs, Convert.ToInt32(totalCount ?? 0));
    }

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    public async Task<(IEnumerable<AuditLogDto> Logs, int TotalCount)> GetEntityAuditLogsAsync(
        string entityName,
        Guid entityId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string countSql = @"
            SELECT COUNT(*) FROM AuditLogs 
            WHERE EntityName = @EntityName AND EntityId = @EntityId";
        
        var totalCount = await QueryScalarAsync(
            countSql,
            new { EntityName = entityName, EntityId = entityId },
            cancellationToken);

        const string sql = @"
            SELECT 
                Id,
                UserId,
                EntityName,
                EntityId,
                OperationType,
                OldValues,
                NewValues,
                Description,
                IpAddress,
                OccurredAtUtc
            FROM AuditLogs
            WHERE EntityName = @EntityName AND EntityId = @EntityId
            ORDER BY OccurredAtUtc DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new { EntityName = entityName, EntityId = entityId, PageSize = pageSize, Offset = (pageNumber - 1) * pageSize };
        var logs = await QueryAsync<AuditLogDto>(sql, parameters, cancellationToken);

        return (logs, Convert.ToInt32(totalCount ?? 0));
    }

    /// <summary>
    /// Get all operations performed on a specific user
    /// </summary>
    public async Task<IEnumerable<AuditLogDto>> GetUserOperationHistoryAsync(
        Guid targetUserId,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TOP @PageSize
                Id,
                UserId,
                EntityName,
                EntityId,
                OperationType,
                OldValues,
                NewValues,
                Description,
                IpAddress,
                OccurredAtUtc
            FROM AuditLogs
            WHERE EntityId = @EntityId AND EntityName = 'User'
            ORDER BY OccurredAtUtc DESC";

        return await QueryAsync<AuditLogDto>(sql, new { EntityId = targetUserId, PageSize = pageSize }, cancellationToken);
    }

    /// <summary>
    /// Get recent changes for a specific user
    /// </summary>
    public async Task<(IEnumerable<ChangeHistoryDto> Changes, int TotalCount)> GetUserChangeHistoryAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var whereConditions = new List<string> { "UserId = @UserId" };
        var parameters = new Dictionary<string, object>
        {
            { "UserId", userId },
            { "PageSize", pageSize },
            { "Offset", (pageNumber - 1) * pageSize }
        };

        if (fromDate.HasValue)
        {
            whereConditions.Add("ChangedAtUtc >= @FromDate");
            parameters["FromDate"] = fromDate.Value;
        }

        if (toDate.HasValue)
        {
            whereConditions.Add("ChangedAtUtc <= @ToDate");
            parameters["ToDate"] = toDate.Value;
        }

        var whereClause = string.Join(" AND ", whereConditions);

        // Get total count
        var countSql = $"SELECT COUNT(*) FROM ChangeHistories WHERE {whereClause}";
        var totalCount = await QueryScalarAsync(countSql, parameters, cancellationToken);

        // Get paginated data
        const string sql = @"
            SELECT 
                Id,
                UserId,
                FieldName,
                OldValue,
                NewValue,
                ChangeReason,
                IpAddress,
                ChangedAtUtc,
                IsReversible
            FROM ChangeHistories
            WHERE {whereClause}
            ORDER BY ChangedAtUtc DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var changes = await QueryAsync<ChangeHistoryDto>(
            sql.Replace("{whereClause}", whereClause),
            parameters,
            cancellationToken);

        return (changes, Convert.ToInt32(totalCount ?? 0));
    }
}