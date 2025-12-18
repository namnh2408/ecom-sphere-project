using System.Text.Json;
using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Infrastructure.Shared.Redis;

/// <summary>
/// Distributed cache service implementation using Redis
/// </summary>
internal class CacheService : ICacheService
{
    private readonly IRedisConnectionProvider _connectionProvider;
    private readonly RedisOptions _options;

    public CacheService(IRedisConnectionProvider connectionProvider, RedisOptions options)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var prefixedKey = GetPrefixedKey(key);
        return await _connectionProvider.ExecuteAsync(async db =>
        {
            var value = await db.StringGetAsync(prefixedKey);
            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(value.ToString());
        });
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var prefixedKey = GetPrefixedKey(key);
        var serialized = JsonSerializer.Serialize(value);
        var ttl = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpiration);

        await _connectionProvider.ExecuteAsync(async db =>
        {
            await db.StringSetAsync(prefixedKey, serialized, ttl);
            return true;
        });
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var prefixedKey = GetPrefixedKey(key);
        await _connectionProvider.ExecuteAsync(async db =>
        {
            await db.KeyDeleteAsync(prefixedKey);
            return true;
        });
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var prefixedKey = GetPrefixedKey(key);
        return await _connectionProvider.ExecuteAsync(async db =>
        {
            return await db.KeyExistsAsync(prefixedKey);
        });
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
            return cached;

        var value = await factory(cancellationToken);
        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        var prefixedPattern = GetPrefixedKey(pattern);
        await _connectionProvider.ExecuteAsync(async db =>
        {
            var endpoints = _connectionProvider.Connection.GetEndPoints();
            if (endpoints.Length == 0)
                throw new InvalidOperationException("No Redis endpoints available");

            var server = _connectionProvider.Connection.GetServer(endpoints.FirstOrDefault() 
                ?? throw new InvalidOperationException("No Redis endpoints available"));
            
            var keys = server.Keys(pattern: prefixedPattern).ToArray();
            if (keys.Length > 0)
            {
                await db.KeyDeleteAsync(keys);
            }

            return true;
        });
    }

    public async Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var prefixedKey = GetPrefixedKey(key);
        return await _connectionProvider.ExecuteAsync(async db =>
        {
            return await db.StringIncrementAsync(prefixedKey, value);
        });
    }

    public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var prefixedKey = GetPrefixedKey(key);
        return await _connectionProvider.ExecuteAsync(async db =>
        {
            return await db.StringDecrementAsync(prefixedKey, value);
        });
    }

    private string GetPrefixedKey(string key) => $"{_options.CacheKeyPrefix}{key}";
}