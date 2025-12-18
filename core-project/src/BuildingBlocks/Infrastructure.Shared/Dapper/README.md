# Dapper Integration - BuildingBlocks

This directory contains the shared Dapper infrastructure for all modules in the project.

## Architecture

```
IDapperRepository<T>
       ▲
       │ (interface)
       │
       └─► DapperRepository<T> (abstract base)
                   ▲
                   │ (inherits)
                   │
    ┌──────────────┼──────────────┐
    │              │              │
UserQueryRepository RoleQueryRepository PermissionQueryRepository
```

## Files

### IDapperConnectionProvider.cs
Interface for managing database connections used by Dapper.

```csharp
public interface IDapperConnectionProvider
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken);
}
```

**Usage:**
```csharp
using var connection = await _connectionProvider.CreateConnectionAsync(cancellationToken);
var result = await connection.QueryAsync<UserDto>(sql, parameters);
```

### DapperConnectionProvider.cs
Implementation of `IDapperConnectionProvider` for SQL Server connections.

**Features:**
- ✅ Separate connection pool from EF Core
- ✅ Async connection creation
- ✅ Automatic connection disposal
- ✅ Logging support

**Usage in DI:**
```csharp
services.AddScoped<IDapperConnectionProvider>(sp =>
    new DapperConnectionProvider(connectionString, sp.GetRequiredService<ILogger<DapperConnectionProvider>>()));
```

### IDapperRepository<T>.cs
Generic interface for Dapper repositories with common query operations.

**Methods:**
```csharp
// Single result projection
Task<TResult?> QuerySingleOrDefaultAsync<TResult>(
    string sql, 
    object? parameters = null, 
    CancellationToken cancellationToken = default) 
    where TResult : class;

// Multiple results projection
Task<IEnumerable<TResult>> QueryAsync<TResult>(
    string sql, 
    object? parameters = null, 
    CancellationToken cancellationToken = default) 
    where TResult : class;

// Scalar query (COUNT, SUM, MAX, etc.)
Task<object?> QueryScalarAsync(
    string sql, 
    object? parameters = null, 
    CancellationToken cancellationToken = default);

// Execute command (INSERT, UPDATE, DELETE)
Task<int> ExecuteAsync(
    string sql, 
    object? parameters = null, 
    CancellationToken cancellationToken = default);
```

### DapperRepository<T>.cs
Abstract base class providing generic implementation of `IDapperRepository<T>`.

**Features:**
- ✅ All methods pre-implemented
- ✅ Built-in logging
- ✅ Exception handling
- ✅ Async/await support

**Creating a Custom Repository:**
```csharp
public class UserQueryRepository : DapperRepository<UserDto>
{
    public UserQueryRepository(IDapperConnectionProvider connectionProvider, ILogger<UserQueryRepository> logger)
        : base(connectionProvider, logger)
    {
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Email, FirstName, LastName, IsActive, IsEmailVerified
            FROM Users
            WHERE Id = @UserId AND IsDeleted = 0";

        return await QuerySingleOrDefaultAsync<UserDto>(sql, new { UserId = userId }, cancellationToken);
    }
}
```

## Usage Pattern

### 1. Define DTOs
```csharp
public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    bool IsEmailVerified,
    List<Guid> RoleIds,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? LastLoginAtUtc);
```

### 2. Create Query Repository
```csharp
public class UserQueryRepository : DapperRepository<UserDto>
{
    public UserQueryRepository(IDapperConnectionProvider connectionProvider, ILogger<UserQueryRepository> logger)
        : base(connectionProvider, logger) { }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT Id, Email, FirstName, LastName, IsActive, IsEmailVerified FROM Users WHERE Id = @UserId";
        return await QuerySingleOrDefaultAsync<UserDto>(sql, new { UserId = userId }, cancellationToken);
    }
}
```

### 3. Register in DI
```csharp
services.AddScoped<UserQueryRepository>();
```

### 4. Use in Query Handler
```csharp
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly UserQueryRepository _userQueryRepository;

    public GetUserByIdQueryHandler(UserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userDto = await _userQueryRepository.GetUserByIdAsync(request.UserId, cancellationToken);
        
        if (userDto is null)
            return Result<UserDto>.Failure(Error.NotFound("User not found"));

        return Result<UserDto>.Success(userDto);
    }
}
```

## Best Practices

### 1. SQL Injection Prevention
Always use parameterized queries:

```csharp
// ✅ Good
const string sql = "SELECT * FROM Users WHERE Email = @Email";
await QuerySingleOrDefaultAsync<UserDto>(sql, new { Email = email }, cancellationToken);

// ❌ Bad - SQL Injection vulnerable
string sql = $"SELECT * FROM Users WHERE Email = '{email}'";
```

### 2. Null Handling
```csharp
// ✅ Good - Handle nulls properly
var user = await _userQueryRepository.GetUserByIdAsync(userId, cancellationToken);
if (user is null)
    return NotFound();

return Ok(user);
```

### 3. Pagination
```csharp
// ✅ Good - SQL pagination
const string sql = @"
    SELECT * FROM Users
    ORDER BY CreatedAtUtc DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY";

await QueryAsync<UserDto>(sql, new { Offset = offset, PageSize = pageSize }, cancellationToken);

// ❌ Avoid - Memory pagination with large datasets
var allUsers = await QueryAsync<UserDto>(sql, null, cancellationToken);
var paginated = allUsers.Skip(offset).Take(pageSize).ToList();
```

### 4. Error Handling
```csharp
try
{
    var result = await _userQueryRepository.GetUserByIdAsync(userId, cancellationToken);
    return Ok(result);
}
catch (OperationCanceledException)
{
    return StatusCode(499, "Request cancelled");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error fetching user");
    return StatusCode(500, "Internal server error");
}
```

## Performance Tips

### 1. Index Strategy
```sql
-- Frequently queried columns
CREATE INDEX IX_Users_Email_IsDeleted ON Users(Email, IsDeleted);
CREATE INDEX IX_Users_IsActive_IsDeleted ON Users(IsActive, IsDeleted);
CREATE INDEX IX_LoginAttempts_UserId_IsSuccessful ON LoginAttempts(UserId, IsSuccessful, AttemptedAtUtc);
```

### 2. Query Optimization
- Select only needed columns
- Filter in SQL, not in C#
- Use pagination
- Create appropriate indexes
- Analyze execution plans

### 3. Connection Pooling
Dapper automatically uses connection pooling. Monitor:
```csharp
// SqlConnection automatically pools connections
// Min pool size: 5 (default)
// Max pool size: 100 (default)
// Can be configured in connection string:
// "Server=...;Connection Lifetime=300;Min Pool Size=10;Max Pool Size=100"
```

## Migration Guide

See `../../../Modules/Users/Users.Infrastructure/DAPPER_MIGRATION_GUIDE.md` for step-by-step migration examples.

## Troubleshooting

### Issue: "Connection timeout"
**Cause:** Connection pool exhausted or slow queries  
**Solution:** Check connection string, optimize queries, increase pool size

### Issue: "DTO mapping fails"
**Cause:** Column names don't match property names  
**Solution:** Ensure case-insensitive matching or use column aliases:
```sql
SELECT UserEmail AS Email, FirstName, LastName FROM Users
```

### Issue: "Null reference exception"
**Cause:** Trying to use null query result  
**Solution:** Always check for null:
```csharp
var result = await QuerySingleOrDefaultAsync<UserDto>(...);
if (result is null) throw new...
```

## References

- [Dapper GitHub Repository](https://github.com/DapperLib/Dapper)
- [Dapper Documentation](https://github.com/DapperLib/Dapper#dapper-a-light-orm-for-net)
- [SQL Server Connection Pooling](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-connection-pooling)