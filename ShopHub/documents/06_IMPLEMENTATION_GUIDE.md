# 📖 Hướng Dẫn Triển Khai

Hướng dẫn chi tiết về cách triển khai các service theo kiến trúc đã định nghĩa.

---

## 🚀 Bắt Đầu Với Service Mới

### Bước 1: Tạo Cấu Trúc Thư Mục

```
src/Services/[ServiceName]/
├── Application/
│   ├── DTOs/
│   ├── Services/
│   ├── UseCases/
│   ├── Validators/
│   └── Mappings/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   ├── Services/
│   └── Repositories/
├── Infrastructure/
│   ├── Persistence/
│   │   ├── Repositories/
│   │   └── Migrations/
│   ├── ExternalServices/
│   └── DependencyInjection.cs
├── Presentation/
│   ├── Controllers/
│   ├── Filters/
│   └── Middleware/
└── Program.cs
```

### Bước 2: Định Nghĩa Domain Model

**File**: `Domain/Entities/[Entity].cs`

```csharp
using ShopHub.Domain.Base;

namespace ShopHub.Services.[Service].Domain.Entities;

public class Order : BaseAggregateRoot
{
    // Properties
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    // Private constructor
    private Order() { }

    // Factory method
    public static Order Create(
        string orderNumber,
        Guid customerId,
        decimal totalAmount)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be empty");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerId = customerId,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Add domain event
        order.AddDomainEvent(new OrderCreatedEvent(
            order.Id,
            order.OrderNumber,
            order.CustomerId));

        return order;
    }

    // Domain methods
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Can only confirm pending orders");

        Status = OrderStatus.Confirmed;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Can only ship confirmed orders");

        Status = OrderStatus.Shipped;
    }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

// Domain Event
public class OrderCreatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public int Version => 1;

    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid CustomerId { get; }

    public OrderCreatedEvent(Guid orderId, string orderNumber, Guid customerId)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        OrderId = orderId;
        OrderNumber = orderNumber;
        CustomerId = customerId;
    }
}
```

### Bước 3: Tạo Repository Interface

**File**: `Domain/Repositories/IOrderRepository.cs`

```csharp
using ShopHub.Domain.Abstractions;
using ShopHub.Services.[Service].Domain.Entities;

namespace ShopHub.Services.[Service].Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
}
```

### Bước 4: Tạo DTOs

**File**: `Application/DTOs/CreateOrderDto.cs`

```csharp
namespace ShopHub.Services.[Service].Application.DTOs;

public class CreateOrderDto
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class OrderResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### Bước 5: Tạo Use Cases

**File**: `Application/UseCases/CreateOrderUseCase.cs`

```csharp
using ShopHub.Common.Results;
using ShopHub.Services.[Service].Application.DTOs;
using ShopHub.Services.[Service].Domain.Entities;
using ShopHub.Services.[Service].Domain.Repositories;

namespace ShopHub.Services.[Service].Application.UseCases;

public class CreateOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CreateOrderUseCase> _logger;

    public CreateOrderUseCase(
        IOrderRepository orderRepository,
        ILogger<CreateOrderUseCase> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<OrderResponseDto>> ExecuteAsync(
        CreateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";

            // Calculate total
            var totalAmount = dto.Items.Sum(i => i.Price * i.Quantity);

            // Create domain entity
            var order = Order.Create(
                orderNumber,
                dto.CustomerId,
                totalAmount);

            // Save
            await _orderRepository.AddAsync(order, cancellationToken);

            // Map to DTO
            var response = new OrderResponseDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt
            };

            _logger.LogInformation($"Order {orderNumber} created");

            return Result<OrderResponseDto>.Success(response, "Order created", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating order: {ex.Message}");
            return Result<OrderResponseDto>.Failure("Error creating order", 500);
        }
    }
}
```

### Bước 6: Tạo DbContext

**File**: `Infrastructure/Persistence/OrderDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using ShopHub.Services.[Service].Domain.Entities;

namespace ShopHub.Services.[Service].Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(o => o.Status)
                .HasConversion<string>();

            builder.HasIndex(o => o.OrderNumber)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(o => o.CustomerId);
        });
    }
}
```

### Bước 7: Tạo Repository Implementation

**File**: `Infrastructure/Persistence/Repositories/OrderRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using ShopHub.Services.[Service].Domain.Entities;
using ShopHub.Services.[Service].Domain.Repositories;

namespace ShopHub.Services.[Service].Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(OrderDbContext dbContext, ILogger<OrderRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Id == id && !o.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Orders.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Order entity, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        _dbContext.Orders.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePermanentlyAsync(Order entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Orders.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(o => o.Id == id && !o.IsDeleted)
            .AnyAsync(cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.OrderNumber == orderNumber && !o.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(o => o.OrderNumber == orderNumber && !o.IsDeleted)
            .AnyAsync(cancellationToken);
    }
}
```

### Bước 8: Tạo Controllers

**File**: `Presentation/Controllers/OrdersController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using ShopHub.Services.[Service].Application.DTOs;
using ShopHub.Services.[Service].Application.UseCases;

namespace ShopHub.Services.[Service].Presentation.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderUseCase _createOrderUseCase;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        CreateOrderUseCase createOrderUseCase,
        ILogger<OrdersController> logger)
    {
        _createOrderUseCase = createOrderUseCase;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _createOrderUseCase.ExecuteAsync(dto, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetOrder), new { id = result.Data?.OrderId }, result.Data);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrder([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }
}
```

### Bước 9: Dependency Injection

**File**: `Infrastructure/DependencyInjection.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopHub.Services.[Service].Application.UseCases;
using ShopHub.Services.[Service].Domain.Repositories;
using ShopHub.Services.[Service].Infrastructure.Persistence;
using ShopHub.Services.[Service].Infrastructure.Persistence.Repositories;

namespace ShopHub.Services.[Service].Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add[Service](
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("[Service]Db")
            ?? throw new InvalidOperationException("[Service]Db connection string not found");

        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Use Cases
        services.AddScoped<CreateOrderUseCase>();

        return services;
    }
}
```

### Bước 10: Program.cs

**File**: `Program.cs`

```csharp
using ShopHub.Services.[Service].Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add service
builder.Services.Add[Service](builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Bước 11: Unit Tests

**File**: `tests/Unit/Services.[Service].Tests/Domain/OrderTests.cs`

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShopHub.Services.[Service].Domain.Entities;

namespace ShopHub.Services.[Service].Tests.Domain;

[TestClass]
public class OrderTests
{
    [TestMethod]
    public void CreateOrder_WithValidData_ShouldSucceed()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var totalAmount = 100m;

        // Act
        var order = Order.Create("ORD-001", customerId, totalAmount);

        // Assert
        Assert.IsNotNull(order);
        Assert.AreEqual("ORD-001", order.OrderNumber);
        Assert.AreEqual(customerId, order.CustomerId);
        Assert.AreEqual(totalAmount, order.TotalAmount);
        Assert.AreEqual(OrderStatus.Pending, order.Status);
    }

    [TestMethod]
    public void ConfirmOrder_WithPendingOrder_ShouldSucceed()
    {
        // Arrange
        var order = Order.Create("ORD-001", Guid.NewGuid(), 100m);

        // Act
        order.Confirm();

        // Assert
        Assert.AreEqual(OrderStatus.Confirmed, order.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ConfirmOrder_WithConfirmedOrder_ShouldThrow()
    {
        // Arrange
        var order = Order.Create("ORD-001", Guid.NewGuid(), 100m);
        order.Confirm();

        // Act
        order.Confirm();
    }
}
```

---

## 📋 Checklist

- [ ] Domain entities (DDD principles)
- [ ] Domain events
- [ ] Repository interfaces
- [ ] DTOs (Input + Output)
- [ ] Use Cases
- [ ] Validators (Fluent Validation)
- [ ] DbContext
- [ ] Repository implementations
- [ ] Controllers
- [ ] Dependency injection
- [ ] Unit tests
- [ ] Integration tests
- [ ] API documentation (Swagger)
- [ ] Error handling
- [ ] Logging

---

## 🔗 Links
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Kiến trúc tổng thể
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)