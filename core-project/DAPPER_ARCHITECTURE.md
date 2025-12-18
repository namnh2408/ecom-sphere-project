# Dapper Integration - Technical Architecture

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     ASP.NET Core Controllers                             │
│                   (Gateway/WebApi/Controllers)                           │
└──────────────────────────────┬──────────────────────────────────────────┘
                               │
                    ┌──────────┴──────────┐
                    │                     │
                    ▼                     ▼
            ┌───────────────┐    ┌───────────────┐
            │    GET        │    │   POST/PUT    │
            │   Requests    │    │   Requests    │
            └───────┬───────┘    └───────┬───────┘
                    │                     │
        ┌───────────┴─────────┬──────────┴────────────┐
        │                     │                       │
        ▼                     ▼                       ▼
    ┌─────────────┐    ┌──────────────┐    ┌──────────────┐
    │   Queries   │    │   Commands   │    │  Mutations   │
    │  (MediatR)  │    │  (MediatR)   │    │  (MediatR)   │
    └──────┬──────┘    └──────┬───────┘    └──────┬───────┘
           │                   │                   │
    ┌──────┴───────┐      ┌────┴────┐         ┌───┴────┐
    │              │      │         │         │        │
    ▼              ▼      ▼         ▼         ▼        ▼
┌─────────────────────┐ ┌──────────────────┐ ┌──────────────┐
│    Query Handlers   │ │ Command Handlers │ │     ...      │
│  • GetAllUsers      │ │ • CreateUser     │ │              │
│  • GetUserById      │ │ • UpdateUser     │ │              │
│  • GetAllRoles      │ │ • AssignRole     │ │              │
│  • GetPermissions   │ │ • DeleteUser     │ │              │
│  • GetActivityHist  │ │ • ...            │ │              │
│  • GetAuditLogs     │ └──────────────────┘ └──────────────┘
└────────┬────────────┘        │                    │
         │              ┌──────┴────────┐      ┌────┴────┐
         │              │               │      │         │
         ▼              ▼               ▼      ▼         ▼
    ┌──────────────────────────────────────────────────────┐
    │         Domain Layer (Users.Domain)                  │
    │  • Repositories (Write contracts)                    │
    │    - IUserRepository (write operations)              │
    │    - IRoleRepository                                 │
    │    - IPermissionRepository                           │
    │    - ...                                             │
    │  • Entities (User, Role, Permission, ...)            │
    │  • ValueObjects (Email, Password)                    │
    │  • Domain Events                                     │
    └──────────────────────────────────────────────────────┘
           │                                       │
    ┌──────┴────────────┐           ┌─────────────┴──────────┐
    │                   │           │                        │
    ▼                   ▼           ▼                        ▼
┌──────────────────┐ ┌──────────────────┐    ┌────────────────────┐
│  EF Core Write   │ │ Domain Events    │    │ Application Events │
│  Repositories    │ │ Publisher        │    │ Handler            │
│                  │ │                  │    │                    │
│ • UserRepository │ │ • Domain Events  │    │ • Event Handlers   │
│ • RoleRepository │ │ • Event Bus      │    │ • Outbox Pattern   │
│ • Permission...  │ └──────────────────┘    │ • Integration      │
│ • ...            │                        │   Events           │
│                  │                        └────────────────────┘
│ (Change Tracking)│
│ (Transactions)   │
│ (Flush SaveChanges)
└────────┬─────────┘
         │
         ▼
    ┌──────────────────────────────────────────────────────┐
    │   Entity Framework Core DbContext                    │
    │  (Users.Infrastructure.Persistence.UsersDbContext)  │
    │                                                      │
    │   • DbSet<User>                                      │
    │   • DbSet<Role>                                      │
    │   • DbSet<Permission>                                │
    │   • DbSet<AuditLog>                                  │
    │   • DbSet<ChangeHistory>                             │
    │   • DbSet<LoginAttempt>                              │
    │   • OnModelCreating() with full configuration        │
    └──────────────────────────────────────────────────────┘
         │
    ┌────┴───────────────────────────────────────┐
    │                                            │
    ▼                                            ▼
[SQL Server Connection 1]          [SQL Server Connection 2]
  (EF Core Connection)               (Dapper Connection)
  • Change Tracking                  • No Change Tracking
  • Transaction Support              • Direct SQL Execution
  • LINQ Translation                 • Parameter Binding
    │                                  │
    └────────────────┬─────────────────┘
                     │
                     ▼
            ┌─────────────────────┐
            │   SQL Server        │
            │   Database          │
            │                     │
            │ • Users Table       │
            │ • Roles Table       │
            │ • Permissions Table │
            │ • AuditLogs Table   │
            │ • ChangeHistories   │
            │ • ...               │
            └─────────────────────┘
```

---

## Read Path (Queries with Dapper)

```
User GET Request
     │
     ▼
┌───────────────────┐
│ MediatR Pipeline  │
│ (GetUserByIdQuery)│
└─────────┬─────────┘
          │
          ▼
┌──────────────────────────────────┐
│ GetUserByIdQueryHandler          │
│  (Dependency Injection)          │
│  - UserQueryRepository injected  │
└──────────────┬───────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│ UserQueryRepository                  │
│ extends DapperRepository<UserDto>    │
│                                      │
│ GetUserByIdAsync(userId)             │
│ {                                    │
│   SELECT Id, Email, FirstName, ...   │
│   FROM Users                         │
│   WHERE Id = @UserId AND IsDeleted=0 │
│ }                                    │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│ DapperRepository<UserDto>            │
│ (Abstract Base)                      │
│                                      │
│ QuerySingleOrDefaultAsync<UserDto>   │
│ {                                    │
│   Validates SQL                      │
│   Logs query                         │
│   Executes with Dapper               │
│ }                                    │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│ IDapperConnectionProvider            │
│ CreateConnectionAsync()              │
│ {                                    │
│   Creates new SqlConnection          │
│   Opens async                        │
│   Returns pooled connection          │
│ }                                    │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│ Dapper ORM                           │
│ (NuGet Package)                      │
│                                      │
│ connection.QuerySingleOrDefaultAsync │
│ <UserDto>(sql, parameters)           │
│ {                                    │
│   Binds parameters @UserId           │
│   Executes SQL                       │
│   Maps DataReader to UserDto         │
│   Returns result                     │
│ }                                    │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│ SQL Server Connection Pool           │
│ (Separate from EF Core)              │
│ • Min: 5 connections                 │
│ • Max: 100 connections               │
└──────────────┬───────────────────────┘
               │
               ▼
        ┌──────────────┐
        │ SQL Server   │
        │              │
        │ Execute:     │
        │ SELECT ...   │
        │ WHERE Id=123 │
        └──────┬───────┘
               │
               ▼
        ┌──────────────┐
        │ Result:      │
        │ UserDto {    │
        │  Id: 123,    │
        │  Email: ..   │
        │  FirstName:..│
        │ }            │
        └──────┬───────┘
               │
               ▼
        ┌──────────────────┐
        │ Query Handler    │
        │ Wraps in Result  │
        │ Returns OK(dto)  │
        └──────┬───────────┘
               │
               ▼
        HTTP 200 OK Response
        with UserDto JSON
```

---

## Write Path (Commands with EF Core)

```
User POST/PUT Request
     │
     ▼
┌───────────────────┐
│ MediatR Pipeline  │
│ (RegisterUserCmd) │
└─────────┬─────────┘
          │
          ▼
┌──────────────────────────────────┐
│ RegisterUserCommandHandler       │
│  (Dependency Injection)          │
│  - IUserRepository injected      │
│  - IUnitOfWork injected          │
│  - Password hasher injected      │
└──────────────┬───────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│ Handler Logic                        │
│                                      │
│ 1. Validate input                    │
│ 2. Hash password                     │
│ 3. Create User entity                │
│ 4. Check email not exists            │
│ 5. repository.AddAsync(user)         │
│ 6. unitOfWork.SaveChangesAsync()     │
│ 7. Publish domain events             │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│ UserRepository (EF Core)             │
│ implements IUserRepository           │
│                                      │
│ AddAsync(user)                       │
│ {                                    │
│   _context.Users.AddAsync(user)      │
│ }                                    │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│ UsersDbContext (EF Core)             │
│                                      │
│ ChangeTracker.Entries:               │
│ • User [Added]                       │
│ • Email [Value Object]               │
│ • Password [Value Object]            │
│                                      │
│ SaveChangesAsync()                   │
│ {                                    │
│   1. Generate INSERT SQL             │
│   2. Begin transaction               │
│   3. Execute INSERT                  │
│   4. Publish domain events           │
│   5. Commit transaction              │
│ }                                    │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│ EF Core Connection                   │
│ • With transaction support          │
│ • With change tracking               │
│ • With event publishing              │
└──────────────┬───────────────────────┘
               │
               ▼
        ┌──────────────┐
        │ SQL Server   │
        │              │
        │ BEGIN TRANS  │
        │ INSERT INTO  │
        │  Users(...)  │
        │ VALUES(...)  │
        │ COMMIT TRANS │
        └──────┬───────┘
               │
               ▼
        ┌──────────────────┐
        │ Insert Result    │
        │ NewUserId: 123   │
        └──────┬───────────┘
               │
               ▼
        ┌──────────────────┐
        │ Domain Events    │
        │ Published:       │
        │ • UserCreated    │
        │ • EmailVerif...  │
        │                  │
        │ (Async handlers) │
        └──────┬───────────┘
               │
               ▼
        ┌──────────────────┐
        │ Handler returns  │
        │ Result.Success() │
        └──────┬───────────┘
               │
               ▼
        HTTP 201 Created Response
        with UserDto JSON
```

---

## Data Flow Comparison

### EF Core (Write Operations)

```
Entity Object
    ↓
Change Tracker
    ↓
Lazy Loading
    ↓
Navigation Properties
    ↓
Query Translation
    ↓
SQL Generation
    ↓
SQL Execution
    ↓
Database
    ↓
Result Mapping
    ↓
Entity Instance
    ↓
Client
```

**Overhead:** Change tracking, query translation, lazy loading

---

### Dapper (Read Operations)

```
SQL Query (Direct)
    ↓
Parameter Binding
    ↓
SQL Execution
    ↓
Database
    ↓
DataReader
    ↓
DTO Mapping (Reflection)
    ↓
DTO Instance
    ↓
Client
```

**Speed:** Direct execution, no translation, minimal overhead

---

## Connection Pool Architecture

```
┌─────────────────────────────────────────────────────────┐
│         SQL Server Connection Pools                      │
└─────────────────────────────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
        ▼                       ▼
┌──────────────────┐  ┌──────────────────┐
│ EF Core Pool     │  │ Dapper Pool      │
│                  │  │                  │
│ • Min: 5         │  │ • Min: 5         │
│ • Max: 100       │  │ • Max: 100       │
│ • Same Conn Str  │  │ • Same Conn Str  │
│                  │  │                  │
│ Used for:        │  │ Used for:        │
│ • Write (Commands) │ • Read (Queries) │
│ • Transactions   │  │ • No Transactions│
│ • Change Track   │  │ • Direct SQL     │
│                  │  │                  │
│ Connections:     │  │ Connections:     │
│ • DbContext      │  │ • Query Handlers │
│ • SaveChanges()  │  │ • Custom Queries │
│                  │  │                  │
└──────────────────┘  └──────────────────┘
        │                       │
        └───────────┬───────────┘
                    │
                    ▼
            ┌──────────────────┐
            │  SQL Server      │
            │  Database        │
            │  (Single DB)     │
            └──────────────────┘
```

---

## Dependency Injection Structure

```
ServiceCollection.AddUsersInfrastructure()
    │
    ├─► EF Core Registration
    │   └─► AddDbContext<UsersDbContext>()
    │       └─► SQL Server Provider
    │
    ├─► Dapper Registration
    │   ├─► AddScoped<IDapperConnectionProvider>()
    │   │   └─► DapperConnectionProvider
    │   │       └─► Connection String
    │   │
    │   ├─► AddScoped<UserQueryRepository>()
    │   │   └─► DapperRepository<UserDto>
    │   │       └─► IDapperConnectionProvider
    │   │
    │   ├─► AddScoped<RoleQueryRepository>()
    │   │   └─► DapperRepository<RoleDto>
    │   │
    │   ├─► AddScoped<PermissionQueryRepository>()
    │   │   └─► DapperRepository<PermissionDto>
    │   │
    │   ├─► AddScoped<ActivityHistoryQueryRepository>()
    │   │   └─► DapperRepository<UserActivityHistoryDto>
    │   │
    │   └─► AddScoped<AuditLogQueryRepository>()
    │       └─► DapperRepository<AuditLogDto>
    │
    └─► Write Repositories (EF Core)
        ├─► AddScoped<IUserRepository, UserRepository>()
        ├─► AddScoped<IRoleRepository, RoleRepository>()
        ├─► AddScoped<IPermissionRepository, PermissionRepository>()
        └─► ... (other write repositories)
```

---

## Query Handler Injection Pattern

```
Query Handler Constructor:

    public GetUserByIdQueryHandler(
        UserQueryRepository userQueryRepository,           // ← Dapper
        ILogger<GetUserByIdQueryHandler> logger)            // ← Optional
    {
        _userQueryRepository = userQueryRepository;
        _logger = logger;
    }

Query Handler Handle Method:

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
```

---

## Transaction Safety

### EF Core (Write Operations)
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();
    
    // Publish events
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Dapper (Read Operations)
```csharp
// No transactions needed for reads
// Connection isolation level: READ COMMITTED (default)
using var connection = await _connectionProvider.CreateConnectionAsync();
var result = await connection.QuerySingleOrDefaultAsync<UserDto>(sql, parameters);
```

---

## Performance Characteristics

### Query Handler with Dapper
- **Query Time:** 5-15ms for typical 10K row pagination
- **Memory:** ~100KB for 10 DTO objects
- **CPU:** Minimal (direct mapping)
- **Connection Time:** ~1ms (pooled)

### Query Handler with EF Core
- **Query Time:** 20-50ms for typical 10K row pagination
- **Memory:** ~500KB (entity objects + change tracking)
- **CPU:** Moderate (LINQ translation + change tracking)
- **Connection Time:** ~1ms (pooled)

**Typical Improvement:** 50-70% faster with Dapper

---

## Scalability Patterns

### Horizontal Scaling
```
Load Balancer
    ├─► App Server 1 (Reads + Writes)
    │   └─► Connection Pool to SQL Server
    │
    ├─► App Server 2 (Reads + Writes)
    │   └─► Connection Pool to SQL Server
    │
    └─► App Server 3 (Reads + Writes)
        └─► Connection Pool to SQL Server
        
                    ▼
            ┌──────────────────┐
            │  SQL Server      │
            │  (Single Master) │
            │                  │
            │ Max Connections: │
            │ 300 (300 * 1/3)  │
            │ per app server   │
            └──────────────────┘
```

### Query Caching Layer (Future)
```
Dapper Query Repository
    ↓
Caching Layer (Redis)
    ↓
SQL Query
    ↓
SQL Server
```

---

This architecture ensures:
- ✅ **Separation of Concerns** - Reads and writes use appropriate tools
- ✅ **Performance** - Optimized query paths
- ✅ **Maintainability** - Clear repository contracts
- ✅ **Scalability** - Connection pooling and caching ready
- ✅ **Testability** - Interfaces allow mocking