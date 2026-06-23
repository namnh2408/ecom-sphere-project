# 🛒 E-Commerce Microservice — Lộ trình học & kiến trúc đầy đủ

> **Mục tiêu:** Xây dựng hệ thống E-Commerce theo kiến trúc Microservice bằng .NET, tích hợp đầy đủ Kafka, Elasticsearch, Redis, SignalR, Hangfire, Docker, Kubernetes và các best practice production-ready.
>
> **Đối tượng:** .NET Backend Developer muốn nâng cao kinh nghiệm thực chiến.
>
> **Thời gian ước tính:** 14–18 tuần (tùy tốc độ học)

---

## Mục lục

1. [Tổng quan kiến trúc](#1-tổng-quan-kiến-trúc)
2. [Tech stack đầy đủ](#2-tech-stack-đầy-đủ)
3. [Cấu trúc solution](#3-cấu-trúc-solution)
4. [Giai đoạn 1 — Nền móng & Clean Architecture](#giai-đoạn-1--nền-móng--clean-architecture-tuần-12)
5. [Giai đoạn 2 — API Gateway & Authentication](#giai-đoạn-2--api-gateway--authentication-tuần-34)
6. [Giai đoạn 3 — Kafka & Event-Driven](#giai-đoạn-3--kafka--event-driven-tuần-56)
7. [Giai đoạn 4 — Redis Cache & Distributed Lock](#giai-đoạn-4--redis-cache--distributed-lock-tuần-7)
8. [Giai đoạn 5 — Elasticsearch](#giai-đoạn-5--elasticsearch-tuần-8)
9. [Giai đoạn 6 — SignalR Real-time](#giai-đoạn-6--signalr-real-time-tuần-9)
10. [Giai đoạn 7 — Hangfire Background Jobs](#giai-đoạn-7--hangfire-background-jobs-tuần-10)
11. [Giai đoạn 8 — Observability & Health Check](#giai-đoạn-8--observability--health-check-tuần-11)
12. [Giai đoạn 9 — Docker & Docker Compose](#giai-đoạn-9--docker--docker-compose-tuần-12)
13. [Giai đoạn 10 — Kubernetes](#giai-đoạn-10--kubernetes-tuần-1314)
14. [Giai đoạn 11 — Testing](#giai-đoạn-11--testing-tuần-1516)
15. [Giai đoạn 12 — CI/CD & Hoàn thiện](#giai-đoạn-12--cicd--hoàn-thiện-tuần-1718)
16. [Business domain & nghiệp vụ](#16-business-domain--nghiệp-vụ)
17. [Checklist hoàn thành](#17-checklist-hoàn-thành)

---

## 1. Tổng quan kiến trúc

```
┌──────────────────────────────────────────────────────────┐
│                      CLIENT LAYER                         │
│              Web App · Mobile · Admin Dashboard           │
└─────────────────────────┬────────────────────────────────┘
                          │ HTTPS
┌─────────────────────────▼────────────────────────────────┐
│               API GATEWAY (YARP)                          │
│    Auth · Rate Limit · Routing · Load Balance · SSL       │
└──┬───────┬────────┬──────────┬──────────┬────────────────┘
   │       │        │          │          │
   ▼       ▼        ▼          ▼          ▼
[Identity][Order][Product] [Payment] [Notification]
 Service  Service  Service   Service    Service
   │       │        │          │          │
   └───────┴────────┴──────────┴──────────┘
                       │
            ┌──────────▼──────────┐
            │   Apache Kafka      │
            │   (MassTransit)     │
            └──┬──────┬──────┬───┘
               │      │      │
            [Redis] [ES]  [SignalR]
                           │
                        [Client]  ← real-time push
```

### Đặc điểm nhận diện Microservice

| Nguyên tắc | Cách áp dụng trong project |
|---|---|
| **Database per service** | Mỗi service có PostgreSQL/MongoDB riêng, không share |
| **Loose coupling** | Service không gọi thẳng nhau, chỉ qua Kafka event |
| **High cohesion** | Mỗi service chỉ làm 1 nghiệp vụ cụ thể |
| **Independent deploy** | Mỗi service có Dockerfile và CI/CD riêng |
| **Failure isolation** | 1 service down không kéo sập service khác |
| **Scale độc lập** | Scale Order Service x5 mà không đụng Payment Service |

---

## 2. Tech stack đầy đủ

### Core

| Layer | Technology | Version | Mục đích |
|---|---|---|---|
| Runtime | .NET | 8.0 LTS | Nền tảng chính |
| Web framework | ASP.NET Core | 8.0 | REST API |
| ORM | Entity Framework Core | 8.x | Database access (write) |
| Micro ORM | Dapper | 2.x | Optimized read queries |
| CQRS | MediatR | 12.x | Command/Query separation |
| Validation | FluentValidation | 11.x | Request validation |
| Mapping | Mapster | 7.x | Object mapping |

### Infrastructure

| Technology | Version | Mục đích |
|---|---|---|
| Apache Kafka | 3.7 | Event bus, async messaging |
| MassTransit | 8.x | Kafka abstraction, Saga, Outbox |
| Redis | 7.x | Cache, Session, Distributed lock |
| Elasticsearch | 8.x | Full-text search, Analytics |
| SignalR | ASP.NET Core built-in | Real-time WebSocket |
| Hangfire | 1.8.x | Background jobs, Scheduling |
| PostgreSQL | 16 | Primary database |
| MongoDB | 7.x | Notification history |

### API & Security

| Technology | Mục đích |
|---|---|
| YARP | API Gateway, Reverse proxy |
| Keycloak | Identity Provider, OAuth2/OIDC |
| JWT Bearer | Token authentication |
| Polly | Retry, Circuit Breaker, Timeout |
| gRPC | Internal service-to-service calls |

### Observability

| Technology | Mục đích |
|---|---|
| OpenTelemetry | Distributed tracing chuẩn |
| Jaeger | Trace visualization UI |
| Serilog | Structured logging |
| Seq | Log aggregation UI (local dev) |
| Prometheus | Metrics collection |
| Grafana | Metrics dashboard |
| .NET Health Checks | Liveness/Readiness probe |

### DevOps

| Technology | Mục đích |
|---|---|
| Docker | Containerize tất cả services |
| Docker Compose | Local development environment |
| Kubernetes | Production orchestration |
| Helm | K8s package manager |
| GitHub Actions | CI/CD pipeline |
| Testcontainers | Integration testing với real containers |

---

## 3. Cấu trúc solution

```
ECommerceMS/
├── src/
│   ├── ApiGateway/
│   │   └── ECommerceMS.ApiGateway/          # YARP Gateway
│   │
│   ├── Services/
│   │   ├── Identity/
│   │   │   └── ECommerceMS.Identity/        # Keycloak config + Auth helpers
│   │   │
│   │   ├── Order/
│   │   │   ├── ECommerceMS.Order.Domain/    # Entities, Value Objects, Events
│   │   │   ├── ECommerceMS.Order.Application/  # Commands, Queries, Handlers
│   │   │   ├── ECommerceMS.Order.Infrastructure/ # EF Core, Kafka, Redis
│   │   │   └── ECommerceMS.Order.API/       # Controllers, Endpoints
│   │   │
│   │   ├── Product/
│   │   │   ├── ECommerceMS.Product.Domain/
│   │   │   ├── ECommerceMS.Product.Application/
│   │   │   ├── ECommerceMS.Product.Infrastructure/
│   │   │   └── ECommerceMS.Product.API/
│   │   │
│   │   ├── Payment/
│   │   │   ├── ECommerceMS.Payment.Domain/
│   │   │   ├── ECommerceMS.Payment.Application/
│   │   │   ├── ECommerceMS.Payment.Infrastructure/
│   │   │   └── ECommerceMS.Payment.API/
│   │   │
│   │   └── Notification/
│   │       ├── ECommerceMS.Notification.Domain/
│   │       ├── ECommerceMS.Notification.Application/
│   │       ├── ECommerceMS.Notification.Infrastructure/
│   │       └── ECommerceMS.Notification.API/
│   │
│   └── Shared/
│       ├── ECommerceMS.Shared.Contracts/    # Kafka events, DTOs dùng chung
│       ├── ECommerceMS.Shared.Infrastructure/ # Redis, Outbox, HealthCheck helpers
│       └── ECommerceMS.Shared.Domain/      # Base entities, Value objects
│
├── tests/
│   ├── Unit/
│   │   ├── ECommerceMS.Order.UnitTests/
│   │   └── ECommerceMS.Product.UnitTests/
│   └── Integration/
│       ├── ECommerceMS.Order.IntegrationTests/    # Testcontainers
│       └── ECommerceMS.Payment.IntegrationTests/
│
├── infra/
│   ├── docker/
│   │   └── docker-compose.yml
│   ├── k8s/
│   │   ├── base/
│   │   └── overlays/
│   └── helm/
│       └── ecommerce-chart/
│
├── docs/
│   ├── architecture/
│   ├── api/
│   └── runbooks/
│
└── .github/
    └── workflows/
        ├── order-service.yml
        ├── product-service.yml
        └── payment-service.yml
```

### Clean Architecture per service

```
ECommerceMS.Order/
├── Domain/
│   ├── Entities/           Order.cs, OrderItem.cs
│   ├── ValueObjects/       Money.cs, Address.cs
│   ├── Enums/              OrderStatus.cs
│   ├── Events/             OrderCreatedEvent.cs
│   ├── Exceptions/         OrderNotFoundException.cs
│   └── Interfaces/         IOrderRepository.cs
│
├── Application/
│   ├── Commands/
│   │   ├── CreateOrder/    CreateOrderCommand.cs
│   │   │                   CreateOrderCommandHandler.cs
│   │   │                   CreateOrderCommandValidator.cs
│   │   └── CancelOrder/    CancelOrderCommand.cs ...
│   ├── Queries/
│   │   └── GetOrderById/   GetOrderByIdQuery.cs ...
│   ├── DTOs/               OrderDto.cs, OrderItemDto.cs
│   ├── Behaviors/          ValidationBehavior.cs, LoggingBehavior.cs
│   └── Interfaces/         IOrderService.cs
│
├── Infrastructure/
│   ├── Persistence/
│   │   ├── OrderDbContext.cs
│   │   ├── Repositories/   OrderRepository.cs
│   │   ├── Configurations/ OrderConfiguration.cs
│   │   └── Migrations/
│   ├── Messaging/
│   │   ├── Consumers/      PaymentCompletedConsumer.cs
│   │   └── Publishers/     OrderEventPublisher.cs
│   ├── Caching/            OrderCacheService.cs
│   └── Outbox/             OutboxProcessor.cs
│
└── API/
    ├── Controllers/        OrdersController.cs
    ├── Endpoints/          (Minimal API nếu dùng)
    ├── Middlewares/        ExceptionMiddleware.cs
    ├── Extensions/         ServiceCollectionExtensions.cs
    └── Program.cs
```

---

## Giai đoạn 1 — Nền móng & Clean Architecture (Tuần 1–2)

### Mục tiêu
Dựng được skeleton project, hiểu Clean Architecture trong .NET, implement Order Service đầy đủ.

### Việc cần làm

**1.1. Khởi tạo solution**
```bash
dotnet new sln -n ECommerceMS
dotnet new webapi -n ECommerceMS.Order.API
dotnet new classlib -n ECommerceMS.Order.Domain
dotnet new classlib -n ECommerceMS.Order.Application
dotnet new classlib -n ECommerceMS.Order.Infrastructure
dotnet sln add **/*.csproj
```

**1.2. Packages cần cài**
```xml
<!-- Order.Domain — không dependency nào -->

<!-- Order.Application -->
<PackageReference Include="MediatR" Version="12.*" />
<PackageReference Include="FluentValidation" Version="11.*" />
<PackageReference Include="Mapster" Version="7.*" />

<!-- Order.Infrastructure -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.*" />
<PackageReference Include="Dapper" Version="2.*" />

<!-- Order.API -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.*" />
<PackageReference Include="Scalar.AspNetCore" Version="1.*" />
```

**1.3. Implement Order aggregate**
```csharp
// Domain/Entities/Order.cs
public sealed class Order : AggregateRoot
{
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    private readonly List<OrderItem> _items = [];

    private Order() { } // EF Core

    public static Order Create(CustomerId customerId, List<OrderItem> items)
    {
        var order = new Order
        {
            Id = OrderId.New(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            _items = items,
            TotalAmount = Money.Sum(items.Select(i => i.TotalPrice))
        };
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new OrderInvalidStatusException(Id, Status, OrderStatus.Confirmed);
        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id));
    }
}
```

**1.4. CQRS với MediatR**
```csharp
// Application/Commands/CreateOrder/CreateOrderCommand.cs
public record CreateOrderCommand(
    Guid CustomerId,
    List<CreateOrderItemDto> Items
) : ICommand<OrderDto>;

// Application/Commands/CreateOrder/CreateOrderCommandHandler.cs
public sealed class CreateOrderCommandHandler(
    IOrderRepository repository,
    IUnitOfWork unitOfWork,
    IPublisher publisher
) : ICommandHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var order = Order.Create(
            new CustomerId(command.CustomerId),
            command.Items.Select(i => OrderItem.Create(i.ProductId, i.Quantity, i.UnitPrice)).ToList()
        );

        await repository.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Dispatch domain events → Kafka via MassTransit
        foreach (var evt in order.DomainEvents)
            await publisher.Publish(evt, ct);

        return order.ToDto();
    }
}
```

**1.5. MediatR Pipeline Behaviors**
```csharp
// Logging → Validation → Transaction → Handler
public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(e => e != null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

### Checklist giai đoạn 1
- [ ] Solution structure đúng Clean Architecture
- [ ] Order aggregate với domain events
- [ ] CQRS: CreateOrder, CancelOrder, GetOrderById, GetOrders
- [ ] FluentValidation cho tất cả Commands
- [ ] EF Core + PostgreSQL migration chạy được
- [ ] Repository pattern + Unit of Work
- [ ] API endpoint hoạt động, Scalar UI hiển thị

---

## Giai đoạn 2 — API Gateway & Authentication (Tuần 3–4)

### Mục tiêu
Implement YARP API Gateway, tích hợp Keycloak làm Identity Provider, bảo vệ tất cả endpoints.

### Việc cần làm

**2.1. YARP API Gateway**
```bash
dotnet new webapi -n ECommerceMS.ApiGateway
cd ECommerceMS.ApiGateway
dotnet add package Yarp.ReverseProxy
```

```json
// appsettings.json — YARP config
{
  "ReverseProxy": {
    "Routes": {
      "order-route": {
        "ClusterId": "order-cluster",
        "Match": { "Path": "/api/orders/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/orders/{**catch-all}" }]
      },
      "product-route": {
        "ClusterId": "product-cluster",
        "Match": { "Path": "/api/products/{**catch-all}" }
      }
    },
    "Clusters": {
      "order-cluster": {
        "HealthCheck": {
          "Active": { "Enabled": true, "Path": "/health" }
        },
        "Destinations": {
          "order-service-1": { "Address": "http://order-service:5001" }
        }
      }
    }
  }
}
```

**2.2. Rate Limiting trong Gateway**
```csharp
// Program.cs — API Gateway
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("api", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.SegmentsPerWindow = 6;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 10;
    });
});
```

**2.3. Keycloak setup (Docker)**
```yaml
# docker-compose.yml
keycloak:
  image: quay.io/keycloak/keycloak:25.0
  command: start-dev
  environment:
    KC_DB: postgres
    KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
    KEYCLOAK_ADMIN: admin
    KEYCLOAK_ADMIN_PASSWORD: admin
  ports:
    - "8080:8080"
```

**2.4. JWT validation trong các service**
```csharp
// Shared/Infrastructure/Extensions/AuthExtensions.cs
public static IServiceCollection AddJwtAuthentication(
    this IServiceCollection services,
    IConfiguration config)
{
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = config["Keycloak:Authority"];
            options.Audience = config["Keycloak:Audience"];
            options.RequireHttpsMetadata = false; // dev only
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

    return services;
}
```

**2.5. Polly Resilience**
```csharp
// Retry + Circuit Breaker cho downstream calls
builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>()
    .AddResilienceHandler("order-pipeline", pipeline =>
    {
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(500),
            BackoffType = DelayBackoffType.Exponential
        });
        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(10)
        });
        pipeline.AddTimeout(TimeSpan.FromSeconds(5));
    });
```

### Checklist giai đoạn 2
- [ ] YARP route đến tất cả services
- [ ] Rate limiting hoạt động (test bằng cách spam request)
- [ ] Keycloak chạy trong Docker, tạo realm `ecommerce`
- [ ] Client `ecommerce-api` trong Keycloak
- [ ] JWT validation trong Order Service
- [ ] Protected endpoints trả 401 khi thiếu token
- [ ] Polly retry/circuit breaker cho HTTP calls

---

## Giai đoạn 3 — Kafka & Event-Driven (Tuần 5–6)

### Mục tiêu
Implement event-driven communication giữa services, đảm bảo at-least-once delivery với Outbox Pattern.

### Kafka topics

| Topic | Publisher | Consumers | Payload |
|---|---|---|---|
| `order.created` | Order Service | Product, Payment, Notification | OrderCreatedEvent |
| `order.confirmed` | Order Service | Notification, SignalR | OrderConfirmedEvent |
| `order.cancelled` | Order Service | Product, Payment, Notification | OrderCancelledEvent |
| `payment.completed` | Payment Service | Order, Notification | PaymentCompletedEvent |
| `payment.failed` | Payment Service | Order, Notification | PaymentFailedEvent |
| `stock.reserved` | Product Service | Order | StockReservedEvent |
| `stock.insufficient` | Product Service | Order, Notification | StockInsufficientEvent |

### Việc cần làm

**3.1. MassTransit + Kafka setup**
```bash
dotnet add package MassTransit.Kafka
dotnet add package MassTransit.EntityFrameworkCore  # Outbox
```

```csharp
// Infrastructure/Extensions/MessagingExtensions.cs
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingInMemory(); // fallback

    x.AddRider(rider =>
    {
        rider.AddConsumer<PaymentCompletedConsumer>();
        rider.AddConsumer<StockReservedConsumer>();

        rider.AddProducer<OrderCreatedEvent>("order.created");
        rider.AddProducer<OrderConfirmedEvent>("order.confirmed");

        rider.UsingKafka((ctx, k) =>
        {
            k.Host("kafka:9092");

            k.TopicEndpoint<PaymentCompletedEvent>("payment.completed", "order-service-group", e =>
            {
                e.ConfigureConsumer<PaymentCompletedConsumer>(ctx);
                e.CreateIfMissing(t => { t.NumPartitions = 3; t.ReplicationFactor = 1; });
            });
        });
    });
});
```

**3.2. Outbox Pattern — đảm bảo không mất event**
```csharp
// Infrastructure/Persistence/OrderDbContext.cs
public class OrderDbContext(DbContextOptions<OrderDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OutboxState> OutboxStates => Set<OutboxState>();
    public DbSet<InboxState> InboxStates => Set<InboxState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
```

**3.3. Consumer với Idempotency**
```csharp
// Infrastructure/Messaging/Consumers/PaymentCompletedConsumer.cs
public sealed class PaymentCompletedConsumer(
    IOrderRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<PaymentCompletedConsumer> logger
) : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing PaymentCompleted for Order {OrderId}", message.OrderId);

        var order = await repository.GetByIdAsync(new OrderId(message.OrderId), context.CancellationToken);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found — skipping", message.OrderId);
            return;
        }

        if (order.Status == OrderStatus.Confirmed)
        {
            logger.LogInformation("Order {OrderId} already confirmed — idempotent skip", message.OrderId);
            return;
        }

        order.Confirm();
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
```

**3.4. Saga Pattern cho Distributed Transaction**
```csharp
// Application/Sagas/OrderProcessingSaga.cs
public class OrderProcessingSaga : MassTransitStateMachine<OrderProcessingSagaState>
{
    public State Submitted { get; private set; } = null!;
    public State StockReserved { get; private set; } = null!;
    public State PaymentProcessing { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    public Event<OrderCreatedEvent> OrderCreated { get; private set; } = null!;
    public Event<StockReservedEvent> StockReserved_ { get; private set; } = null!;
    public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; } = null!;
    public Event<PaymentFailedEvent> PaymentFailed { get; private set; } = null!;

    public OrderProcessingSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreated, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReserved_, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentCompleted, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentFailed, x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderCreated)
                .TransitionTo(Submitted)
                .Publish(ctx => new ReserveStockCommand(ctx.Message.OrderId, ctx.Message.Items))
        );

        During(Submitted,
            When(StockReserved_)
                .TransitionTo(PaymentProcessing)
                .Publish(ctx => new ProcessPaymentCommand(ctx.Message.OrderId))
        );

        During(PaymentProcessing,
            When(PaymentCompleted)
                .TransitionTo(Completed)
                .Finalize(),
            When(PaymentFailed)
                .TransitionTo(Cancelled)
                .Publish(ctx => new ReleaseStockCommand(ctx.Message.OrderId))
                .Finalize()
        );
    }
}
```

### Checklist giai đoạn 3
- [ ] Kafka chạy trong Docker (Bitnami Kafka 3.7)
- [ ] MassTransit publish/consume hoạt động
- [ ] Outbox table trong OrderDbContext
- [ ] Không mất event khi service restart
- [ ] Idempotency: xử lý duplicate message
- [ ] Saga quản lý flow: Order → Stock → Payment
- [ ] Dead Letter Queue cho failed messages

---

## Giai đoạn 4 — Redis Cache & Distributed Lock (Tuần 7)

### Mục tiêu
Cache product data, giỏ hàng, session. Distributed lock khi cập nhật stock.

### Use cases

| Use case | Cache key | TTL | Invalidation |
|---|---|---|---|
| Product detail | `product:{id}` | 10 phút | Khi product update |
| Product list | `products:page:{n}:size:{s}` | 5 phút | Khi có product mới |
| Shopping cart | `cart:{userId}` | 24 giờ | Khi checkout |
| User session | `session:{token}` | 1 giờ | Khi logout |
| Stock count | `stock:{productId}` | 1 phút | Khi stock change |

### Việc cần làm

**4.1. Setup Redis**
```bash
dotnet add package StackExchange.Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

```csharp
// Shared/Infrastructure/Caching/CacheService.cs
public sealed class CacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(10));
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
        => await _db.KeyDeleteAsync(key);

    public async Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
    {
        var server = redis.GetServer(redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"*{pattern}*").ToArray();
        if (keys.Length > 0) await _db.KeyDeleteAsync(keys);
    }
}
```

**4.2. Cache-Aside trong Query Handler**
```csharp
public sealed class GetProductByIdQueryHandler(
    IProductRepository repository,
    ICacheService cache
) : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var cacheKey = CacheKeys.Product(query.ProductId);

        var cached = await cache.GetAsync<ProductDto>(cacheKey, ct);
        if (cached is not null) return cached;

        var product = await repository.GetByIdAsync(query.ProductId, ct)
            ?? throw new ProductNotFoundException(query.ProductId);

        var dto = product.ToDto();
        await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), ct);

        return dto;
    }
}
```

**4.3. Distributed Lock cho stock reservation**
```csharp
// Infrastructure/Locking/RedisDistributedLock.cs
public sealed class RedisDistributedLock(IConnectionMultiplexer redis) : IDistributedLock
{
    public async Task<ILockHandle?> AcquireAsync(string resource, TimeSpan expiry, CancellationToken ct = default)
    {
        var db = redis.GetDatabase();
        var lockKey = $"lock:{resource}";
        var lockValue = Guid.NewGuid().ToString();

        var acquired = await db.StringSetAsync(lockKey, lockValue, expiry, When.NotExists);
        if (!acquired) return null;

        return new RedisLockHandle(db, lockKey, lockValue);
    }
}

// Usage trong StockService
public async Task ReserveStockAsync(Guid productId, int quantity)
{
    await using var lockHandle = await _lock.AcquireAsync($"stock:{productId}", TimeSpan.FromSeconds(10))
        ?? throw new StockLockUnavailableException(productId);

    var stock = await _repository.GetStockAsync(productId);
    if (stock.Available < quantity)
        throw new InsufficientStockException(productId, quantity, stock.Available);

    stock.Reserve(quantity);
    await _repository.UpdateAsync(stock);
}
```

**4.4. Shopping Cart với Redis**
```csharp
public sealed class CartService(ICacheService cache) : ICartService
{
    public async Task<Cart> GetCartAsync(Guid userId, CancellationToken ct)
    {
        return await cache.GetAsync<Cart>(CacheKeys.Cart(userId), ct) ?? new Cart(userId);
    }

    public async Task AddItemAsync(Guid userId, CartItem item, CancellationToken ct)
    {
        var cart = await GetCartAsync(userId, ct);
        cart.AddItem(item);
        await cache.SetAsync(CacheKeys.Cart(userId), cart, TimeSpan.FromHours(24), ct);
    }

    public async Task ClearAsync(Guid userId, CancellationToken ct)
        => await cache.RemoveAsync(CacheKeys.Cart(userId), ct);
}
```

### Checklist giai đoạn 4
- [ ] Redis chạy trong Docker
- [ ] Cache-aside cho Product queries
- [ ] Cart lưu/đọc/xóa từ Redis
- [ ] Distributed lock cho stock reservation
- [ ] Cache invalidation khi data thay đổi
- [ ] Redis health check endpoint

---

## Giai đoạn 5 — Elasticsearch (Tuần 8)

### Mục tiêu
Full-text search sản phẩm, filter/sort, faceted search. Index order data cho analytics.

### Việc cần làm

**5.1. Setup**
```bash
dotnet add package Elastic.Clients.Elasticsearch
```

**5.2. Product index mapping**
```csharp
// Infrastructure/Search/ProductSearchService.cs
public sealed class ProductSearchService(ElasticsearchClient client) : IProductSearchService
{
    private const string IndexName = "products";

    public async Task IndexAsync(ProductDocument doc, CancellationToken ct)
    {
        await client.IndexAsync(doc, i => i.Index(IndexName).Id(doc.Id), ct);
    }

    public async Task<SearchResult<ProductDocument>> SearchAsync(
        ProductSearchRequest request, CancellationToken ct)
    {
        var response = await client.SearchAsync<ProductDocument>(s => s
            .Index(IndexName)
            .From((request.Page - 1) * request.PageSize)
            .Size(request.PageSize)
            .Query(q => q
                .Bool(b => b
                    .Must(m =>
                    {
                        if (!string.IsNullOrEmpty(request.Keyword))
                            m.MultiMatch(mm => mm
                                .Fields(["name^3", "description", "category"])
                                .Query(request.Keyword)
                                .Fuzziness(new Fuzziness("AUTO")));
                        return m;
                    })
                    .Filter(f =>
                    {
                        if (request.MinPrice.HasValue)
                            f.Range(r => r.NumberRange(n => n.Field(p => p.Price).Gte((double)request.MinPrice)));
                        if (request.CategoryId.HasValue)
                            f.Term(t => t.Field(p => p.CategoryId).Value(request.CategoryId.Value));
                        return f;
                    })
                )
            )
            .Aggregations(a => a
                .Terms("categories", t => t.Field(p => p.Category).Size(20))
                .Range("price_ranges", r => r
                    .Field(p => p.Price)
                    .Ranges([
                        new NumberRangeExpression { To = 100000 },
                        new NumberRangeExpression { From = 100000, To = 500000 },
                        new NumberRangeExpression { From = 500000 }
                    ])
                )
            )
            .Sort(so => request.SortBy switch
            {
                "price_asc" => so.Field(p => p.Price, s => s.Order(SortOrder.Asc)),
                "price_desc" => so.Field(p => p.Price, s => s.Order(SortOrder.Desc)),
                "newest" => so.Field(p => p.CreatedAt, s => s.Order(SortOrder.Desc)),
                _ => so.Score(s => s.Order(SortOrder.Desc))
            }),
            ct
        );

        return new SearchResult<ProductDocument>
        {
            Items = response.Documents.ToList(),
            Total = response.Total,
            Aggregations = MapAggregations(response.Aggregations)
        };
    }
}
```

**5.3. Sync DB → Elasticsearch qua Kafka**
```csharp
// Notification/Infrastructure/Consumers/ProductUpdatedConsumer.cs
public sealed class ProductUpdatedConsumer(
    IProductSearchService searchService,
    ILogger<ProductUpdatedConsumer> logger
) : IConsumer<ProductUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var doc = context.Message.ToDocument();
        await searchService.IndexAsync(doc, context.CancellationToken);
        logger.LogInformation("Indexed product {ProductId} to Elasticsearch", doc.Id);
    }
}
```

**5.4. Product document**
```csharp
public sealed class ProductDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public int StockQuantity { get; set; }
    public double AverageRating { get; set; }
    public List<string> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
```

### Checklist giai đoạn 5
- [ ] Elasticsearch 8.x chạy trong Docker
- [ ] Index `products` với mapping đúng
- [ ] Full-text search hoạt động với fuzzy
- [ ] Filter theo category, price range
- [ ] Sort: relevance, price, newest
- [ ] Aggregation: category counts, price buckets
- [ ] Auto-sync khi product create/update/delete
- [ ] Elasticsearch health check

---

## Giai đoạn 6 — SignalR Real-time (Tuần 9)

### Mục tiêu
Push real-time order status updates đến client. Notification toàn hệ thống.

### Việc cần làm

**6.1. SignalR Hub**
```csharp
// Notification.API/Hubs/OrderHub.cs
[Authorize]
public sealed class OrderHub(ILogger<OrderHub> logger) : Hub<IOrderHubClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        logger.LogInformation("User {UserId} connected to OrderHub", userId);
        await base.OnConnectedAsync();
    }

    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order:{orderId}");
        logger.LogInformation("Connection {ConnId} joined order group {OrderId}",
            Context.ConnectionId, orderId);
    }

    public async Task LeaveOrderGroup(string orderId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order:{orderId}");
}

public interface IOrderHubClient
{
    Task OrderStatusUpdated(OrderStatusUpdate update);
    Task OrderNotification(string message);
}
```

**6.2. Push từ Kafka consumer**
```csharp
// Notification/Infrastructure/Consumers/OrderConfirmedConsumer.cs
public sealed class OrderConfirmedConsumer(
    IHubContext<OrderHub, IOrderHubClient> hubContext,
    IEmailService emailService,
    ILogger<OrderConfirmedConsumer> logger
) : IConsumer<OrderConfirmedEvent>
{
    public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        var evt = context.Message;

        // Push real-time to client watching this order
        await hubContext.Clients
            .Group($"order:{evt.OrderId}")
            .OrderStatusUpdated(new OrderStatusUpdate
            {
                OrderId = evt.OrderId,
                Status = "Confirmed",
                Message = "Your order has been confirmed!",
                Timestamp = DateTime.UtcNow
            });

        // Push to user's personal channel
        await hubContext.Clients
            .User(evt.CustomerId.ToString())
            .OrderNotification($"Order #{evt.OrderId} confirmed!");

        // Also send email (via Hangfire)
        await emailService.QueueOrderConfirmationEmailAsync(evt.OrderId, evt.CustomerId);

        logger.LogInformation("Pushed real-time update for Order {OrderId}", evt.OrderId);
    }
}
```

**6.3. Scale SignalR với Redis Backplane**
```csharp
// Khi scale Notification Service lên nhiều pod:
builder.Services.AddSignalR()
    .AddStackExchangeRedis("redis:6379", options =>
    {
        options.Configuration.ChannelPrefix = RedisChannel.Literal("ecommerce:signalr");
    });
```

**6.4. Client JavaScript**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/orders", {
        accessTokenFactory: () => getAuthToken()
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .build();

connection.on("OrderStatusUpdated", (update) => {
    console.log(`Order ${update.orderId} → ${update.status}`);
    updateOrderStatusUI(update);
});

await connection.start();
await connection.invoke("JoinOrderGroup", orderId);
```

### Checklist giai đoạn 6
- [ ] OrderHub với typed interface
- [ ] Kafka consumer push to SignalR
- [ ] User-specific và group-specific push
- [ ] Redis backplane cho multi-pod SignalR
- [ ] Reconnect logic trong client
- [ ] Authentication trong Hub (JWT)

---

## Giai đoạn 7 — Hangfire Background Jobs (Tuần 10)

### Mục tiêu
Background processing: gửi email, tạo báo cáo, cleanup data, retry failed operations.

### Jobs cần implement

| Job | Trigger | Mục đích |
|---|---|---|
| `SendOrderConfirmationEmail` | Event-driven | Gửi email sau khi order confirmed |
| `SendDailyOrderReport` | Hàng ngày 8:00 | Report cho admin |
| `CleanupExpiredCarts` | Mỗi giờ | Xóa cart hết hạn |
| `SyncElasticsearch` | Mỗi 30 phút | Re-sync nếu có inconsistency |
| `RetryFailedNotifications` | Mỗi 5 phút | Retry email/push bị lỗi |
| `GenerateInvoice` | Event-driven | Tạo PDF invoice |

### Việc cần làm

**7.1. Setup Hangfire**
```bash
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.PostgreSql
```

```csharp
// Program.cs
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("Hangfire")));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
    options.Queues = ["critical", "default", "low"];
});
```

**7.2. Job definitions**
```csharp
// Application/Jobs/NotificationJobs.cs
public sealed class NotificationJobs(
    IEmailService emailService,
    IOrderRepository orderRepository,
    ILogger<NotificationJobs> logger
)
{
    [Queue("critical")]
    public async Task SendOrderConfirmationEmailAsync(Guid orderId)
    {
        var order = await orderRepository.GetByIdAsync(new OrderId(orderId))
            ?? throw new OrderNotFoundException(orderId);

        await emailService.SendOrderConfirmationAsync(order);
        logger.LogInformation("Order confirmation email sent for {OrderId}", orderId);
    }

    [Queue("default")]
    [DisableConcurrentExecution(300)]
    public async Task SendDailyOrderReportAsync()
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var orders = await orderRepository.GetByDateAsync(yesterday);

        var report = new DailyOrderReport(yesterday, orders);
        await emailService.SendDailyReportAsync(report);

        logger.LogInformation("Daily report sent: {Count} orders on {Date}", orders.Count, yesterday);
    }

    [Queue("low")]
    public async Task CleanupExpiredCartsAsync()
    {
        // Scan Redis keys với prefix cart: và xóa key hết hạn manual
        logger.LogInformation("Cleanup expired carts completed");
    }
}
```

**7.3. Schedule recurring jobs**
```csharp
// Startup — đăng ký recurring jobs
RecurringJob.AddOrUpdate<NotificationJobs>(
    "daily-order-report",
    job => job.SendDailyOrderReportAsync(),
    Cron.Daily(8, 0),  // 8:00 AM UTC mỗi ngày
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
);

RecurringJob.AddOrUpdate<NotificationJobs>(
    "cleanup-expired-carts",
    job => job.CleanupExpiredCartsAsync(),
    Cron.Hourly()
);
```

**7.4. Enqueue từ event consumer**
```csharp
public sealed class OrderConfirmedConsumer(IBackgroundJobClient hangfire) : IConsumer<OrderConfirmedEvent>
{
    public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        // Fire-and-forget, retry tự động nếu lỗi
        hangfire.Enqueue<NotificationJobs>(
            queue: "critical",
            methodCall: j => j.SendOrderConfirmationEmailAsync(context.Message.OrderId)
        );
    }
}
```

### Checklist giai đoạn 7
- [ ] Hangfire Dashboard truy cập được tại `/hangfire`
- [ ] Email confirmation job chạy sau order confirmed
- [ ] Recurring: daily report, hourly cleanup
- [ ] Retry policy: 3 lần, exponential backoff
- [ ] Job queue: critical/default/low
- [ ] Hangfire dùng PostgreSQL storage (không in-memory)

---

## Giai đoạn 8 — Observability & Health Check (Tuần 11)

### Mục tiêu
Không bao giờ debug bằng `Console.WriteLine`. Phải trace được request xuyên service, xem log tập trung, alert khi có lỗi.

### Việc cần làm

**8.1. Serilog structured logging**
```csharp
// Program.cs — mọi service đều có
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Service", "OrderService")
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();
```

**8.2. OpenTelemetry distributed tracing**
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("ECommerceMS.OrderService")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
        .AddOtlpExporter(o => o.Endpoint = new Uri("http://jaeger:4317"))
    )
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter()
    );
```

**8.3. Health checks**
```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: ["db", "ready"])
    .AddRedis("redis:6379", name: "redis", tags: ["cache", "ready"])
    .AddKafka(kafkaConfig, name: "kafka", tags: ["messaging", "ready"])
    .AddElasticsearch("http://elasticsearch:9200", name: "elasticsearch", tags: ["search", "ready"])
    .AddHangfire(o => o.MinimumAvailableServers = 1, name: "hangfire", tags: ["jobs", "ready"]);

// Endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false  // Chỉ check process còn sống
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/detail", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

**8.4. Custom metrics**
```csharp
public sealed class OrderMetrics
{
    private readonly Counter<long> _ordersCreated;
    private readonly Counter<long> _ordersFailed;
    private readonly Histogram<double> _orderProcessingTime;

    public OrderMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("ECommerceMS.OrderService");
        _ordersCreated = meter.CreateCounter<long>("orders.created.total");
        _ordersFailed = meter.CreateCounter<long>("orders.failed.total");
        _orderProcessingTime = meter.CreateHistogram<double>("orders.processing.duration.ms");
    }

    public void RecordOrderCreated(string status) =>
        _ordersCreated.Add(1, new("status", status));

    public void RecordProcessingTime(double milliseconds) =>
        _orderProcessingTime.Record(milliseconds);
}
```

### Checklist giai đoạn 8
- [ ] Serilog JSON logging trong tất cả services
- [ ] Seq UI xem log tại `localhost:5341`
- [ ] OpenTelemetry trace xuyên Gateway → Service → DB
- [ ] Jaeger UI xem trace tại `localhost:16686`
- [ ] Prometheus scrape metrics tại `/metrics`
- [ ] Grafana dashboard: request/s, error rate, latency
- [ ] `/health/live` và `/health/ready` đúng spec
- [ ] Custom business metrics (order count, revenue)

---

## Giai đoạn 9 — Docker & Docker Compose (Tuần 12)

### Mục tiêu
Chạy toàn bộ hệ thống bằng 1 lệnh `docker compose up`. Production-like local environment.

### Việc cần làm

**9.1. Dockerfile chuẩn cho .NET service**
```dockerfile
# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj và restore trước để tận dụng layer cache
COPY ["src/Services/Order/ECommerceMS.Order.API/ECommerceMS.Order.API.csproj", "Services/Order/ECommerceMS.Order.API/"]
COPY ["src/Services/Order/ECommerceMS.Order.Application/ECommerceMS.Order.Application.csproj", "Services/Order/ECommerceMS.Order.Application/"]
COPY ["src/Services/Order/ECommerceMS.Order.Domain/ECommerceMS.Order.Domain.csproj", "Services/Order/ECommerceMS.Order.Domain/"]
COPY ["src/Services/Order/ECommerceMS.Order.Infrastructure/ECommerceMS.Order.Infrastructure.csproj", "Services/Order/ECommerceMS.Order.Infrastructure/"]
COPY ["src/Shared/ECommerceMS.Shared.Contracts/ECommerceMS.Shared.Contracts.csproj", "Shared/ECommerceMS.Shared.Contracts/"]

RUN dotnet restore "Services/Order/ECommerceMS.Order.API/ECommerceMS.Order.API.csproj"

COPY src/ .
RUN dotnet publish "Services/Order/ECommerceMS.Order.API/ECommerceMS.Order.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Non-root user cho security
RUN addgroup --system app && adduser --system --ingroup app app
USER app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerceMS.Order.API.dll"]
```

**9.2. Docker Compose đầy đủ**
```yaml
# infra/docker/docker-compose.yml
name: ecommerce-ms

services:
  # ── Infrastructure ──────────────────────────────────────
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres123
    ports: ["5432:5432"]
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./init-db.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      retries: 5

  redis:
    image: redis:7-alpine
    command: redis-server --requirepass redis123
    ports: ["6379:6379"]
    volumes: [redis_data:/data]
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s

  kafka:
    image: bitnami/kafka:3.7
    environment:
      KAFKA_CFG_NODE_ID: 1
      KAFKA_CFG_PROCESS_ROLES: broker,controller
      KAFKA_CFG_CONTROLLER_QUORUM_VOTERS: 1@kafka:9093
      KAFKA_CFG_LISTENERS: PLAINTEXT://:9092,CONTROLLER://:9093,EXTERNAL://:9094
      KAFKA_CFG_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092,EXTERNAL://localhost:9094
      KAFKA_CFG_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,CONTROLLER:PLAINTEXT,EXTERNAL:PLAINTEXT
      KAFKA_CFG_CONTROLLER_LISTENER_NAMES: CONTROLLER
      KAFKA_CFG_AUTO_CREATE_TOPICS_ENABLE: "true"
    ports: ["9094:9094"]
    volumes: [kafka_data:/bitnami/kafka]
    healthcheck:
      test: ["CMD-SHELL", "kafka-topics.sh --bootstrap-server localhost:9092 --list"]
      interval: 30s
      retries: 5

  elasticsearch:
    image: elasticsearch:8.14.1
    environment:
      discovery.type: single-node
      xpack.security.enabled: "false"
      ES_JAVA_OPTS: -Xms512m -Xmx512m
    ports: ["9200:9200"]
    volumes: [es_data:/usr/share/elasticsearch/data]
    healthcheck:
      test: ["CMD-SHELL", "curl -s http://localhost:9200/_health | grep -q green"]
      interval: 30s

  keycloak:
    image: quay.io/keycloak/keycloak:25.0
    command: start-dev --import-realm
    environment:
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: postgres
      KC_DB_PASSWORD: postgres123
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
    ports: ["8080:8080"]
    depends_on:
      postgres: { condition: service_healthy }
    volumes: [./keycloak/realm-export.json:/opt/keycloak/data/import/realm.json]

  seq:
    image: datalust/seq:2024
    environment: { ACCEPT_EULA: Y }
    ports: ["5341:80"]
    volumes: [seq_data:/data]

  jaeger:
    image: jaegertracing/all-in-one:1.58
    ports:
      - "16686:16686"  # UI
      - "4317:4317"    # OTLP gRPC

  prometheus:
    image: prom/prometheus:v2.53.0
    ports: ["9090:9090"]
    volumes: [./prometheus.yml:/etc/prometheus/prometheus.yml]

  grafana:
    image: grafana/grafana:11.1.0
    ports: ["3000:3000"]
    environment: { GF_SECURITY_ADMIN_PASSWORD: admin }
    volumes:
      - grafana_data:/var/lib/grafana
      - ./grafana/dashboards:/etc/grafana/provisioning/dashboards

  # ── Services ─────────────────────────────────────────────
  api-gateway:
    build:
      context: ../..
      dockerfile: src/ApiGateway/ECommerceMS.ApiGateway/Dockerfile
    ports: ["5000:8080"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      order-service: { condition: service_healthy }
      product-service: { condition: service_healthy }

  order-service:
    build:
      context: ../..
      dockerfile: src/Services/Order/ECommerceMS.Order.API/Dockerfile
    environment:
      ConnectionStrings__Default: "Host=postgres;Database=order_db;Username=postgres;Password=postgres123"
      Kafka__BootstrapServers: kafka:9092
      Redis__ConnectionString: "redis:6379,password=redis123"
      Keycloak__Authority: "http://keycloak:8080/realms/ecommerce"
      Seq__Url: http://seq:80
      Otlp__Endpoint: http://jaeger:4317
    depends_on:
      postgres: { condition: service_healthy }
      kafka: { condition: service_healthy }
      redis: { condition: service_healthy }
    healthcheck:
      test: ["CMD-SHELL", "curl -sf http://localhost:8080/health/live || exit 1"]
      interval: 15s

  product-service:
    build:
      context: ../..
      dockerfile: src/Services/Product/ECommerceMS.Product.API/Dockerfile
    environment:
      ConnectionStrings__Default: "Host=postgres;Database=product_db;Username=postgres;Password=postgres123"
      Elasticsearch__Url: http://elasticsearch:9200
      Kafka__BootstrapServers: kafka:9092
      Redis__ConnectionString: "redis:6379,password=redis123"
    depends_on:
      postgres: { condition: service_healthy }
      elasticsearch: { condition: service_healthy }
    healthcheck:
      test: ["CMD-SHELL", "curl -sf http://localhost:8080/health/live || exit 1"]
      interval: 15s

  payment-service:
    build:
      context: ../..
      dockerfile: src/Services/Payment/ECommerceMS.Payment.API/Dockerfile
    environment:
      ConnectionStrings__Default: "Host=postgres;Database=payment_db;Username=postgres;Password=postgres123"
      Kafka__BootstrapServers: kafka:9092
    depends_on:
      postgres: { condition: service_healthy }
      kafka: { condition: service_healthy }
    healthcheck:
      test: ["CMD-SHELL", "curl -sf http://localhost:8080/health/live || exit 1"]
      interval: 15s

  notification-service:
    build:
      context: ../..
      dockerfile: src/Services/Notification/ECommerceMS.Notification.API/Dockerfile
    ports: ["5004:8080"]  # Expose cho SignalR
    environment:
      ConnectionStrings__Default: "mongodb://mongo:27017/notification_db"
      Kafka__BootstrapServers: kafka:9092
      Redis__ConnectionString: "redis:6379,password=redis123"
      Hangfire__ConnectionString: "Host=postgres;Database=hangfire_db;Username=postgres;Password=postgres123"

volumes:
  postgres_data:
  redis_data:
  kafka_data:
  es_data:
  seq_data:
  grafana_data:
```

### Checklist giai đoạn 9
- [ ] `docker compose up -d` chạy toàn bộ không lỗi
- [ ] Multi-stage Dockerfile cho tất cả services
- [ ] Health check đúng với `condition: service_healthy`
- [ ] Volumes persist data khi restart
- [ ] `.dockerignore` đúng
- [ ] Không hardcode secret trong Dockerfile
- [ ] `docker compose logs -f order-service` hiện log đẹp

---

## Giai đoạn 10 — Kubernetes (Tuần 13–14)

### Mục tiêu
Deploy lên k8s local (Kind/Minikube), hiểu Deployment/Service/Ingress/ConfigMap/Secret/HPA.

### Cấu trúc k8s manifests

```
infra/k8s/
├── base/
│   ├── namespaces.yml
│   ├── order-service/
│   │   ├── deployment.yml
│   │   ├── service.yml
│   │   ├── configmap.yml
│   │   └── hpa.yml
│   ├── product-service/
│   ├── payment-service/
│   ├── notification-service/
│   └── api-gateway/
└── overlays/
    ├── dev/
    └── staging/
```

### Việc cần làm

**10.1. Deployment**
```yaml
# k8s/base/order-service/deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-service
  namespace: ecommerce
spec:
  replicas: 2
  selector:
    matchLabels:
      app: order-service
  template:
    metadata:
      labels:
        app: order-service
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/path: "/metrics"
        prometheus.io/port: "8080"
    spec:
      containers:
        - name: order-service
          image: ecommerce/order-service:latest
          ports:
            - containerPort: 8080
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: Production
            - name: ConnectionStrings__Default
              valueFrom:
                secretKeyRef:
                  name: order-service-secrets
                  key: db-connection-string
          resources:
            requests:
              memory: "128Mi"
              cpu: "100m"
            limits:
              memory: "256Mi"
              cpu: "500m"
          livenessProbe:
            httpGet:
              path: /health/live
              port: 8080
            initialDelaySeconds: 10
            periodSeconds: 15
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            initialDelaySeconds: 5
            periodSeconds: 10
          lifecycle:
            preStop:
              exec:
                command: ["/bin/sh", "-c", "sleep 5"]
```

**10.2. Service**
```yaml
# k8s/base/order-service/service.yml
apiVersion: v1
kind: Service
metadata:
  name: order-service
  namespace: ecommerce
spec:
  selector:
    app: order-service
  ports:
    - name: http
      port: 80
      targetPort: 8080
  type: ClusterIP
```

**10.3. Ingress (NGINX)**
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ecommerce-ingress
  namespace: ecommerce
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    nginx.ingress.kubernetes.io/rate-limit: "100"
spec:
  ingressClassName: nginx
  rules:
    - host: api.ecommerce.local
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: api-gateway
                port: { number: 80 }
```

**10.4. HPA — Horizontal Pod Autoscaler**
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: order-service-hpa
  namespace: ecommerce
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: order-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization: 80
```

**10.5. Setup Kind cluster**
```bash
# Install Kind
kind create cluster --name ecommerce --config kind-config.yml

# Install NGINX Ingress
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml

# Install Metrics Server (cho HPA)
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml

# Deploy
kubectl create namespace ecommerce
kubectl apply -k infra/k8s/overlays/dev/
```

### Checklist giai đoạn 10
- [ ] Kind cluster chạy local
- [ ] Deployment, Service, ConfigMap, Secret đúng
- [ ] Ingress route đến đúng service
- [ ] Liveness/Readiness probe hoạt động
- [ ] HPA scale khi CPU cao
- [ ] Rolling update: `kubectl set image` không downtime
- [ ] Secrets không bị lộ trong git (dùng sealed-secrets hoặc env)

---

## Giai đoạn 11 — Testing (Tuần 15–16)

### Mục tiêu
Unit test domain logic, integration test với container thật, không mock Kafka/DB.

### Test pyramid

```
         /\
        /E2E\          ← ít, chậm, tốn kém
       /──────\
      /  Integ \       ← Testcontainers (Kafka, Redis, DB)
     /──────────\
    /  Unit Test  \    ← nhiều, nhanh, không I/O
   ──────────────────
```

### Việc cần làm

**11.1. Unit test domain**
```csharp
// Unit/ECommerceMS.Order.UnitTests/Domain/OrderTests.cs
public sealed class OrderTests
{
    [Fact]
    public void Create_WithValidItems_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 2, Money.From(100_000)),
            OrderItem.Create(Guid.NewGuid(), 1, Money.From(50_000))
        };

        // Act
        var order = Order.Create(new CustomerId(Guid.NewGuid()), items);

        // Assert
        order.TotalAmount.Should().Be(Money.From(250_000));
        order.Status.Should().Be(OrderStatus.Pending);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCreatedEvent);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ShouldThrow()
    {
        // Arrange
        var order = OrderFaker.CreateConfirmedOrder();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<OrderInvalidStatusException>()
           .WithMessage("*cannot transition*");
    }
}
```

**11.2. Integration test với Testcontainers**
```csharp
// Integration/ECommerceMS.Order.IntegrationTests/OrderApiTests.cs
public sealed class OrderApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Default"] = _postgres.GetConnectionString(),
                        ["Redis:ConnectionString"] = _redis.GetConnectionString()
                    });
                });
            });

        _client = _factory.CreateClient();

        // Run migrations
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await db.Database.MigrateAsync();
    }

    [Fact]
    public async Task CreateOrder_WithValidPayload_ShouldReturn201()
    {
        // Arrange
        var payload = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            Items: [new(Guid.NewGuid(), 2, 100_000)]
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", payload);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        order!.Status.Should().Be("Pending");
        order.TotalAmount.Should().Be(200_000);
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
    }
}
```

**11.3. Kafka integration test**
```csharp
public sealed class KafkaIntegrationTests : IAsyncLifetime
{
    private readonly KafkaContainer _kafka = new KafkaBuilder()
        .WithImage("confluentinc/cp-kafka:7.6.1")
        .Build();

    [Fact]
    public async Task CreateOrder_ShouldPublishOrderCreatedEvent()
    {
        // Arrange
        var consumer = CreateKafkaConsumer("order.created");
        consumer.Subscribe("order.created");

        // Act
        await _client.PostAsJsonAsync("/api/orders", validPayload);

        // Assert — wait for message with timeout
        var result = consumer.Consume(TimeSpan.FromSeconds(10));
        result.Should().NotBeNull();

        var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(result!.Message.Value);
        evt!.CustomerId.Should().Be(validPayload.CustomerId);
    }
}
```

### Checklist giai đoạn 11
- [ ] Unit test coverage Domain > 90%
- [ ] Integration test: Create, Cancel, Get order
- [ ] Kafka consumer test với Testcontainers
- [ ] FluentAssertions cho readable assertions
- [ ] Bogus tạo fake data cho tests
- [ ] CI chạy tất cả tests trước khi merge

---

## Giai đoạn 12 — CI/CD & Hoàn thiện (Tuần 17–18)

### Mục tiêu
Tự động build, test, đóng gói Docker image và deploy khi push code.

### GitHub Actions pipeline

**12.1. Pipeline per service**
```yaml
# .github/workflows/order-service.yml
name: Order Service CI/CD

on:
  push:
    branches: [main, develop]
    paths:
      - "src/Services/Order/**"
      - "src/Shared/**"
      - ".github/workflows/order-service.yml"
  pull_request:
    paths:
      - "src/Services/Order/**"

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}/order-service

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0.x"

      - name: Restore
        run: dotnet restore src/Services/Order/ECommerceMS.Order.API/ECommerceMS.Order.API.csproj

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Unit Tests
        run: dotnet test tests/Unit/ECommerceMS.Order.UnitTests/ --no-build --configuration Release \
          --logger "trx;LogFileName=unit-results.xml" \
          --collect:"XPlat Code Coverage"

      - name: Integration Tests
        run: dotnet test tests/Integration/ECommerceMS.Order.IntegrationTests/ --no-build --configuration Release \
          --logger "trx;LogFileName=integration-results.xml"

      - name: Publish test results
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: Order Service Tests
          path: "**/*.xml"
          reporter: dotnet-trx

  build-and-push:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    permissions:
      packages: write

    steps:
      - uses: actions/checkout@v4

      - name: Log in to Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=sha,prefix=,suffix=,format=short
            type=raw,value=latest

      - name: Build and push
        uses: docker/build-push-action@v5
        with:
          context: .
          file: src/Services/Order/ECommerceMS.Order.API/Dockerfile
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
```

### Checklist giai đoạn 12
- [ ] GitHub Actions chạy test tự động khi push
- [ ] Docker image build và push lên ghcr.io
- [ ] Không deploy nếu test fail
- [ ] Branch protection: require passing CI trước khi merge
- [ ] README đầy đủ: setup, run, architecture
- [ ] `docker compose up` từ README chạy được ngay

---

## 16. Business domain & nghiệp vụ

### Order State Machine

```
Pending ──→ Confirmed ──→ Shipping ──→ Delivered
   │              │                         │
   └──→ Cancelled ◄─────────────────────────┘
                  ↑
           (payment failed)
```

### Kafka event contracts (Shared.Contracts)

```csharp
// OrderCreatedEvent.cs
public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    List<OrderItemEvent> Items,
    decimal TotalAmount,
    DateTime CreatedAt
);

// PaymentCompletedEvent.cs
public record PaymentCompletedEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Method,
    DateTime ProcessedAt
);

// StockReservedEvent.cs
public record StockReservedEvent(
    Guid OrderId,
    List<ReservedItem> Items,
    DateTime ReservedAt
);
```

### API Endpoints tổng hợp

| Method | Endpoint | Service | Auth |
|---|---|---|---|
| POST | `/api/orders` | Order | ✅ |
| GET | `/api/orders/{id}` | Order | ✅ |
| GET | `/api/orders` | Order | ✅ |
| PATCH | `/api/orders/{id}/cancel` | Order | ✅ |
| GET | `/api/products` | Product | ❌ |
| GET | `/api/products/{id}` | Product | ❌ |
| POST | `/api/products` | Product | ✅ Admin |
| GET | `/api/products/search?q=...` | Product | ❌ |
| POST | `/api/payments/checkout` | Payment | ✅ |
| GET | `/api/payments/{id}` | Payment | ✅ |
| GET | `/health/live` | All | ❌ |
| GET | `/health/ready` | All | ❌ |
| WS | `/hubs/orders` | Notification | ✅ |

---

## 17. Checklist hoàn thành

### Kiến trúc
- [ ] 5 service độc lập: Identity, Order, Product, Payment, Notification
- [ ] Mỗi service có DB riêng (database-per-service)
- [ ] Không có service nào gọi thẳng DB của service khác
- [ ] API Gateway là điểm vào duy nhất

### Tech stack
- [ ] YARP API Gateway với JWT, rate limit, routing
- [ ] Keycloak: OAuth2, OIDC, Realm, Client config
- [ ] Kafka: 7 topics, producer/consumer, Outbox
- [ ] MassTransit: Saga pattern, retry, dead letter queue
- [ ] Redis: Cache-aside, cart, distributed lock
- [ ] Elasticsearch: Full-text search, aggregation
- [ ] SignalR: Real-time order tracking, Redis backplane
- [ ] Hangfire: Scheduled jobs, retry queue, dashboard
- [ ] Polly: Retry, circuit breaker, timeout
- [ ] Serilog + Seq: Structured logging
- [ ] OpenTelemetry + Jaeger: Distributed tracing
- [ ] Prometheus + Grafana: Metrics dashboard
- [ ] Health checks: /live, /ready, detail per dependency

### DevOps
- [ ] Multi-stage Dockerfile mỗi service
- [ ] Docker Compose: `docker compose up` chạy toàn bộ
- [ ] k8s: Deployment, Service, Ingress, HPA
- [ ] GitHub Actions: Test → Build → Push → Deploy

### Testing
- [ ] Unit test domain logic > 80% coverage
- [ ] Integration test với Testcontainers
- [ ] Kafka consumer test
- [ ] API endpoint test (happy path + error cases)

### Portfolio
- [ ] README đầy đủ với architecture diagram
- [ ] Swagger/Scalar UI có thể demo
- [ ] `docker compose up` chạy được từ repo
- [ ] Mỗi giai đoạn có git commit rõ ràng

---

*Tài liệu này được tạo bởi Claude — cập nhật lần cuối: 2025*
