namespace BuildingBlocks.Abstractions;

/// <summary>
/// Distributed cache service using Redis
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates a value in cache
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes all keys matching a pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments a numeric value in cache
    /// </summary>
    Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrements a numeric value in cache
    /// </summary>
    Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);
}