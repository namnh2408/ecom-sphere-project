namespace ShopHub.Infrastructure.Caching;

/// <summary>
/// Interface cho caching service (Redis/MemoryCache)
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Lấy giá trị từ cache
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set giá trị vào cache
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa giá trị từ cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra key có tồn tại trong cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa tất cả cache có prefix
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}