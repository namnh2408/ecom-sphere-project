namespace BuildingBlocks.Infrastructure.Shared.Redis;

/// <summary>
/// Redis configuration options
/// </summary>
public class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// Redis connection string (host:port)
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Default database number
    /// </summary>
    public int DefaultDatabase { get; set; } = 0;

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Sync timeout in milliseconds
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Allow admin commands
    /// </summary>
    public bool AllowAdmin { get; set; } = false;

    /// <summary>
    /// SSL enabled
    /// </summary>
    public bool Ssl { get; set; } = false;

    /// <summary>
    /// Password for Redis authentication
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    public int DefaultExpiration { get; set; } = 60;

    /// <summary>
    /// Session expiration in minutes
    /// </summary>
    public int SessionExpiration { get; set; } = 1440; // 24 hours

    /// <summary>
    /// Enable compression for large values
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Cache key prefix
    /// </summary>
    public string CacheKeyPrefix { get; set; } = "cache:";

    /// <summary>
    /// Session key prefix
    /// </summary>
    public string SessionKeyPrefix { get; set; } = "session:";

    /// <summary>
    /// Event bus channel prefix
    /// </summary>
    public string EventBusChannelPrefix { get; set; } = "event:";

    /// <summary>
    /// Message queue key prefix
    /// </summary>
    public string MessageQueuePrefix { get; set; } = "queue:";
}