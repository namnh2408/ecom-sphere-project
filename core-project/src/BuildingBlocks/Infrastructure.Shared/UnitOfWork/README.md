# Unit of Work Pattern - Shared Implementation

## 📋 Overview

Một implementation **generic Unit of Work** cho tất cả modules, giúp quản lý database transactions và change tracking.

- **Generic implementation**: `EFCoreUnitOfWork<TDbContext>`
- **Interface chung**: `IUnitOfWork` (từ BuildingBlocks.Abstractions)
- **Dễ register**: Extension method `AddUnitOfWork<TDbContext>()`

---

## 🚀 Quick Start - Cách dùng trong Module

### 1. Thêm vào Infrastructure Service Extension

```csharp
using BuildingBlocks.Infrastructure.Shared.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace YourModule.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddYourModuleInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // 1. Add DbContext
        services.AddDbContext<YourDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        // 2. Add repositories
        services.AddScoped<IYourRepository, YourRepository>();

        // 3. Add generic Unit of Work - ONE LINE ONLY! 🎉
        services.AddUnitOfWork<YourDbContext>();

        return services;
    }
}
```

### 2. Inject vào Command Handlers

```csharp
using BuildingBlocks.Abstractions;
using MediatR;

namespace YourModule.Application.Handlers;

public class CreateSomethingCommandHandler : IRequestHandler<CreateSomethingCommand, Result>
{
    private readonly IYourRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSomethingCommandHandler(
        IYourRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateSomethingCommand command, CancellationToken cancellationToken)
    {
        // Business logic...
        var entity = new YourEntity(...);

        // Add to repository
        await _repository.AddAsync(entity, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

---

## 📦 Implementation Details

### EFCoreUnitOfWork Generic Class

```csharp
public class EFCoreUnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EFCoreUnitOfWork(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Implement IUnitOfWork interface
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### Extension Method

```csharp
public static IServiceCollection AddUnitOfWork<TDbContext>(this IServiceCollection services)
    where TDbContext : DbContext
{
    services.AddScoped<IUnitOfWork>(sp =>
        new EFCoreUnitOfWork<TDbContext>(sp.GetRequiredService<TDbContext>()));

    return services;
}
```

---

## ✨ Key Benefits

| Aspect | Lợi ích |
|--------|---------|
| **Reusable** | 1 implementation cho tất cả modules |
| **Simple** | 1 dòng code cho mỗi module |
| **Consistent** | Cùng pattern trong toàn project |
| **Type-safe** | Generic với compile-time checking |
| **Testable** | Mock `IUnitOfWork` trong tests |
| **Extensible** | Có thể add thêm features sau |

---

## 🔄 Common Usage Patterns

### Pattern 1: Basic Save

```csharp
var result = await _repository.AddAsync(entity, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

### Pattern 2: Transaction with Rollback

```csharp
try
{
    await _unitOfWork.BeginTransactionAsync(cancellationToken);

    // Multiple operations
    await _repository.AddAsync(entity1, cancellationToken);
    await _repository.AddAsync(entity2, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    await _unitOfWork.CommitTransactionAsync(cancellationToken);
}
catch
{
    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
    throw;
}
```

### Pattern 3: Update

```csharp
var entity = await _repository.GetByIdAsync(id, cancellationToken);
if (entity == null)
    return Result.Fail("Entity not found");

// Modify entity
entity.UpdateProperty(newValue);

// Update via repository (marks as modified)
_repository.Update(entity);

// Save
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

---

## ❓ FAQs

### Q: Tại sao dùng generic Unit of Work?
**A**: Tránh duplicate code. Mỗi module có DbContext khác nhau, nhưng logic Unit of Work giống nhau.

### Q: Có thể dùng cho multiple DbContexts?
**A**: Không trong 1 UnitOfWork object. Mỗi module register riêng với `AddUnitOfWork<TheirDbContext>()`.
Nếu cần cross-module transaction, đó là design issue - modules không nên tightly coupled.

### Q: Performance impact?
**A**: Minimal. Generic là compile-time construct, không runtime overhead.

### Q: Có thể extend EFCoreUnitOfWork?
**A**: Có! Tạo subclass nếu module cần custom behavior:
```csharp
public class YourModuleUnitOfWork : EFCoreUnitOfWork<YourDbContext>
{
    public YourModuleUnitOfWork(YourDbContext context) : base(context) { }

    // Custom methods here
}

// Register
services.AddScoped<IUnitOfWork, YourModuleUnitOfWork>();
```

---

## 📚 Related Files

- `EFCoreUnitOfWork.cs` - Generic implementation
- `ServiceCollectionExtensions.cs` - DI registration
- `BuildingBlocks.Abstractions/IUnitOfWork.cs` - Interface definition

## 🔗 Used by

- ✅ **Catalog Module** - `Catalog.Infrastructure.Extensions`
- ✅ **Users Module** - `Users.Infrastructure.Extensions`
- ✅ Future modules...

---

**Status**: ✅ Production Ready
**Version**: 1.0.0