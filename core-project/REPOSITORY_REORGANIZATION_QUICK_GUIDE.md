# Repository Reorganization - Quick Reference Guide

## Directory Structure at a Glance

### Infrastructure Layer (Write Operations)
```
Users.Infrastructure/Repositories/Commands/
├── UserRepository.cs              (IUserRepository)
├── RoleRepository.cs              (IRoleRepository)
├── PermissionRepository.cs        (IPermissionRepository)
├── EmailVerificationTokenRepository.cs
├── PasswordResetTokenRepository.cs
├── LoginAttemptRepository.cs
├── RefreshTokenRepository.cs
├── UserActivityHistoryRepository.cs
├── AuditLogRepository.cs
├── ChangeHistoryRepository.cs
└── AccessLogRepository.cs
```

### Infrastructure Layer (Read Operations)
```
Users.Infrastructure/Repositories/Queries/
├── UserQueryRepository.cs         (Dapper-based, optimized reads)
├── RoleQueryRepository.cs
├── PermissionQueryRepository.cs
├── ActivityHistoryQueryRepository.cs
└── AuditLogQueryRepository.cs
```

### Application Layer (Write Handlers)
```
Users.Application/Handlers/CommandHandlers/
├── CreateRoleCommandHandler.cs
├── UpdateRoleCommandHandler.cs
├── DeleteRoleCommandHandler.cs
├── RegisterUserCommandHandler.cs
├── LoginUserCommandHandler.cs
├── ChangePasswordCommandHandler.cs
└── ... (23 total command handlers)
```

### Application Layer (Read Handlers)
```
Users.Application/Handlers/QueryHandlers/
├── GetAllUsersQueryHandler.cs
├── GetUserByIdQueryHandler.cs
├── GetAllRolesQueryHandler.cs
├── GetLoginHistoryQueryHandler.cs
├── GetUserActivityHistoryQueryHandler.cs
└── ... (9 total query handlers)
```

## Using Command Repositories

### Example: Creating a User

```csharp
// Handler location: CommandHandlers/RegisterUserCommandHandler.cs
// Namespace: Users.Application.Handlers.CommandHandlers

using Users.Infrastructure.Repositories.Commands;
using Users.Domain.Repositories;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    
    public RegisterUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<Result<UserDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var user = User.Create(command.Email, command.FirstName, command.LastName);
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<UserDto>.Success(MapToDto(user));
    }
}
```

## Using Query Repositories

### Example: Getting Users by ID

```csharp
// Handler location: QueryHandlers/GetUserByIdQueryHandler.cs
// Namespace: Users.Application.Handlers.QueryHandlers

using Users.Infrastructure.Repositories.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly UserQueryRepository _userQueryRepository;
    
    public GetUserByIdQueryHandler(UserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }
    
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userQueryRepository.GetUserByIdAsync(request.UserId, cancellationToken);
        
        if (user is null)
            return Result<UserDto>.Fail("USER_NOT_FOUND", "User not found");
            
        return Result<UserDto>.Success(user);
    }
}
```

## Namespace Mapping

| Purpose | Old Namespace | New Namespace |
|---------|---|---|
| Write Repositories | `Users.Infrastructure.Repositories` | `Users.Infrastructure.Repositories.Commands` |
| Read Repositories | `Users.Infrastructure.Repositories` | `Users.Infrastructure.Repositories.Queries` |
| Command Handlers | `Users.Application.Handlers` | `Users.Application.Handlers.CommandHandlers` |
| Query Handlers | `Users.Application.Handlers` | `Users.Application.Handlers.QueryHandlers` |

## How to Inject

### Command Repository (EF Core)
```csharp
// Register in ServiceCollectionExtensions.cs
services.AddScoped<IUserRepository, UserRepository>();

// Use in handler
public MyCommandHandler(IUserRepository userRepository)
{
    _userRepository = userRepository;
}
```

### Query Repository (Dapper)
```csharp
// Register in ServiceCollectionExtensions.cs
services.AddScoped<UserQueryRepository>();

// Use in handler
public MyQueryHandler(UserQueryRepository userQueryRepository)
{
    _userQueryRepository = userQueryRepository;
}
```

## What Goes Where?

### ✅ Use Command Repositories for:
- Creating new entities
- Updating existing entities
- Deleting entities
- Complex business logic requiring transactions
- Maintaining data consistency

**Example**: `CreateRoleCommandHandler` → `RoleRepository`

### ✅ Use Query Repositories for:
- Retrieving user data
- Filtering and searching
- Getting historical/audit logs
- Performance-critical queries
- Pagination scenarios

**Example**: `GetUserByIdQueryHandler` → `UserQueryRepository`

## Common Patterns

### Create Command
```
Command: CreateUserCommand
↓
Handler: CreateUserCommandHandler (in CommandHandlers/)
↓
Repository: IUserRepository (in Repositories/Commands/)
```

### Query Handler
```
Query: GetUserByIdQuery
↓
Handler: GetUserByIdQueryHandler (in QueryHandlers/)
↓
Repository: UserQueryRepository (in Repositories/Queries/)
```

## Key Files to Remember

| File | Purpose |
|------|---------|
| `ServiceCollectionExtensions.cs` (Infrastructure) | Repository registration |
| `ServiceCollectionExtensions.cs` (Application) | Handler registration (MediatR) |
| `Repositories/Commands/` | EF Core write repositories |
| `Repositories/Queries/` | Dapper read repositories |
| `Handlers/CommandHandlers/` | Command request handlers |
| `Handlers/QueryHandlers/` | Query request handlers |

## Adding a New Handler

### Step 1: Create Command/Query
```csharp
// Location: Users.Application/Queries/GetUsersByRoleQuery.cs
public record GetUsersByRoleQuery(Guid RoleId, int PageNumber, int PageSize) 
    : IRequest<Result<PaginatedResult<UserDto>>>;
```

### Step 2: Create Handler
```csharp
// Location: Users.Application/Handlers/QueryHandlers/GetUsersByRoleQueryHandler.cs
namespace Users.Application.Handlers.QueryHandlers;

public class GetUsersByRoleQueryHandler : IRequestHandler<GetUsersByRoleQuery, Result<PaginatedResult<UserDto>>>
{
    private readonly UserQueryRepository _userQueryRepository;
    
    public GetUsersByRoleQueryHandler(UserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }
    
    public async Task<Result<PaginatedResult<UserDto>>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await _userQueryRepository.GetUsersByRoleIdAsync(
            request.RoleId, 
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);
            
        var result = new PaginatedResult<UserDto>(users, totalCount, request.PageNumber, request.PageSize);
        return Result<PaginatedResult<UserDto>>.Success(result);
    }
}
```

### Step 3: Handler is Auto-Registered
MediatR automatically discovers handlers in subdirectories. No registration needed!

## Performance Optimization

### For Queries:
- Dapper repositories (`Queries/`) provide direct SQL mapping
- Use for high-volume read operations
- Include pagination to limit result sets
- Consider caching frequently accessed data

### For Commands:
- EF Core repositories (`Commands/`) handle complex updates
- Use transactions for multi-entity operations
- Leverage unit of work pattern for consistency
- Emit domain events for side effects

## Common Mistakes to Avoid

❌ **DON'T** mix read and write logic in one handler  
✅ **DO** use separate query and command handlers

❌ **DON'T** use query repositories for writes  
✅ **DO** use command repositories with EF Core for writes

❌ **DON'T** forget to register repositories in DI  
✅ **DO** register in `ServiceCollectionExtensions.cs`

❌ **DON'T** put business logic in repositories  
✅ **DO** put it in handlers or domain services

## Troubleshooting

### Handler Not Found
```
MediatR can't find my handler!
```
Solution: Ensure handler is in correct folder and namespace is correct

### Repository Not Injected
```
Cannot resolve UserQueryRepository
```
Solution: Check that repository is registered in `ServiceCollectionExtensions.cs`

### Namespace Resolution Error
```
The type 'UserRepository' could not be found
```
Solution: Add correct using statement:
```csharp
using Users.Infrastructure.Repositories.Commands;
```

---

**Last Updated**: October 31, 2025  
**For Questions**: Refer to REPOSITORY_REORGANIZATION_SUMMARY.md