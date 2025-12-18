using BuildingBlocks.Infrastructure.Shared.Dapper;
using Users.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Users.Infrastructure.Repositories.Queries;

/// <summary>
/// Read-only repository for Activity History queries using Dapper
/// Optimized for historical data retrieval and analysis
/// </summary>
public class ActivityHistoryQueryRepository : DapperRepository<UserActivityHistoryDto>
{
    public ActivityHistoryQueryRepository(IDapperConnectionProvider connectionProvider, ILogger<ActivityHistoryQueryRepository> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Get user activity history with pagination
    /// </summary>
    public async Task<(IEnumerable<UserActivityHistoryDto> Activities, int TotalCount)> GetUserActivityHistoryAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        string? activityType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var whereConditions = new List<string> { "UserId = @UserId" };
        var parameters = new Dictionary<string, object> { { "UserId", userId }, { "PageSize", pageSize }, { "Offset", (pageNumber - 1) * pageSize } };

        if (!string.IsNullOrWhiteSpace(activityType))
        {
            whereConditions.Add("ActivityType = @ActivityType");
            parameters["ActivityType"] = activityType;
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
        var countSql = $"SELECT COUNT(*) FROM UserActivityHistories WHERE {whereClause}";
        var totalCount = await QueryScalarAsync(countSql, parameters, cancellationToken);

        // Get paginated data
        const string sql = @"
            SELECT 
                Id,
                UserId,
                ActivityType,
                Description,
                IpAddress,
                UserAgent,
                OccurredAtUtc,
                Metadata
            FROM UserActivityHistories
            WHERE {whereClause}
            ORDER BY OccurredAtUtc DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var activities = await QueryAsync<UserActivityHistoryDto>(
            sql.Replace("{whereClause}", whereClause), 
            parameters, 
            cancellationToken);

        return (activities, Convert.ToInt32(totalCount ?? 0));
    }

    /// <summary>
    /// Get login history for a user
    /// </summary>
    public async Task<(IEnumerable<LoginHistoryDto> Logins, int TotalCount)> GetLoginHistoryAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string countSql = "SELECT COUNT(*) FROM LoginAttempts WHERE UserId = @UserId";
        var totalCount = await QueryScalarAsync(countSql, new { UserId = userId }, cancellationToken);

        const string sql = @"
            SELECT 
                Id,
                UserId,
                Email,
                IsSuccessful,
                FailureReason,
                IpAddress,
                UserAgent,
                AttemptedAtUtc
            FROM LoginAttempts
            WHERE UserId = @UserId
            ORDER BY AttemptedAtUtc DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new { UserId = userId, PageSize = pageSize, Offset = (pageNumber - 1) * pageSize };
        var logins = await QueryAsync<LoginHistoryDto>(sql, parameters, cancellationToken);

        return (logins, Convert.ToInt32(totalCount ?? 0));
    }

    /// <summary>
    /// Get recent successful logins for a user
    /// </summary>
    public async Task<IEnumerable<LoginHistoryDto>> GetRecentSuccessfulLoginsAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TOP @Count
                Id,
                UserId,
                Email,
                IsSuccessful,
                FailureReason,
                IpAddress,
                UserAgent,
                AttemptedAtUtc
            FROM LoginAttempts
            WHERE UserId = @UserId AND IsSuccessful = 1
            ORDER BY AttemptedAtUtc DESC";

        return await QueryAsync<LoginHistoryDto>(sql, new { UserId = userId, Count = count }, cancellationToken);
    }

    /// <summary>
    /// Count failed login attempts in the last N minutes
    /// </summary>
    public async Task<int> GetRecentFailedLoginAttemptsAsync(Guid userId, int minutesBack = 30, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM LoginAttempts
            WHERE UserId = @UserId 
            AND IsSuccessful = 0 
            AND AttemptedAtUtc >= DATEADD(MINUTE, -@MinutesBack, GETUTCDATE())";

        var count = await QueryScalarAsync(sql, new { UserId = userId, MinutesBack = minutesBack }, cancellationToken);
        return Convert.ToInt32(count ?? 0);
    }
}