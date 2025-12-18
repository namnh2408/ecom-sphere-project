using BuildingBlocks.Infrastructure.Shared.Dapper;
using Users.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Users.Infrastructure.Repositories.Queries;

/// <summary>
/// Read-only repository for Role queries using Dapper
/// </summary>
public class RoleQueryRepository : DapperRepository<RoleDto>
{
    public RoleQueryRepository(IDapperConnectionProvider connectionProvider, ILogger<RoleQueryRepository> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Get all active roles
    /// </summary>
    public async Task<IEnumerable<RoleDto>> GetAllActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Name,
                Description,
                IsActive,
                PermissionIds,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM Roles
            WHERE IsActive = 1
            ORDER BY Name";

        return await QueryAsync<RoleDto>(sql, null, cancellationToken);
    }

    /// <summary>
    /// Get all roles with pagination
    /// </summary>
    public async Task<(IEnumerable<RoleDto> Roles, int TotalCount)> GetPagedRolesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string countSql = "SELECT COUNT(*) FROM Roles";
        var totalCount = await QueryScalarAsync(countSql, null, cancellationToken);

        const string sql = @"
            SELECT 
                Id,
                Name,
                Description,
                IsActive,
                PermissionIds,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM Roles
            ORDER BY Name
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new { PageSize = pageSize, Offset = (pageNumber - 1) * pageSize };
        var roles = await QueryAsync<RoleDto>(sql, parameters, cancellationToken);

        return (roles, Convert.ToInt32(totalCount ?? 0));
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    public async Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Name,
                Description,
                IsActive,
                PermissionIds,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM Roles
            WHERE Id = @RoleId";

        return await QuerySingleOrDefaultAsync<RoleDto>(sql, new { RoleId = roleId }, cancellationToken);
    }

    /// <summary>
    /// Check if role exists by ID
    /// </summary>
    public async Task<bool> RoleExistsByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(*) FROM Roles WHERE Id = @RoleId";
        var count = await QueryScalarAsync(sql, new { RoleId = roleId }, cancellationToken);
        return Convert.ToInt32(count ?? 0) > 0;
    }

    /// <summary>
    /// Check if role exists by name
    /// </summary>
    public async Task<bool> RoleExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(*) FROM Roles WHERE Name = @Name";
        var count = await QueryScalarAsync(sql, new { Name = name }, cancellationToken);
        return Convert.ToInt32(count ?? 0) > 0;
    }

    /// <summary>
    /// Get roles by IDs
    /// </summary>
    public async Task<IEnumerable<RoleDto>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Name,
                Description,
                IsActive,
                PermissionIds,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM Roles
            WHERE Id IN (SELECT value FROM STRING_SPLIT(@RoleIds, ','))
            ORDER BY Name";

        var roleIdString = string.Join(",", roleIds);
        return await QueryAsync<RoleDto>(sql, new { RoleIds = roleIdString }, cancellationToken);
    }
}