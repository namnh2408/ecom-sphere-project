using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace ShopHub.Infrastructure.Caching;

/// <summary>
/// Redis caching service implementation
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IDatabase _db;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
        _db = _redis.GetDatabase();
    }

    /// <summary>
    /// Lấy giá trị từ Redis cache
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
            {
                _logger.LogDebug($"Cache miss for key: {key}");
                return default;
            }

            _logger.LogDebug($"Cache hit for key: {key}");
            var deserialized = JsonSerializer.Deserialize<T>(value.ToString());
            return deserialized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting cache for key: {key}");
            return default;
        }
    }

    /// <summary>
    /// Set giá trị vào Redis cache
    /// </summary>
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, serialized, expiration);
            _logger.LogDebug($"Set cache for key: {key}, expiration: {expiration?.TotalSeconds ?? 0}s");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting cache for key: {key}");
        }
    }

    /// <summary>
    /// Xóa giá trị từ Redis cache
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
            _logger.LogDebug($"Removed cache for key: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache for key: {key}");
        }
    }

    /// <summary>
    /// Kiểm tra key có tồn tại trong Redis cache
    /// </summary>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking cache existence for key: {key}");
            return false;
        }
    }

    /// <summary>
    /// Xóa tất cả cache có prefix (ví dụ: "products:*")
    /// </summary>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().FirstOrDefault() ??
                                         throw new InvalidOperationException("No Redis server found"));
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();

            if (keys.Length > 0)
            {
                await _db.KeyDeleteAsync(keys);
                _logger.LogDebug($"Removed {keys.Length} cache entries with prefix: {prefix}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache by prefix: {prefix}");
        }
    }
}