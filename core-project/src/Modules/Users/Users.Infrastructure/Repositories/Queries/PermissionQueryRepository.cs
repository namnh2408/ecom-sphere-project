using BuildingBlocks.Infrastructure.Shared.Dapper;
using Users.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace Users.Infrastructure.Repositories.Queries;

/// <summary>
/// Read-only repository for Permission queries using Dapper
/// </summary>
public class PermissionQueryRepository : DapperRepository<PermissionDto>
{
    public PermissionQueryRepository(IDapperConnectionProvider connectionProvider, ILogger<PermissionQueryRepository> logger)
        : base(connectionProvider, logger)
    {
    }

    /// <summary>
    /// Get all active permissions
    /// </summary>
    public async Task<IEnumerable<PermissionDto>> GetAllActivePermissionsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Name,
                Description,
                Resource,
                Action,
                IsActive,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM Permissions
            WHERE IsActive = 1
            ORDER BY Resource, Action";

        return await QueryAsync<PermissionDto>(sql, null, cancellationToken);
    }

    /// <summary>
    /// Get all permissions with pagination
    /// </summary>
    public async Task<(IEnumerable<PermissionDto> Permissions, int TotalCount)> GetPagedPermissionsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string countSql = "SELECT COUNT(*) FROM Permissions";
        var totalCount = await QueryScalarAsync(countSql, null, cancellationToken);

        const string sql = @"
            SELECT 
                Id,
                Name,
                Description,
                Resource,
                Action,
                IsActive,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM Permissions
            ORDER BY Resource, Action
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new { PageSize = pageSize, Offset = (pageNumber - 1) * pageSize };
        var permissions = await QueryAsync<PermissionDto>(sql, parameters, cancellationToken);

        return (permissions, Convert.ToInt32(totalCount ?? 0));
    }

    /// <summary>
    /// Get permission by ID
    /// </summary>
    public async Task<PermissionDto?> GetPermissionByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Name,
                Description,
                Resource,
                Action,
                IsActive,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM Permissions
            WHERE Id = @PermissionId";

        return await QuerySingleOrDefaultAsync<PermissionDto>(sql, new { PermissionId = permissionId }, cancellationToken);
    }

    /// <summary>
    /// Get permissions by role ID
    /// </summary>
    public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT DISTINCT
                p.Id,
                p.Name,
                p.Description,
                p.Resource,
                p.Action,
                p.IsActive,
                p.CreatedAtUtc,
                p.UpdatedAtUtc
            FROM Permissions p
            WHERE p.Id IN (
                SELECT value FROM STRING_SPLIT((
                    SELECT PermissionIds FROM Roles WHERE Id = @RoleId
                ), ',')
            )
            ORDER BY p.Resource, p.Action";

        return await QueryAsync<PermissionDto>(sql, new { RoleId = roleId }, cancellationToken);
    }

    /// <summary>
    /// Get permissions by IDs
    /// </summary>
    public async Task<IEnumerable<PermissionDto>> GetPermissionsByIdsAsync(IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Name,
                Description,
                Resource,
                Action,
                IsActive,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM Permissions
            WHERE Id IN (SELECT value FROM STRING_SPLIT(@PermissionIds, ','))
            ORDER BY Resource, Action";

        var permIdString = string.Join(",", permissionIds);
        return await QueryAsync<PermissionDto>(sql, new { PermissionIds = permIdString }, cancellationToken);
    }

    /// <summary>
    /// Check if permission exists by resource and action
    /// </summary>
    public async Task<bool> PermissionExistsByResourceActionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(*) FROM Permissions WHERE Resource = @Resource AND Action = @Action";
        var count = await QueryScalarAsync(sql, new { Resource = resource, Action = action }, cancellationToken);
        return Convert.ToInt32(count ?? 0) > 0;
    }

    /// <summary>
    /// Get all permission by resource
    /// </summary>
    public async Task<IEnumerable<PermissionDto>> GetPermissionsByResourceAsync(string resource, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                Name,
                Description,
                Resource,
                Action,
                IsActive,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM Permissions
            WHERE Resource = @Resource AND IsActive = 1
            ORDER BY Action";

        return await QueryAsync<PermissionDto>(sql, new { Resource = resource }, cancellationToken);
    }
}