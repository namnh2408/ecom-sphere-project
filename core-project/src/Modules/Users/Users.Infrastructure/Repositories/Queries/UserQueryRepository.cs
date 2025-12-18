using BuildingBlocks.Abstractions;
using BuildingBlocks.Infrastructure.Shared.Dapper;
using Users.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Users.Infrastructure.Repositories.Queries;

/// <summary>
/// Read-only repository for User queries using Dapper
/// Optimized for query performance with direct SQL mapping
/// </summary>
public class UserQueryRepository : DapperRepository<UserDto>
{
    public UserQueryRepository(IDapperConnectionProvider connectionProvider, ILogger<UserQueryRepository> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Get user by ID with all related data
    /// </summary>
    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Email,
                FirstName,
                LastName,
                IsActive,
                IsEmailVerified,
                RoleIds,
                CreatedAtUtc,
                UpdatedAtUtc,
                LastLoginAtUtc
            FROM Users
            WHERE Id = @UserId AND IsDeleted = 0";

        return await QuerySingleOrDefaultAsync<UserDto>(sql, new { UserId = userId }, cancellationToken);
    }

    /// <summary>
    /// Get user by email address
    /// </summary>
    public async Task<UserDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Email,
                FirstName,
                LastName,
                IsActive,
                IsEmailVerified,
                RoleIds,
                CreatedAtUtc,
                UpdatedAtUtc,
                LastLoginAtUtc
            FROM Users
            WHERE Email = @Email AND IsDeleted = 0";

        return await QuerySingleOrDefaultAsync<UserDto>(sql, new { Email = email.ToLowerInvariant() }, cancellationToken);
    }

    /// <summary>
    /// Get paginated users with optional filtering
    /// </summary>
    public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetPagedUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        bool? isEmailVerified = null,
        CancellationToken cancellationToken = default)
    {
        // Build dynamic WHERE clause
        var whereConditions = new List<string> { "IsDeleted = 0" };
        var parameters = new Dictionary<string, object> { { "PageSize", pageSize }, { "Offset", (pageNumber - 1) * pageSize } };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereConditions.Add("(FirstName LIKE @SearchTerm OR LastName LIKE @SearchTerm OR Email LIKE @SearchTerm)");
            parameters["SearchTerm"] = $"%{searchTerm.ToLowerInvariant()}%";
        }

        if (isActive.HasValue)
        {
            whereConditions.Add("IsActive = @IsActive");
            parameters["IsActive"] = isActive.Value;
        }

        if (isEmailVerified.HasValue)
        {
            whereConditions.Add("IsEmailVerified = @IsEmailVerified");
            parameters["IsEmailVerified"] = isEmailVerified.Value;
        }

        var whereClause = string.Join(" AND ", whereConditions);

        // Get total count
        var countSql = $@"SELECT COUNT(*) FROM Users WHERE {whereClause}";
        var totalCount = await QueryScalarAsync(countSql, parameters, cancellationToken);

        // Get paginated users
        var sql = $@"
            SELECT 
                Id,
                Email,
                FirstName,
                LastName,
                IsActive,
                IsEmailVerified,
                RoleIds,
                CreatedAtUtc,
                UpdatedAtUtc,
                LastLoginAtUtc
            FROM Users
            WHERE {whereClause}
            ORDER BY CreatedAtUtc DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var users = await QueryAsync<UserDto>(sql, parameters, cancellationToken);
        
        return (users, Convert.ToInt32(totalCount ?? 0));
    }

    /// <summary>
    /// Get all active users (no pagination)
    /// </summary>
    public async Task<IEnumerable<UserDto>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Email,
                FirstName,
                LastName,
                IsActive,
                IsEmailVerified,
                RoleIds,
                CreatedAtUtc,
                UpdatedAtUtc,
                LastLoginAtUtc
            FROM Users
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY FirstName, LastName";

        return await QueryAsync<UserDto>(sql, null, cancellationToken);
    }

    /// <summary>
    /// Check if user exists by email
    /// </summary>
    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT COUNT(*) FROM Users WHERE Email = @Email AND IsDeleted = 0";
        var count = await QueryScalarAsync(sql, new { Email = email.ToLowerInvariant() }, cancellationToken);
        return Convert.ToInt32(count ?? 0) > 0;
    }

    /// <summary>
    /// Check if user exists by ID
    /// </summary>
    public async Task<bool> UserExistsByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT COUNT(*) FROM Users WHERE Id = @UserId AND IsDeleted = 0";
        var count = await QueryScalarAsync(sql, new { UserId = userId }, cancellationToken);
        return Convert.ToInt32(count ?? 0) > 0;
    }

    /// <summary>
    /// Get users by role ID
    /// </summary>
    public async Task<IEnumerable<UserDto>> GetUsersByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Email,
                FirstName,
                LastName,
                IsActive,
                IsEmailVerified,
                RoleIds,
                CreatedAtUtc,
                UpdatedAtUtc,
                LastLoginAtUtc
            FROM Users
            WHERE RoleIds LIKE @RoleId AND IsDeleted = 0
            ORDER BY FirstName, LastName";

        // Search for RoleId in comma-separated list
        return await QueryAsync<UserDto>(sql, new { RoleId = $"%{roleId}%" }, cancellationToken);
    }
}