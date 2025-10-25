using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using ShopHub.Domain.CQRS;

namespace ShopHub.Common.CQRS;

/// <summary>
/// Attribute để đánh dấu query cần cache
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CacheableAttribute : Attribute
{
    public int DurationSeconds { get; set; } = 300; // Default 5 minutes

    public CacheableAttribute(int durationSeconds = 300)
    {
        DurationSeconds = durationSeconds;
    }
}

/// <summary>
/// MediatR Behavior cho caching Query results
/// </summary>
/// <typeparam name="TRequest">Kiểu request (Query)</typeparam>
/// <typeparam name="TResponse">Kiểu response</typeparam>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IQuery<object>
    where TResponse : notnull
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        IMemoryCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Handle caching
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheAttribute = GetCacheAttribute(request);

        // Nếu không có CacheableAttribute, skip caching
        if (cacheAttribute == null)
            return await next();

        var cacheKey = GenerateCacheKey(request);

        // Kiểm tra cache
        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            _logger.LogInformation($"[CQRS] Cache hit for {typeof(TRequest).Name}");
            return (TResponse)cachedResult!;
        }

        _logger.LogInformation($"[CQRS] Cache miss for {typeof(TRequest).Name}");

        // Execute handler
        var response = await next();

        // Lưu vào cache
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheAttribute.DurationSeconds)
        };

        _cache.Set(cacheKey, response, cacheOptions);
        _logger.LogInformation($"[CQRS] Cached {typeof(TRequest).Name} for {cacheAttribute.DurationSeconds}s");

        return response;
    }

    /// <summary>
    /// Lấy CacheableAttribute từ request type
    /// </summary>
    private static CacheableAttribute? GetCacheAttribute(TRequest request)
    {
        return request.GetType()
            .GetCustomAttribute<CacheableAttribute>();
    }

    /// <summary>
    /// Generate cache key từ request và properties
    /// </summary>
    private static string GenerateCacheKey(TRequest request)
    {
        var type = typeof(TRequest);
        var properties = type.GetProperties();

        // Nếu không có properties, dùng type name làm key
        if (!properties.Any())
            return type.Name;

        // Tạo key từ type name + values của properties
        var key = type.Name;
        foreach (var prop in properties)
        {
            var value = prop.GetValue(request);
            key += $"_{prop.Name}_{value}";
        }

        return key;
    }
}