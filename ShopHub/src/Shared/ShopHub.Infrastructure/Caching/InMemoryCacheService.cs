using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ShopHub.Infrastructure.Caching;

/// <summary>
/// In-Memory caching service implementation (fallback cho Redis)
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<InMemoryCacheService> _logger;

    public InMemoryCacheService(
        IMemoryCache memoryCache,
        ILogger<InMemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Lấy giá trị từ in-memory cache
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug($"Cache hit for key: {key}");
                return await Task.FromResult(value);
            }

            _logger.LogDebug($"Cache miss for key: {key}");
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting cache for key: {key}");
            return default;
        }
    }

    /// <summary>
    /// Set giá trị vào in-memory cache
    /// </summary>
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
            {
                cacheOptions.AbsoluteExpirationRelativeToNow = expiration;
            }

            _memoryCache.Set(key, value, cacheOptions);
            _logger.LogDebug($"Set cache for key: {key}, expiration: {expiration?.TotalSeconds ?? 0}s");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting cache for key: {key}");
        }
    }

    /// <summary>
    /// Xóa giá trị từ in-memory cache
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _memoryCache.Remove(key);
            _logger.LogDebug($"Removed cache for key: {key}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache for key: {key}");
        }
    }

    /// <summary>
    /// Kiểm tra key có tồn tại trong in-memory cache
    /// </summary>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking cache existence for key: {key}");
            return false;
        }
    }

    /// <summary>
    /// Xóa tất cả cache có prefix (limitation: không thể xóa by prefix trong MemoryCache)
    /// </summary>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            // MemoryCache không support remove by prefix natively
            // Cần track keys hoặc dùng Redis cho functionality này
            _logger.LogWarning($"RemoveByPrefixAsync not fully supported in MemoryCache. Prefix: {prefix}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache by prefix: {prefix}");
        }
    }
}