# Hướng dẫn cấu trúc dự án - Modular Monolith Architecture

## Tổng quan

Dự án này được xây dựng dựa trên kiến trúc **Modular Monolith** với các nguyên tắc **Domain-Driven Design (DDD)** và **Clean Architecture**. Cấu trúc được thiết kế để:

- **Dễ mở rộng**: Có thể thêm modules mới một cách độc lập
- **Tách biệt rõ ràng**: Mỗi module có boundaries rõ ràng, không phụ thuộc lẫn nhau
- **Microservices-ready**: Có thể tách thành microservices khi cần thiết
- **Testability**: Kiến trúc hỗ trợ testing ở mọi tầng

## Cấu trúc thư mục

```
CoreProject/
├── src/
│   ├── BuildingBlocks/
│   │   ├── Abstractions/                    # Shared contracts và interfaces
│   │   └── Infrastructure.Shared/           # Shared infrastructure implementations
│   ├── Gateway/
│   │   └── WebApi/                         # Main API Gateway (Host application)
│   ├── Modules/                            # Business modules
│   │   ├── Identity/
│   │   │   ├── Identity.Domain/            # Domain layer
│   │   │   ├── Identity.Application/       # Application layer (CQRS, Validation)
│   │   │   └── Identity.Infrastructure/    # Infrastructure layer (DB, External services)
│   │   ├── Inventory/
│   │   │   ├── Inventory.Domain/
│   │   │   ├── Inventory.Application/
│   │   │   └── Inventory.Infrastructure/
│   │   └── Procurement/
│   │       ├── Procurement.Domain/
│   │       ├── Procurement.Application/
│   │       └── Procurement.Infrastructure/
├── tests/
│   ├── Shared.Testing/                     # Shared testing utilities
│   ├── Identity.UnitTests/
│   ├── Identity.IntegrationTests/
│   ├── Inventory.UnitTests/
│   └── Inventory.IntegrationTests/
├── Directory.Packages.props                # Centralized package management
├── .editorconfig                          # Code style configuration
└── CoreProject.sln                        # Solution file
```

## Kiến trúc từng tầng

### 1. BuildingBlocks
Chứa các **shared contracts** và **infrastructure** được sử dụng chung:

#### Abstractions
- `IClock`: Interface để làm việc với thời gian (testable)
- `IDomainEvent`: Interface cho domain events
- `IEventBus`: Interface cho event bus
- `IOutbox`: Interface cho transactional outbox pattern
- `Result<T>`: Wrapper cho kết quả có thể thành công hoặc thất bại
- `Entity<TId>`: Base class cho domain entities

#### Infrastructure.Shared
- `SystemClock`: Implementation của IClock
- Shared implementations cho logging, caching, messaging, etc.

### 2. Gateway/WebApi
- **Host application** chính của ứng dụng
- Load và khởi tạo tất cả modules
- Expose API endpoints từ các modules
- Chứa middleware, authentication, authorization
- OpenTelemetry, Serilog configuration

### 3. Modules
Mỗi module đại diện cho một **bounded context** trong business:

#### Domain Layer
- **Entities & Aggregates**: Core business objects
- **Value Objects**: Immutable objects representing concepts
- **Domain Events**: Events phát sinh từ business logic
- **Repository Interfaces**: Contracts để truy cập data
- **Domain Services**: Business logic không thuộc về entity nào

#### Application Layer
- **Commands/Queries (CQRS)**: Use cases của ứng dụng
- **Handlers**: Xử lý commands/queries
- **DTOs**: Data transfer objects
- **Validators (FluentValidation)**: Validation rules
- **Pipeline Behaviors**: Cross-cutting concerns

#### Infrastructure Layer
- **Repository Implementations**: EF Core repositories
- **DbContext**: Database context cho module
- **External Service Clients**: Gọi APIs bên ngoài
- **Message Handlers**: Xử lý events từ modules khác

## Dependency Rules

```
┌─────────────────── Gateway/WebApi ───────────────────┐
│  Presentation (Controllers, Endpoints)               │
└─────────────────────▲────────────────────────────────┘
                      │ depends on
┌─────────────────────┴── Application ─────────────────┐
│  Commands/Queries, Handlers, Validators              │
└─────────────────────▲────────────────────────────────┘
                      │ uses abstractions
┌─────────────────────┴──── Domain ────────────────────┐
│  Entities, Value Objects, Domain Events              │
└───────────────────────────────────────────────────────┘
                      │ implemented by
┌─────────────────────▼── Infrastructure ──────────────┐
│  EF Core, External APIs, Message Bus                 │
└───────────────────────────────────────────────────────┘
```

**Quy tắc quan trọng:**
- **Domain** không phụ thuộc vào bất kỳ layer nào khác
- **Application** chỉ phụ thuộc Domain và BuildingBlocks.Abstractions
- **Infrastructure** implement interfaces từ Domain và Application
- **Gateway** wire tất cả lại với nhau
- **Modules không được tham chiếu trực tiếp lẫn nhau** - chỉ giao tiếp qua events

## Package Management

Sử dụng **Central Package Management** với `Directory.Packages.props`:

### Core Packages
- **.NET 9.0**: Latest framework version
- **Entity Framework Core**: ORM with PostgreSQL provider
- **MediatR**: CQRS implementation
- **FluentValidation**: Validation framework

### Observability
- **OpenTelemetry**: Distributed tracing
- **Serilog**: Structured logging

### Testing
- **xUnit**: Testing framework
- **FluentAssertions**: Assertion library
- **Bogus**: Test data generation
- **Testcontainers**: Integration testing with real databases

## Module Communication

### Within Module
- **Direct method calls** trong cùng module
- **Domain events** trong aggregate boundaries

### Between Modules
- **Application Events** thông qua IEventBus
- **Transactional Outbox** đảm bảo consistency
- **Message Bus** cho async communication

## Database Schema

Mỗi module có **schema riêng**:
- `identity.*` cho Identity module
- `inventory.*` cho Inventory module
- `procurement.*` cho Procurement module

## Ví dụ Implementation

### 1. Domain Entity (Inventory.Domain)
```csharp
public sealed class Item : Entity<ItemId>
{
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public int Quantity { get; private set; }

    public static Result<Item> Create(string sku, string name, int quantity, IClock clock)
    {
        // Business validation
        var item = new Item(ItemId.New(), sku, name, quantity);
        item.Raise(new ItemCreated(item.Id, clock.UtcNow));
        return Result<Item>.Success(item);
    }
}
```

### 2. Application Handler (Inventory.Application)
```csharp
public static class CreateItem
{
    public record Command(string Sku, string Name, int Quantity) : IRequest<Result<Guid>>;

    internal sealed class Handler(IInventoryRepository repo, IClock clock) 
        : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken ct)
        {
            var item = Item.Create(request.Sku, request.Name, request.Quantity, clock);
            await repo.AddAsync(item.Value!);
            await repo.UnitOfWork.SaveChangesAsync(ct);
            return Result<Guid>.Success(item.Value!.Id);
        }
    }
}
```

### 3. Infrastructure Repository (Inventory.Infrastructure)
```csharp
public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;
    
    public IUnitOfWork UnitOfWork => _context;
    
    public async Task<Item?> GetByIdAsync(ItemId id, CancellationToken ct = default)
    {
        return await _context.Items.FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
```

## Development Workflow

### 1. Thêm Module mới
```bash
# Tạo projects cho module mới
dotnet new classlib -n Orders.Domain -o src/Modules/Orders/Orders.Domain
dotnet new classlib -n Orders.Application -o src/Modules/Orders/Orders.Application  
dotnet new classlib -n Orders.Infrastructure -o src/Modules/Orders/Orders.Infrastructure

# Add vào solution
dotnet sln add src/Modules/Orders/Orders.Domain/Orders.Domain.csproj

# Setup references
dotnet add src/Modules/Orders/Orders.Application/Orders.Application.csproj reference src/Modules/Orders/Orders.Domain/Orders.Domain.csproj
```

### 2. Tạo Migration cho module
```bash
# Trong Infrastructure project
dotnet ef migrations add InitialCreate --context OrdersDbContext --output-dir Migrations
```

### 3. Chạy tests
```bash
# Unit tests
dotnet test tests/Orders.UnitTests/

# Integration tests  
dotnet test tests/Orders.IntegrationTests/
```

## Best Practices

### 1. Domain Layer
- Entities chứa business logic core
- Use strong-typed IDs (ItemId, OrderId)
- Domain events cho business events quan trọng
- Không tham chiếu infrastructure

### 2. Application Layer  
- CQRS pattern với Commands/Queries riêng biệt
- Validation với FluentValidation
- Use Result<T> thay vì exceptions
- Pipeline behaviors cho cross-cutting concerns

### 3. Infrastructure Layer
- Repository pattern với EF Core
- Schema riêng cho mỗi module
- Configuration trong OnModelCreating
- Optimistic concurrency với RowVersion

### 4. Testing
- Unit tests cho Domain và Application logic
- Integration tests với real database (Testcontainers)
- WebApplicationFactory cho API testing
- Bogus cho test data generation

## Roadmap to Microservices

Khi cần tách module thành microservice:

1. **Message Bus**: Thay in-process events bằng RabbitMQ/Kafka
2. **Outbox Pattern**: Đảm bảo consistency across services
3. **API Gateway**: YARP routing đến services
4. **Service Discovery**: Consul/Eureka
5. **Configuration**: External config với Consul/Vault

## Tools & IDE Setup

### Visual Studio Code Extensions
- C# Dev Kit
- .NET Install Tool
- GitLens
- REST Client

### Development Environment
```bash
# Start development dependencies
docker-compose -f docker/docker-compose.dev.yml up -d

# Run migrations
dotnet ef database update --context InventoryDbContext

# Start application
dotnet run --project src/Gateway/WebApi
```

## Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Modular Monolith](https://www.kamilgrzybek.com/blog/posts/modular-monolith-primer)
- [.NET Application Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/)

---

**Lưu ý**: Đây là kiến trúc foundation có thể mở rộng theo nhu cầu dự án. Hãy điều chỉnh cho phù hợp với context cụ thể của team và business requirements.