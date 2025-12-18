# Redis Configuration Documentation

## Overview

Redis đã được tích hợp vào project với đầy đủ support cho 4 mục đích chính:

1. **Distributed Caching** - Cache dữ liệu ở mức ứng dụng
2. **Session Management** - Quản lý session người dùng
3. **Event Bus** - Pub/Sub pattern cho inter-service communication
4. **Message Queue** - Hàng đợi xử lý bất đồng bộ

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│              Application Services                        │
│  (Cart, Catalog, Orders, Payment, Shipping, Users)     │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────┴────────────────────────────────────────┐
│           Abstractions (BuildingBlocks)                  │
│  ├─ ICacheService (Caching)                             │
│  ├─ ISessionService (Session Management)                │
│  ├─ IRedisEventBus (Event Bus)                          │
│  └─ IRedisMessageQueue (Message Queue)                  │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────┴────────────────────────────────────────┐
│     Infrastructure.Shared (Redis Implementation)         │
│  ├─ CacheService                                        │
│  ├─ SessionService                                      │
│  ├─ RedisEventBus                                       │
│  ├─ RedisMessageQueue                                   │
│  └─ RedisExtensions (DI Registration)                   │
└────────────────┬────────────────────────────────────────┘
                 │
                 ▼
            ┌─────────┐
            │  Redis  │
            └─────────┘
```

## Configuration Files

### New Abstractions Created

| File | Purpose |
|------|---------|
| `ICacheService.cs` | Distributed cache interface |
| `ISessionService.cs` | Session management interface |
| `IRedisEventBus.cs` | Event pub/sub interface |
| `IRedisMessageQueue.cs` | Message queue interface |
| `IRedisConnectionProvider.cs` | Redis connection management |

### New Implementations Created

| File | Purpose |
|------|---------|
| `RedisOptions.cs` | Configuration class |
| `RedisConnectionProvider.cs` | Connection management |
| `CacheService.cs` | Cache implementation |
| `SessionService.cs` | Session implementation |
| `RedisEventBus.cs` | Event bus implementation |
| `RedisMessageQueue.cs` | Message queue implementation |
| `RedisExtensions.cs` | DI registration |

## Setup Instructions

### 1. Update appsettings.json

Add Redis configuration to your `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
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

### 2. Register Services in Program.cs

```csharp
using BuildingBlocks.Infrastructure.Shared.Redis;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Redis
builder.Services.AddRedis(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();
app.MapControllers();

app.Run();
```

### 3. Install Redis

#### Option A: Docker (Recommended)

```bash
docker run -d \
  --name redis \
  -p 6379:6379 \
  redis:7-alpine
```

#### Option B: Download

Visit [redis.io](https://redis.io/download) for installation instructions.

## Usage Examples

### Cache Service

```csharp
public class UserService
{
    private readonly ICacheService _cache;

    public UserService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<User> GetUserAsync(int userId, CancellationToken ct)
    {
        return await _cache.GetOrSetAsync(
            key: $"user:{userId}",
            factory: async (_) => await FetchUserFromDb(userId),
            expiration: TimeSpan.FromHours(1),
            cancellationToken: ct
        );
    }

    public async Task InvalidateUserCacheAsync(int userId, CancellationToken ct)
    {
        await _cache.RemoveAsync($"user:{userId}", ct);
    }
}
```

### Session Service

```csharp
public class AuthController
{
    private readonly ISessionService _session;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await ValidateCredentials(request);
        
        var sessionData = new Dictionary<string, object>
        {
            { "UserId", user.Id },
            { "Email", user.Email }
        };

        var sessionId = await _session.CreateSessionAsync(
            sessionData,
            TimeSpan.FromMinutes(30),
            ct
        );

        return Ok(new { sessionId });
    }
}
```

### Event Bus

**Publisher:**
```csharp
await _eventBus.PublishAsync(
    new UserCreatedEvent { UserId = userId, Email = email },
    cancellationToken
);
```

**Subscriber (IHostedService):**
```csharp
public class UserNotificationService : BackgroundService
{
    private readonly IRedisEventBus _eventBus;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _eventBus.SubscribeAsync<UserCreatedEvent>(
            async (@event, ct) => await SendWelcomeEmail(@event.Email),
            stoppingToken
        );
    }
}
```

### Message Queue

**Enqueue:**
```csharp
await _messageQueue.EnqueueAsync(
    queue: "email-sending",
    message: new EmailMessage { To = email, Subject = "Welcome" },
    cancellationToken: ct
);
```

**Process (IHostedService):**
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await _messageQueue.ProcessQueueAsync<EmailMessage>(
        queue: "email-sending",
        handler: async (message, ct) => await SendEmail(message, ct),
        batchSize: 10,
        cancellationToken: stoppingToken
    );
}
```

## Configuration Details

### RedisOptions Properties

| Property | Default | Description |
|----------|---------|-------------|
| `ConnectionString` | `localhost:6379` | Redis connection string |
| `DefaultDatabase` | `0` | Default DB number |
| `ConnectTimeout` | `5000` | Connection timeout (ms) |
| `SyncTimeout` | `5000` | Sync timeout (ms) |
| `AllowAdmin` | `false` | Allow admin commands |
| `Ssl` | `false` | Use SSL connection |
| `Password` | `null` | Authentication password |
| `DefaultExpiration` | `60` | Default cache TTL (minutes) |
| `SessionExpiration` | `1440` | Session TTL (minutes) |
| `EnableCompression` | `true` | Compress large values |
| `CacheKeyPrefix` | `cache:` | Prefix for cache keys |
| `SessionKeyPrefix` | `session:` | Prefix for session keys |
| `EventBusChannelPrefix` | `event:` | Prefix for event channels |
| `MessageQueuePrefix` | `queue:` | Prefix for queue keys |

## Key Naming Convention

Following Redis best practices:

```
cache:module:entity:id
session:sessionid
event:EventTypeName
queue:queuename
```

Examples:
- `cache:users:profile:123`
- `cache:catalog:products:456`
- `session:550e8400-e29b-41d4-a716-446655440000`
- `event:UserCreatedEvent`
- `queue:email-sending`

## Performance Tips

1. **Use appropriate expiration times**
   - Short-lived data: 5-15 minutes
   - Session data: 15 minutes to 24 hours
   - Reference data: 1-7 days

2. **Batch operations when possible**
   - Use `RemoveByPatternAsync` for bulk deletions
   - Process queue messages in batches

3. **Monitor memory usage**
   - Set `maxmemory-policy` in Redis config
   - Use eviction policies: `allkeys-lru`, `volatile-lru`

4. **Enable compression** for large objects
   - Reduces network traffic
   - Saves memory in Redis

## Environment-Specific Configuration

### Development

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "AllowAdmin": true,
    "DefaultExpiration": 5
  }
}
```

### Staging

```json
{
  "Redis": {
    "ConnectionString": "redis-staging.internal:6379",
    "Password": "${REDIS_PASSWORD}",
    "DefaultExpiration": 60
  }
}
```

### Production

```json
{
  "Redis": {
    "ConnectionString": "redis-prod.internal:6379",
    "Password": "${REDIS_PASSWORD}",
    "Ssl": true,
    "AllowAdmin": false,
    "DefaultExpiration": 120
  }
}
```

## Migration Guide for Existing Services

If you have existing services, here's how to integrate Redis:

### Before:
```csharp
public class ProductService
{
    public async Task<Product> GetProductAsync(int id)
    {
        return await _db.Products.FindAsync(id);
    }
}
```

### After:
```csharp
public class ProductService
{
    private readonly ICacheService _cache;
    private readonly IProductRepository _repository;

    public ProductService(ICacheService cache, IProductRepository repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<Product> GetProductAsync(int id, CancellationToken ct)
    {
        return await _cache.GetOrSetAsync(
            $"product:{id}",
            async _ => await _repository.GetByIdAsync(id),
            TimeSpan.FromHours(1),
            ct
        );
    }
}
```

## Troubleshooting

### Connection Issues

```csharp
public async Task TestConnectionAsync()
{
    var provider = _serviceProvider.GetRequiredService<IRedisConnectionProvider>();
    
    if (!provider.IsConnected)
    {
        Console.WriteLine("Redis is not connected!");
        return;
    }

    var pong = await provider.Database.ExecuteAsync("PING");
    Console.WriteLine($"Redis: {pong}");
}
```

### Common Issues

| Issue | Solution |
|-------|----------|
| Connection timeout | Check Redis is running, verify host/port in config |
| Authentication failed | Verify password in config matches Redis password |
| Out of memory | Check memory usage, configure eviction policy |
| Slow performance | Check network latency, Redis load, key expiration settings |

## Next Steps

1. Review `README.md` in `Redis` folder for detailed usage examples
2. Implement Redis cache in your services
3. Set up event subscribers in your modules
4. Configure message queues for async operations
5. Monitor Redis metrics using Redis monitoring tools

## Related Documentation

- [ICacheService API](./CacheService.md)
- [ISessionService API](./SessionService.md)
- [IRedisEventBus API](./EventBus.md)
- [IRedisMessageQueue API](./MessageQueue.md)