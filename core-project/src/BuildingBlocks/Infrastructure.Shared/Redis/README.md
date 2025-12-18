# Redis Configuration Guide

Hướng dẫn thiết lập và sử dụng Redis trong dự án.

## Mục đích sử dụng Redis

Redis được sử dụng cho 4 mục đích chính:

1. **Caching** - Lưu trữ cache dữ liệu (ICacheService)
2. **Session Storage** - Quản lý session người dùng (ISessionService)
3. **Event Bus** - Pub/Sub cho việc truyền sự kiện (IRedisEventBus)
4. **Message Queue** - Hàng đợi xử lý tin nhắn (IRedisMessageQueue)

## Cấu hình

### 1. appsettings.json

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "DefaultDatabase": 0,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "AllowAdmin": false,
    "Ssl": false,
    "Password": null,
    "DefaultExpiration": 60,
    "SessionExpiration": 1440,
    "EnableCompression": true,
    "CacheKeyPrefix": "cache:",
    "SessionKeyPrefix": "session:",
    "EventBusChannelPrefix": "event:",
    "MessageQueuePrefix": "queue:"
  }
}
```

### 2. Đăng ký Services trong Program.cs

#### Tùy chọn 1: Từ Configuration

```csharp
using BuildingBlocks.Infrastructure.Shared.Redis;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Thêm Redis services
builder.Services.AddRedis(builder.Configuration);

var app = builder.Build();
```

#### Tùy chọn 2: Configuration tùy chỉnh

```csharp
using BuildingBlocks.Infrastructure.Shared.Redis;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Thêm Redis services với cấu hình tùy chỉnh
builder.Services.AddRedis(options =>
{
    options.ConnectionString = "redis.example.com:6379";
    options.DefaultExpiration = 120;
    options.SessionExpiration = 1440;
    options.Password = "your_password";
});

var app = builder.Build();
```

## Sử dụng

### 1. Caching Service

```csharp
public class ProductService
{
    private readonly ICacheService _cacheService;

    public ProductService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<Product> GetProductAsync(int id, CancellationToken ct)
    {
        // Lấy từ cache, nếu không có thì gọi factory
        return await _cacheService.GetOrSetAsync(
            key: $"product:{id}",
            factory: async (_) => await FetchProductFromDb(id),
            expiration: TimeSpan.FromMinutes(30),
            cancellationToken: ct
        );
    }

    public async Task UpdateProductAsync(Product product, CancellationToken ct)
    {
        // Cập nhật DB
        await SaveProductToDb(product);
        
        // Xóa cache
        await _cacheService.RemoveAsync($"product:{product.Id}", ct);
    }

    private Task<Product> FetchProductFromDb(int id) => Task.FromResult(new Product());
    private Task SaveProductToDb(Product product) => Task.CompletedTask;
}
```

### 2. Session Service

```csharp
public class AuthController
{
    private readonly ISessionService _sessionService;

    public AuthController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost("login")]
    public async Task<LoginResponse> Login(LoginRequest request, CancellationToken ct)
    {
        // Xác thực người dùng
        var user = await AuthenticateUser(request);

        // Tạo session
        var sessionData = new Dictionary<string, object>
        {
            { "UserId", user.Id },
            { "Email", user.Email },
            { "Roles", user.Roles },
            { "LoginTime", DateTime.UtcNow }
        };

        var sessionId = await _sessionService.CreateSessionAsync(
            sessionData,
            TimeSpan.FromMinutes(30),
            ct
        );

        return new LoginResponse { SessionId = sessionId };
    }

    [HttpPost("logout")]
    public async Task Logout([FromHeader] string sessionId, CancellationToken ct)
    {
        await _sessionService.RemoveSessionAsync(sessionId, ct);
    }

    private Task<User> AuthenticateUser(LoginRequest request) => Task.FromResult(new User());
}
```

### 3. Event Bus (Pub/Sub)

#### Publisher

```csharp
public class OrderService
{
    private readonly IRedisEventBus _eventBus;

    public OrderService(IRedisEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
    {
        var order = new Order { /* ... */ };

        // Lưu vào DB
        await SaveOrderToDb(order);

        // Publish event
        await _eventBus.PublishAsync(
            new OrderCreatedEvent { OrderId = order.Id, CustomerId = order.CustomerId },
            ct
        );
    }
}
```

#### Subscriber

```csharp
public class NotificationService : IHostedService
{
    private readonly IRedisEventBus _eventBus;

    public NotificationService(IRedisEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Subscribe đến OrderCreatedEvent
        await _eventBus.SubscribeAsync<OrderCreatedEvent>(
            async (@event, ct) =>
            {
                // Gửi email thông báo
                await SendOrderConfirmationEmail(@event.CustomerId, @event.OrderId, ct);
            },
            cancellationToken
        );
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private Task SendOrderConfirmationEmail(int customerId, int orderId, CancellationToken ct) => Task.CompletedTask;
}
```

### 4. Message Queue

#### Enqueue

```csharp
public class PaymentService
{
    private readonly IRedisMessageQueue _messageQueue;

    public PaymentService(IRedisMessageQueue messageQueue)
    {
        _messageQueue = messageQueue;
    }

    public async Task ProcessPaymentAsync(Payment payment, CancellationToken ct)
    {
        // Thêm vào queue để xử lý bất đồng bộ
        await _messageQueue.EnqueueAsync(
            queue: "payment-processing",
            message: new PaymentProcessingMessage
            {
                PaymentId = payment.Id,
                Amount = payment.Amount,
                UserId = payment.UserId
            },
            cancellationToken: ct
        );
    }
}
```

#### Dequeue (Background Service)

```csharp
public class PaymentProcessingWorker : BackgroundService
{
    private readonly IRedisMessageQueue _messageQueue;

    public PaymentProcessingWorker(IRedisMessageQueue messageQueue)
    {
        _messageQueue = messageQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageQueue.ProcessQueueAsync<PaymentProcessingMessage>(
            queue: "payment-processing",
            handler: async (message, ct) =>
            {
                // Xử lý thanh toán
                await ProcessPayment(message, ct);
            },
            batchSize: 5,
            cancellationToken: stoppingToken
        );
    }

    private Task ProcessPayment(PaymentProcessingMessage message, CancellationToken ct) => Task.CompletedTask;
}
```

## Counter & Atomic Operations

```csharp
public class RateLimitingService
{
    private readonly ICacheService _cacheService;

    public RateLimitingService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<bool> IsAllowedAsync(string userId, int maxRequests = 100, CancellationToken ct = default)
    {
        var key = $"rate-limit:{userId}";
        var count = await _cacheService.IncrementAsync(key, 1, ct);

        // Set expiration on first request
        if (count == 1)
        {
            await _cacheService.RemoveAsync(key, ct);
            await _cacheService.SetAsync(key, "1", TimeSpan.FromMinutes(1), cancellationToken: ct);
            count = 1;
        }

        return count <= maxRequests;
    }
}
```

## Best Practices

1. **Key Naming** - Sử dụng tiền tố rõ ràng: `cache:user:123`, `session:abc-def`, etc.
2. **Expiration** - Đặt thời gian hết hạn hợp lý để tránh dữ liệu cũ
3. **Error Handling** - Redis có thể không khả dụng, cần handle gracefully
4. **Serialization** - Mặc định sử dụng JSON, phù hợp với các loại dữ liệu
5. **Connection Pooling** - IConnectionMultiplexer tự động quản lý connection pool
6. **Async/Await** - Luôn sử dụng async để tránh blocking operations

## Environment-specific Configuration

### Development

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "DefaultDatabase": 0,
    "AllowAdmin": true
  }
}
```

### Production

```json
{
  "Redis": {
    "ConnectionString": "redis.prod.example.com:6379",
    "DefaultDatabase": 0,
    "AllowAdmin": false,
    "Ssl": true,
    "Password": "${REDIS_PASSWORD}"
  }
}
```

## Monitoring & Troubleshooting

### Check Connection

```csharp
public class RedisHealthCheck
{
    private readonly IRedisConnectionProvider _provider;

    public bool IsConnected => _provider.IsConnected;

    public async Task<bool> PingAsync()
    {
        try
        {
            var result = await _provider.Database.ExecuteAsync("PING");
            return result.IsNull == false;
        }
        catch
        {
            return false;
        }
    }
}
```

### Performance Tips

- Sử dụng batch operations khi có thể
- Giảm kích thước value bằng compression
- Sử dụng Pipeline để giảm round-trips
- Monitor Redis memory usage

## Docker Compose

```yaml
version: '3.8'
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    command: redis-server --appendonly yes

volumes:
  redis_data:
```

## Tham khảo

- [StackExchange.Redis Documentation](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis Documentation](https://redis.io/documentation)