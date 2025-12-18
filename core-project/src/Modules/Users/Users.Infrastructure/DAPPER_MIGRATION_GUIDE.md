# Dapper Integration Guide

## Overview

This project now supports **Dapper ORM** alongside Entity Framework Core for optimized read operations. Dapper provides:

- ✅ **Higher Performance**: Direct SQL execution without tracking overhead
- ✅ **Query Optimization**: Fine-grained SQL control
- ✅ **Separate Connection Pool**: Independent from EF Core
- ✅ **CQRS Pattern**: Read and Write separation

## Architecture

```
┌─────────────────────────────────────────────┐
│         Query Handlers (MediatR)            │
│  (GetAllUsers, GetUserById, etc.)           │
└────────────┬────────────────────────────────┘
             │
    ┌────────┴──────────┐
    │                   │
    ▼                   ▼
[Write Operations] [Read Operations]
   (EF Core)          (Dapper)
    │                   │
    ├──────────┬────────┤
    │          │        │
    ▼          ▼        ▼
[DbContext] [DapperConnectionProvider]
   │              │
   └──────────────┴────────────────────────► SQL Server
```

## Available Query Repositories

### 1. UserQueryRepository
Handles all read operations for users.

**Methods:**
- `GetUserByIdAsync(userId)` - Get single user by ID
- `GetUserByEmailAsync(email)` - Get user by email
- `GetPagedUsersAsync(pageNumber, pageSize, searchTerm, isActive, isEmailVerified)` - Paginated users with filters
- `GetActiveUsersAsync()` - Get only active users
- `UserExistsByEmailAsync(email)` - Check email existence
- `UserExistsByIdAsync(userId)` - Check user existence
- `GetUsersByRoleIdAsync(roleId)` - Get users with specific role

### 2. RoleQueryRepository
Handles all read operations for roles.

**Methods:**
- `GetAllActiveRolesAsync()` - Get all active roles
- `GetPagedRolesAsync(pageNumber, pageSize)` - Paginated roles
- `GetRoleByIdAsync(roleId)` - Get single role
- `RoleExistsByIdAsync(roleId)` - Check role existence
- `RoleExistsByNameAsync(name)` - Check role name existence
- `GetRolesByIdsAsync(roleIds)` - Get multiple roles

### 3. PermissionQueryRepository
Handles all read operations for permissions.

**Methods:**
- `GetAllActivePermissionsAsync()` - Get all active permissions
- `GetPagedPermissionsAsync(pageNumber, pageSize)` - Paginated permissions
- `GetPermissionByIdAsync(permissionId)` - Get single permission
- `GetPermissionsByRoleIdAsync(roleId)` - Get permissions for a role
- `GetPermissionsByIdsAsync(permissionIds)` - Get multiple permissions
- `PermissionExistsByResourceActionAsync(resource, action)` - Check permission
- `GetPermissionsByResourceAsync(resource)` - Get permissions by resource

### 4. ActivityHistoryQueryRepository
Handles user activity and login history queries.

**Methods:**
- `GetUserActivityHistoryAsync(userId, pageNumber, pageSize, activityType, fromDate, toDate)` - User activities
- `GetLoginHistoryAsync(userId, pageNumber, pageSize)` - Login attempts
- `GetRecentSuccessfulLoginsAsync(userId, count)` - Recent logins
- `GetRecentFailedLoginAttemptsAsync(userId, minutesBack)` - Failed attempts

### 5. AuditLogQueryRepository
Handles audit and change history queries.

**Methods:**
- `GetUserAuditLogsAsync(userId, pageNumber, pageSize, operationType, fromDate, toDate)` - User audit logs
- `GetEntityAuditLogsAsync(entityName, entityId, pageNumber, pageSize)` - Entity audit logs
- `GetUserOperationHistoryAsync(targetUserId, pageSize)` - All operations on a user
- `GetUserChangeHistoryAsync(userId, pageNumber, pageSize, fromDate, toDate)` - Change history

## Migration Examples

### Example 1: Convert GetAllUsersQueryHandler to Dapper

#### Before (EF Core):
```csharp
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PaginatedResult<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PaginatedResult<UserDto>>> Handle(
        GetAllUsersQuery request, 
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);

        // Apply filters in memory (inefficient for large datasets)
        var filtered = users.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            filtered = filtered.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Email.Value.ToLower().Contains(searchTerm)
            );
        }

        if (request.IsActive.HasValue)
        {
            filtered = filtered.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (request.IsEmailVerified.HasValue)
        {
            filtered = filtered.Where(u => u.IsEmailVerified == request.IsEmailVerified.Value);
        }

        var totalCount = filtered.Count();
        var paginatedUsers = filtered
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to DTOs
        var userDtos = paginatedUsers.Select(u => new UserDto(
            u.Id, u.Email.Value, u.FirstName, u.LastName,
            u.IsActive, u.IsEmailVerified, u.RoleIds,
            u.CreatedAtUtc, u.UpdatedAtUtc, u.LastLoginAtUtc
        )).ToList();

        var paginatedResult = PaginatedResult<UserDto>.Create(
            userDtos, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedResult<UserDto>>.Success(paginatedResult);
    }
}
```

#### After (Dapper):
```csharp
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PaginatedResult<UserDto>>>
{
    private readonly UserQueryRepository _userQueryRepository;

    public GetAllUsersQueryHandler(UserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }

    public async Task<Result<PaginatedResult<UserDto>>> Handle(
        GetAllUsersQuery request, 
        CancellationToken cancellationToken)
    {
        // All filtering and pagination is done at database level
        var (users, totalCount) = await _userQueryRepository.GetPagedUsersAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.IsActive,
            request.IsEmailVerified,
            cancellationToken);

        var paginatedResult = PaginatedResult<UserDto>.Create(
            users.ToList(), 
            totalCount, 
            request.PageNumber, 
            request.PageSize);

        return Result<PaginatedResult<UserDto>>.Success(paginatedResult);
    }
}
```

**Benefits:**
- ✅ Filtering at database level (faster)
- ✅ Pagination in SQL (less memory)
- ✅ Direct DTO mapping (no domain-to-DTO conversion)
- ✅ Simpler, more readable code

### Example 2: Convert GetUserByIdQueryHandler

#### Before (EF Core):
```csharp
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user is null)
            return Result<UserDto>.Failure(Error.NotFound("User not found"));

        var userDto = new UserDto(
            user.Id, user.Email.Value, user.FirstName, user.LastName,
            user.IsActive, user.IsEmailVerified, user.RoleIds,
            user.CreatedAtUtc, user.UpdatedAtUtc, user.LastLoginAtUtc);

        return Result<UserDto>.Success(userDto);
    }
}
```

#### After (Dapper):
```csharp
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly UserQueryRepository _userQueryRepository;

    public GetUserByIdQueryHandler(UserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }

    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var userDto = await _userQueryRepository.GetUserByIdAsync(
            request.UserId, 
            cancellationToken);

        if (userDto is null)
            return Result<UserDto>.Failure(Error.NotFound("User not found"));

        return Result<UserDto>.Success(userDto);
    }
}
```

**Benefits:**
- ✅ Direct DTO retrieval (no domain entity loading)
- ✅ Simpler code (no mapping required)
- ✅ Better performance (fewer objects in memory)

### Example 3: Using Dapper with Custom Queries

If you need custom queries not covered by existing repositories:

```csharp
public class CustomUserReportQueryHandler : IRequestHandler<CustomUserReportQuery, Result<UserReportDto>>
{
    private readonly IDapperConnectionProvider _connectionProvider;

    public CustomUserReportQueryHandler(IDapperConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<Result<UserReportDto>> Handle(
        CustomUserReportQuery request, 
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                COUNT(*) as TotalUsers,
                COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveUsers,
                COUNT(CASE WHEN IsEmailVerified = 1 THEN 1 END) as VerifiedUsers,
                AVG(DATEDIFF(DAY, CreatedAtUtc, GETUTCDATE())) as AvgAccountAgeInDays
            FROM Users
            WHERE IsDeleted = 0";

        using var connection = await _connectionProvider.CreateConnectionAsync(cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<UserReportDto>(sql);

        if (result is null)
            return Result<UserReportDto>.Failure(Error.InternalServerError("Failed to generate report"));

        return Result<UserReportDto>.Success(result);
    }
}
```

## Query Performance Tips

### 1. Use Specific Columns
```csharp
// ✅ Good - Only needed columns
SELECT Id, Email, FirstName, LastName FROM Users WHERE Id = @UserId

// ❌ Avoid - Loads unnecessary data
SELECT * FROM Users WHERE Id = @UserId
```

### 2. Filter at Database Level
```csharp
// ✅ Good - Filter in SQL
WHERE Email LIKE @SearchTerm AND IsActive = @IsActive

// ❌ Avoid - Filter in C#
.Where(u => u.Email.Contains(searchTerm) && u.IsActive == isActive)
```

### 3. Use Pagination Correctly
```csharp
// ✅ Good - SQL pagination
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY

// ❌ Avoid - Memory pagination
.Skip((pageNumber - 1) * pageSize).Take(pageSize)
```

### 4. Index Important Columns
```sql
-- For UserQueryRepository.GetPagedUsersAsync
CREATE INDEX IX_Users_IsActive_IsDeleted ON Users(IsActive, IsDeleted)
CREATE INDEX IX_Users_Email_IsDeleted ON Users(Email, IsDeleted)

-- For ActivityHistoryQueryRepository.GetRecentFailedLoginAttemptsAsync
CREATE INDEX IX_LoginAttempts_UserId_IsSuccessful_Time 
    ON LoginAttempts(UserId, IsSuccessful, AttemptedAtUtc)
```

## CQRS Pattern Integration

The Dapper integration reinforces the CQRS pattern:

```
Commands (Write) ──► EF Core Repositories ──► Transaction & Change Tracking
                                        │
                                        └──► Domain Events
                                        
Queries (Read) ──► Dapper Repositories ──► Direct SQL ──► DTOs
                                   │
                                   └──► No Tracking
                                   └──► No Change Detection
```

## Migration Checklist

When migrating a query handler to Dapper:

- [ ] Identify the query handler and current repository usage
- [ ] Check if a Dapper repository already exists for the entity
- [ ] If not, create a new Dapper repository inheriting from `DapperRepository<TDto>`
- [ ] Write SQL queries with proper filtering and pagination
- [ ] Update the query handler to use the new Dapper repository
- [ ] Verify the query returns correct DTOs
- [ ] Remove EF Core domain entity mapping logic
- [ ] Add unit tests for the new query handler
- [ ] Update logging if needed
- [ ] Document the query for future reference

## Performance Monitoring

Monitor query performance using SQL Server:

```sql
-- Check query execution time
SET STATISTICS TIME ON
-- Your query here
SET STATISTICS TIME OFF

-- Check execution plan
SET STATISTICS IO ON
-- Your query here
SET STATISTICS IO OFF
```

## Troubleshooting

### Issue: "Connection string 'DefaultConnection' not found"
**Solution**: Ensure `appsettings.json` has a `ConnectionStrings:DefaultConnection` entry.

### Issue: "Timeout expired"
**Solution**: Check SQL query performance, add indexes to frequently queried columns.

### Issue: "DTO mapping fails"
**Solution**: Ensure column names in SQL match DTO property names (case-insensitive by default).

## Next Steps

1. ✅ Migrate read-heavy query handlers first
2. ✅ Keep write operations with EF Core (transactions, change tracking)
3. ✅ Monitor query performance
4. ✅ Add Dapper repositories for other modules (Catalog, Orders, etc.)
5. ✅ Create cross-module Dapper repositories for complex queries

## References

- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)