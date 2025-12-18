# Redis Quick Start Guide

Hướng dẫn nhanh để thiết lập và sử dụng Redis trong dự án.

## 🚀 Quick Setup (2 phút)

### 1️⃣ Start Redis with Docker

```bash
# Sử dụng docker-compose (Recommended)
docker-compose -f docker-compose.redis.yml up -d

# Hoặc chạy Redis container trực tiếp
docker run -d -p 6379:6379 --name redis redis:7-alpine
```

### 2️⃣ Update appsettings.json

Copy và paste vào `appsettings.json`:

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

### 3️⃣ Register in Program.cs

```csharp
using BuildingBlocks.Infrastructure.Shared.Redis;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Thêm Redis
builder.Services.AddRedis(builder.Configuration);

// ... rest of configuration
var app = builder.Build();
app.Run();
```

✅ **Done!** Redis đã sẵn sàng sử dụng.

---

## 💾 Using Cache (30 seconds)

```csharp
public class ProductController
{
    private readonly ICacheService _cache;
    private readonly ProductRepository _repo;

    public ProductController(ICacheService cache, ProductRepository repo)
    {
        _cache = cache;
        _repo = repo;
    }

    [HttpGet("{id}")]
    public async Task<Product> GetProduct(int id, CancellationToken ct)
    {
        // Tự động cache nếu không có
        return await _cache.GetOrSetAsync(
            $"product:{id}",
            _ => _repo.GetByIdAsync(id),
            TimeSpan.FromHours(1),
            ct
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateRequest req, CancellationToken ct)
    {
        await _repo.UpdateAsync(id, req);
        
        // Xóa cache cũ
        await _cache.RemoveAsync($"product:{id}", ct);
        
        return Ok();
    }
}
```

---

## 👥 Managing Sessions

```csharp
public class AuthController
{
    private readonly ISessionService _session;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
    {
        var user = await ValidateUser(req); // Your auth logic
        
        var sessionData = new Dictionary<string, object>
        {
            { "UserId", user.Id },
            { "Email", user.Email },
            { "Roles", user.Roles }
        };

        var sessionId = await _session.CreateSessionAsync(sessionData, null, ct);
        
        return Ok(new { sessionId });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(
        [FromHeader(Name = "X-Session-Id")] string sessionId, 
        CancellationToken ct)
    {
        var sessionData = await _session.GetSessionAsync(sessionId, ct);
        if (sessionData == null)
            return Unauthorized("Session expired");

        var userId = (int)sessionData["UserId"];
        return Ok(new { userId });
    }
}
```

---

## 📡 Event Bus (Pub/Sub)

### Publisher

```csharp
public class OrderService
{
    private readonly IRedisEventBus _eventBus;

    public async Task CreateOrderAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var order = new Order { /* ... */ };
        await _repo.SaveAsync(order);

        // Notify subscribers
        await _eventBus.PublishAsync(
            new OrderCreatedEvent 
            { 
                OrderId = order.Id, 
                CustomerId = order.CustomerId,
                Total = order.Total
            },
            ct
        );
    }
}

public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
}
```

### Subscriber (IHostedService)

```csharp
public class OrderNotificationService : BackgroundService
{
    private readonly IRedisEventBus _eventBus;
    private readonly IEmailService _emailService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Subscribe to OrderCreatedEvent
        await _eventBus.SubscribeAsync<OrderCreatedEvent>(
            async (@event, ct) =>
            {
                await _emailService.SendOrderConfirmationAsync(
                    @event.CustomerId,
                    @event.OrderId,
                    ct
                );
            },
            stoppingToken
        );
    }
}

// Register in Program.cs
builder.Services.AddHostedService<OrderNotificationService>();
```

---

## 📬 Message Queue (Async Processing)

### Enqueue Message

```csharp
public class PaymentController
{
    private readonly IRedisMessageQueue _queue;

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment(
        PaymentRequest req, 
        CancellationToken ct)
    {
        // Add to queue for async processing
        await _queue.EnqueueAsync(
            "payment-processing",
            new PaymentMessage 
            { 
                PaymentId = Guid.NewGuid(),
                Amount = req.Amount,
                CustomerId = req.CustomerId,
                CreatedAt = DateTime.UtcNow
            },
            ct
        );

        return Ok("Payment queued for processing");
    }
}
```

### Process Queue (IHostedService)

```csharp
public class PaymentWorker : BackgroundService
{
    private readonly IRedisMessageQueue _queue;
    private readonly PaymentGateway _gateway;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _queue.ProcessQueueAsync<PaymentMessage>(
            queue: "payment-processing",
            handler: async (message, ct) =>
            {
                // Process payment
                var result = await _gateway.ProcessAsync(message.Amount, ct);
                
                if (!result.Success)
                    throw new Exception($"Payment failed: {result.Error}");
                
                await LogSuccessAsync(message, ct);
            },
            batchSize: 5, // Process 5 messages at a time
            cancellationToken: stoppingToken
        );
    }
}

public class PaymentMessage
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Register in Program.cs
builder.Services.AddHostedService<PaymentWorker>();
```

---

## 🔧 Common Patterns

### Rate Limiting

```csharp
public class RateLimitingService
{
    private readonly ICacheService _cache;

    public async Task<bool> IsAllowedAsync(
        string userId, 
        int maxRequests = 100, 
        CancellationToken ct = default)
    {
        var key = $"rate-limit:{userId}";
        var count = await _cache.IncrementAsync(key, 1, ct);

        if (count == 1)
        {
            await _cache.RemoveAsync(key, ct);
            await _cache.SetAsync(key, "1", TimeSpan.FromMinutes(1), cancellationToken: ct);
        }

        return count <= maxRequests;
    }
}
```

### Cache Invalidation

```csharp
public async Task InvalidateByPatternAsync(
    string pattern, 
    CancellationToken ct = default)
{
    // Remove all keys matching pattern
    await _cache.RemoveByPatternAsync($"product:{pattern}", ct);
}

// Usage
await InvalidateByPatternAsync("*", ct); // Clear all products
```

---

## 🚨 Troubleshooting

### ❌ Connection Error

```
Error: Connection to redis server failed
```

**Solution:**
```bash
# Check if Redis is running
docker ps | grep redis

# Restart Redis
docker-compose -f docker-compose.redis.yml restart

# Check connection string in appsettings.json
# Default: "localhost:6379"
```

### ❌ Authentication Failed

```
Error: ERR invalid password
```

**Solution:**
```bash
# If you set a password in docker-compose, add to appsettings.json
"Redis": {
  "Password": "your-password-here"
}
```

### ❌ Timeout

```
Error: Timeout connecting to the server
```

**Solution:**
- Increase `ConnectTimeout` in appsettings.json
- Check network connectivity
- Verify Redis server load

---

## 📊 Monitoring Redis

### Via Redis Commander (UI)

```bash
# Included in docker-compose.redis.yml
# Open: http://localhost:8081
```

### Via CLI

```bash
# Connect to Redis
docker exec -it redis redis-cli

# Commands
PING                    # Check connection
INFO                    # Server info
DBSIZE                  # Number of keys
KEYS pattern            # List keys
DEL key                 # Delete key
FLUSHDB                 # Clear database
MONITOR                 # Watch all commands
```

---

## 📚 Architecture

```
┌─────────────────────────────────────┐
│      Your Service Controllers       │
└────────────┬────────────────────────┘
             │
     ┌───────┴────────┐
     │                │
┌────▼─────┐   ┌─────▼─────┐
│  Cache   │   │  Session  │
└─────┬────┘   └─────┬─────┘
     │                │
┌────┴───────────────┴─────────────┐
│    Redis Connection Provider      │
└────┬───────────────┬──────────────┘
     │               │
     └───────┬───────┘
             │
        ┌────▼─────┐
        │   Redis  │
        └──────────┘
```

---

## 🎯 Best Practices

✅ **DO:**
- Use meaningful key names: `cache:user:123`, `session:abc`
- Set appropriate expiration times
- Handle Redis connection errors gracefully
- Use async/await patterns
- Batch operations when possible

❌ **DON'T:**
- Store large objects without compression
- Forget to set expiration times (memory waste)
- Hardcode Redis connection strings (use config)
- Block on synchronous Redis operations
- Ignore error handling for Redis calls

---

## 📖 Full Documentation

For detailed documentation, see:
- `src/BuildingBlocks/Infrastructure.Shared/Redis/README.md` - Full API guide
- `documents/07_Redis_Configuration.md` - Comprehensive setup guide

---

## 💡 Need Help?

Check example implementations:
1. **Cache**: See `ICacheService.cs` API
2. **Session**: See `ISessionService.cs` API
3. **Events**: See `IRedisEventBus.cs` API
4. **Queue**: See `IRedisMessageQueue.cs` API

Happy caching! 🎉