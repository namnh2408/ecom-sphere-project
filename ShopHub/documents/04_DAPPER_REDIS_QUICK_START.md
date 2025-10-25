# Dapper + Redis Quick Start Guide

## 🚀 5-Minute Setup

### 1. Start Redis
```bash
# Option 1: Docker (Recommended)
docker run -d -p 6379:6379 --name redis redis:latest

# Option 2: Local Redis
redis-server

# Verify
redis-cli ping  # Should output: PONG
```

### 2. Configuration
**appsettings.json**
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### 3. Done! ✅

---

## 📖 Common Patterns

### Pattern 1: Query with Redis Caching

```csharp
// Query Definition
[Cacheable(durationSeconds: 300)]
public class SearchProductsQuery : IQuery<PaginatedList<ProductResponseDto>>
{
    public string SearchTerm { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

// Handler with Dapper + Redis
public class SearchProductsQueryHandler : 
    IQueryHandler<SearchProductsQuery, PaginatedList<ProductResponseDto>>
{
    private readonly IProductQueryRepository _queryRepository;  // Dapper
    private readonly ICacheService _cacheService;              // Redis
    
    public async Task<Result<PaginatedList<ProductResponseDto>>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"search_products:{request.SearchTerm}:page{request.PageNumber}";
        
        // Try Redis cache
        var cached = await _cacheService.GetAsync<PaginatedList<ProductResponseDto>>(
            cacheKey);
        if (cached != null)
            return Result<PaginatedList<ProductResponseDto>>.Success(cached);
        
        // Query database with Dapper
        var products = await _queryRepository.SearchAsync(
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
        
        var result = new PaginatedList<ProductResponseDto>(...);
        
        // Cache result
        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromSeconds(300));
        
        return Result<PaginatedList<ProductResponseDto>>.Success(result);
    }
}
```

### Pattern 2: Invalidate Cache on Update

```csharp
public class UpdateProductCommandHandler : 
    ICommandHandler<UpdateProductCommand, ProductResponseDto>
{
    private readonly IProductRepository _repository;        // EF Core
    private readonly ICacheService _cacheService;          // Redis
    
    public async Task<Result<ProductResponseDto>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Update with EF Core
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        product.Update(request.Name, request.Price, ...);
        await _repository.UpdateAsync(product, cancellationToken);
        
        // Invalidate related caches
        await _cacheService.RemoveAsync($"product:{request.Id}");
        await _cacheService.RemoveByPrefixAsync("search_products");
        await _cacheService.RemoveByPrefixAsync("featured_products");
        
        return Result<ProductResponseDto>.Success(MapToDto(product));
    }
}
```

### Pattern 3: Create New Query with Dapper

```csharp
// Step 1: Add method to IProductQueryRepository
public interface IProductQueryRepository
{
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId);
}

// Step 2: Implement in ProductQueryRepository
public class ProductQueryRepository : IProductQueryRepository
{
    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId)
    {
        using (var connection = CreateConnection())
        {
            var sql = @"
                SELECT Id, Name, Price, Stock, ...
                FROM Products
                WHERE CategoryId = @CategoryId AND IsDeleted = 0
                ORDER BY Name";
            
            var products = await connection.QueryAsync<Product>(
                sql,
                new { CategoryId = categoryId });
            
            return products.ToList();
        }
    }
}

// Step 3: Use in Handler
public class GetProductsByCategoryQueryHandler : 
    IQueryHandler<GetProductsByCategoryQuery, IReadOnlyList<ProductResponseDto>>
{
    private readonly IProductQueryRepository _queryRepository;
    private readonly ICacheService _cacheService;
    
    public async Task<Result<IReadOnlyList<ProductResponseDto>>> Handle(
        GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"category_products:{request.CategoryId}";
        
        var cached = await _cacheService.GetAsync<IReadOnlyList<ProductResponseDto>>(cacheKey);
        if (cached != null)
            return Result<IReadOnlyList<ProductResponseDto>>.Success(cached);
        
        var products = await _queryRepository.GetByCategoryAsync(request.CategoryId);
        var dtos = products.Select(MapToDto).ToList();
        
        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromSeconds(600));
        
        return Result<IReadOnlyList<ProductResponseDto>>.Success(dtos);
    }
}
```

---

## 🔍 Debugging

### Check Redis
```bash
redis-cli
> KEYS search_products:*        # See all search cache keys
> GET "search_products:laptop"  # Get specific key
> TTL "search_products:laptop"  # Check expiration
> FLUSHDB                        # Clear all cache
```

### Check Logs
```
[DEBUG] Cache hit for key: search_products:laptop:page1
[DEBUG] Cache miss for key: search_products:laptop:page1
[DEBUG] Set cache for key: search_products:laptop:page1, expiration: 300s
```

---

## ⚡ Performance Tips

### ✅ DO
- ✅ Cache read-heavy queries
- ✅ Invalidate on write operations
- ✅ Use reasonable TTLs (5-15 minutes for search)
- ✅ Monitor cache hit rate

### ❌ DON'T
- ❌ Cache write operations
- ❌ Cache sensitive data
- ❌ Forget to invalidate on updates
- ❌ Use very long TTLs (stale data)

---

## 📊 Cache Hit vs Miss

```
First Request (Cache Miss)
/search?term=laptop
├─ Redis: Not found
├─ Database (Dapper): Query ~50ms
├─ Redis: Set cache
└─ Total: ~60ms

Second Request (Cache Hit)
/search?term=laptop
├─ Redis: Found! ~2ms
└─ Total: ~5ms

✅ 12x faster with cache!
```

---

## 🧪 Test Your Setup

```csharp
// In controller or test
public class TestController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;
    
    [HttpGet("test-cache")]
    public async Task<IActionResult> TestCache()
    {
        // Test 1: Search (should miss cache first time)
        var query1 = new SearchProductsQuery("laptop", 1, 10);
        var result1 = await _mediator.Send(query1);
        
        // Test 2: Same search (should hit cache)
        var query2 = new SearchProductsQuery("laptop", 1, 10);
        var result2 = await _mediator.Send(query2);
        
        // Test 3: Verify cache
        var key = "search_products:laptop:page1:size10";
        var cached = await _cacheService.GetAsync<PaginatedList<ProductResponseDto>>(key);
        
        return Ok(new
        {
            message = "Cache working!",
            hasCachedData = cached != null
        });
    }
    
    [HttpDelete("clear-cache")]
    public async Task<IActionResult> ClearCache()
    {
        await _cacheService.RemoveByPrefixAsync("search_products");
        return Ok("Cache cleared");
    }
}
```

---

## 🆘 Troubleshooting

| Problem | Solution |
|---------|----------|
| Redis connection error | Check `redis-cli ping`, verify connection string |
| Cache not working | Check logs for cache hit/miss, verify Redis running |
| Stale data | Reduce TTL or invalidate manually |
| High memory usage | Check Redis memory with `INFO memory` |

---

## 📦 Key NuGet Packages

- **Dapper** 2.1.15 - Micro-ORM for queries
- **StackExchange.Redis** 2.7.10 - Redis client
- **Microsoft.EntityFrameworkCore** 9.0.10 - For commands (unchanged)

---

## 🎯 Next: Advanced Scenarios

1. **Custom Cache Keys** - Create prefix-based cache keys
2. **Cache Warming** - Pre-populate cache on startup
3. **Cache Analytics** - Track hit rates and performance
4. **Redis Cluster** - Scale to multiple Redis instances
5. **Cache Strategies** - Implement LRU or TTL strategies

---

**Ready to go! 🚀**

Check out `DAPPER_REDIS_GUIDE.md` for detailed documentation.