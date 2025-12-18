# Dapper Integration Quick Start Guide

## 5-Minute Overview

Dapper is now integrated for **READ operations** (queries). EF Core is still used for **WRITE operations** (commands).

### Why Dapper?
- ✅ **50-70% faster** for read queries
- ✅ **Less memory** - no object tracking
- ✅ **Direct SQL** - fine-grained control
- ✅ **CQRS pattern** - read/write separation

---

## Using Dapper Query Repositories

### 1. Inject Query Repository

```csharp
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly UserQueryRepository _userQueryRepository;  // ← Inject this

    public GetUserByIdQueryHandler(UserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }

    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery request, 
        CancellationToken cancellationToken)
    {
        // Use the repository instead of domain repository
        var userDto = await _userQueryRepository.GetUserByIdAsync(
            request.UserId, 
            cancellationToken);

        if (userDto is null)
            return Result<UserDto>.Failure(Error.NotFound("User not found"));

        return Result<UserDto>.Success(userDto);
    }
}
```

### 2. Available Query Repositories

#### UserQueryRepository
```csharp
GetUserByIdAsync(userId)
GetUserByEmailAsync(email)
GetPagedUsersAsync(pageNumber, pageSize, searchTerm, isActive, isEmailVerified)
GetActiveUsersAsync()
UserExistsByEmailAsync(email)
UserExistsByIdAsync(userId)
GetUsersByRoleIdAsync(roleId)
```

#### RoleQueryRepository
```csharp
GetAllActiveRolesAsync()
GetPagedRolesAsync(pageNumber, pageSize)
GetRoleByIdAsync(roleId)
RoleExistsByIdAsync(roleId)
RoleExistsByNameAsync(name)
GetRolesByIdsAsync(roleIds)
```

#### PermissionQueryRepository
```csharp
GetAllActivePermissionsAsync()
GetPagedPermissionsAsync(pageNumber, pageSize)
GetPermissionByIdAsync(permissionId)
GetPermissionsByRoleIdAsync(roleId)
GetPermissionsByIdsAsync(permissionIds)
PermissionExistsByResourceActionAsync(resource, action)
GetPermissionsByResourceAsync(resource)
```

#### ActivityHistoryQueryRepository
```csharp
GetUserActivityHistoryAsync(userId, pageNumber, pageSize, activityType, fromDate, toDate)
GetLoginHistoryAsync(userId, pageNumber, pageSize)
GetRecentSuccessfulLoginsAsync(userId, count)
GetRecentFailedLoginAttemptsAsync(userId, minutesBack)
```

#### AuditLogQueryRepository
```csharp
GetUserAuditLogsAsync(userId, pageNumber, pageSize, operationType, fromDate, toDate)
GetEntityAuditLogsAsync(entityName, entityId, pageNumber, pageSize)
GetUserOperationHistoryAsync(targetUserId, pageSize)
GetUserChangeHistoryAsync(userId, pageNumber, pageSize, fromDate, toDate)
```

---

## Creating Custom Dapper Queries

### Simple Example

```csharp
public class UserReportQueryHandler : IRequestHandler<UserReportQuery, Result<UserReportDto>>
{
    private readonly IDapperConnectionProvider _connectionProvider;

    public UserReportQueryHandler(IDapperConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<Result<UserReportDto>> Handle(
        UserReportQuery request, 
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                COUNT(*) as TotalUsers,
                COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveUsers,
                COUNT(CASE WHEN IsEmailVerified = 1 THEN 1 END) as VerifiedUsers
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

---

## Common Patterns

### Pattern 1: Get Single Item

```csharp
// Using query repository
var user = await _userQueryRepository.GetUserByIdAsync(userId, cancellationToken);

if (user is null)
    return NotFound();

return Ok(user);
```

### Pattern 2: Get Paginated List

```csharp
// Using query repository
var (users, totalCount) = await _userQueryRepository.GetPagedUsersAsync(
    pageNumber: 1,
    pageSize: 10,
    searchTerm: "john",
    isActive: true,
    isEmailVerified: null,
    cancellationToken);

var result = PaginatedResult<UserDto>.Create(
    users.ToList(), 
    totalCount, 
    pageNumber: 1, 
    pageSize: 10);

return Ok(result);
```

### Pattern 3: Check Existence

```csharp
// Using query repository
var exists = await _userQueryRepository.UserExistsByEmailAsync(
    email, 
    cancellationToken);

if (exists)
    return BadRequest("Email already registered");

// Proceed with registration...
```

### Pattern 4: Custom Query with Parameters

```csharp
// Using connection provider directly
const string sql = @"
    SELECT 
        Id, FirstName, LastName, Email, CreatedAtUtc
    FROM Users
    WHERE CreatedAtUtc >= @FromDate 
      AND CreatedAtUtc <= @ToDate
    ORDER BY CreatedAtUtc DESC";

using var connection = await _connectionProvider.CreateConnectionAsync(cancellationToken);
var users = await connection.QueryAsync<UserDto>(
    sql, 
    new { 
        FromDate = startDate, 
        ToDate = endDate 
    });

return Ok(users);
```

---

## Do's and Don'ts

### ✅ DO's

```csharp
// ✅ Use parameterized queries
const string sql = "SELECT * FROM Users WHERE Email = @Email";
await connection.QueryAsync<UserDto>(sql, new { Email = userEmail });

// ✅ Always check for null results
var user = await _userQueryRepository.GetUserByIdAsync(userId, cancellationToken);
if (user is null) return NotFound();

// ✅ Use pagination for large result sets
var (users, totalCount) = await _userQueryRepository.GetPagedUsersAsync(
    pageNumber, pageSize, cancellationToken: cancellationToken);

// ✅ Handle exceptions gracefully
try
{
    return Ok(await _userQueryRepository.GetUserByIdAsync(...));
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error fetching user");
    return StatusCode(500, "Internal server error");
}
```

### ❌ DON'Ts

```csharp
// ❌ DON'T use string concatenation (SQL injection risk)
string sql = $"SELECT * FROM Users WHERE Email = '{userEmail}'";

// ❌ DON'T ignore null results
var user = await _userQueryRepository.GetUserByIdAsync(userId, cancellationToken);
return Ok(user);  // Crashes if null

// ❌ DON'T load all data and paginate in C#
var allUsers = await QueryAllAsync();
var paginated = allUsers.Skip(offset).Take(pageSize);  // Inefficient!

// ❌ DON'T use column wildcards in performance-critical queries
const string sql = "SELECT * FROM Users";  // Loads unnecessary columns
const string sql = "SELECT Id, Email, FirstName FROM Users";  // Better
```

---

## Troubleshooting

### "Repository not found" in DI
**Cause:** Repository not registered in ServiceCollectionExtensions  
**Fix:** Check `Users.Infrastructure/Extensions/ServiceCollectionExtensions.cs`

```csharp
// Should be registered like this:
services.AddScoped<UserQueryRepository>();
```

### "Column names don't match"
**Cause:** SQL column names don't match DTO property names  
**Fix:** Use column aliases or ensure exact name match

```csharp
// Option 1: Column alias
const string sql = "SELECT UserEmail AS Email, FirstName FROM Users";

// Option 2: Use property names exactly
const string sql = "SELECT Email, FirstName FROM Users";  // Assumes column is "Email"
```

### "Timeout expired"
**Cause:** Query taking too long, probably missing index  
**Fix:** Check execution plan, add recommended indexes

```sql
CREATE INDEX IX_Users_Email_IsDeleted ON Users(Email, IsDeleted);
```

---

## Performance Tips

### 1. Select Only Needed Columns
```csharp
// ✅ Good
const string sql = "SELECT Id, Email, FirstName FROM Users";

// ❌ Bad
const string sql = "SELECT * FROM Users";
```

### 2. Filter at Database Level
```csharp
// ✅ Good - Filter in SQL
const string sql = "SELECT * FROM Users WHERE IsActive = 1 AND CreatedAtUtc > @Date";

// ❌ Bad - Filter in C#
var users = await GetAllUsersAsync();
var filtered = users.Where(u => u.IsActive && u.CreatedAtUtc > date);
```

### 3. Use Pagination
```csharp
// ✅ Good - SQL pagination
const string sql = @"
    SELECT * FROM Users
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

// ❌ Bad - Memory pagination
var all = await GetAllAsync();
var page = all.Skip(offset).Take(pageSize).ToList();
```

---

## Migration Checklist

Converting an EF Core query handler to Dapper:

1. [ ] Identify the query handler
2. [ ] Check if a Dapper repository exists for the entity
3. [ ] If not, create one inheriting from `DapperRepository<T>`
4. [ ] Update the handler to inject the query repository
5. [ ] Replace EF Core repository calls with Dapper calls
6. [ ] Remove domain entity mapping logic
7. [ ] Test the query handler
8. [ ] Verify performance improvement
9. [ ] Commit changes

---

## Example Files

See these files for concrete examples:

- `Users/Application/Handlers/GetAllUsersQueryHandler.Dapper.cs` - Paginated queries
- `Users/Application/Handlers/GetUserByIdQueryHandler.Dapper.cs` - Single entity
- `Users/Application/Handlers/GetAllRolesQueryHandler.Dapper.cs` - Simple list queries

---

## Where to Find More Information

1. **Architecture Overview:** `DAPPER_IMPLEMENTATION_SUMMARY.md`
2. **Migration Guide:** `Users/Infrastructure/DAPPER_MIGRATION_GUIDE.md`
3. **Technical Docs:** `BuildingBlocks/Infrastructure.Shared/Dapper/README.md`
4. **Implementation Checklist:** `DAPPER_IMPLEMENTATION_CHECKLIST.md`

---

## Questions?

1. Check the migration guide for your specific use case
2. Look at example handlers
3. Review SQL queries in existing repositories
4. Ask team members who've completed migrations

---

Happy Dapper querying! 🚀