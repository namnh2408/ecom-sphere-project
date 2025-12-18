using System.Text.Json;
using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Infrastructure.Shared.Redis;

/// <summary>
/// Session management service implementation using Redis
/// </summary>
internal class SessionService : ISessionService
{
    private readonly IRedisConnectionProvider _connectionProvider;
    private readonly RedisOptions _options;

    public SessionService(IRedisConnectionProvider connectionProvider, RedisOptions options)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<string> CreateSessionAsync(Dictionary<string, object> data, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid().ToString();
        var prefixedKey = GetPrefixedKey(sessionId);
        var serialized = JsonSerializer.Serialize(data);
        var ttl = expiration ?? TimeSpan.FromMinutes(_options.SessionExpiration);

        await _connectionProvider.ExecuteAsync(async db =>
        {
            await db.StringSetAsync(prefixedKey, serialized, ttl);
            return true;
        });

        return sessionId;
    }

    public async Task<Dictionary<string, object>?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

        var prefixedKey = GetPrefixedKey(sessionId);
        return await _connectionProvider.ExecuteAsync(async db =>
        {
            var value = await db.StringGetAsync(prefixedKey);
            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<Dictionary<string, object>>(value.ToString());
        });
    }

    public async Task UpdateSessionAsync(string sessionId, Dictionary<string, object> data, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

        var prefixedKey = GetPrefixedKey(sessionId);
        var serialized = JsonSerializer.Serialize(data);
        var ttl = expiration ?? TimeSpan.FromMinutes(_options.SessionExpiration);

        await _connectionProvider.ExecuteAsync(async db =>
        {
            await db.StringSetAsync(prefixedKey, serialized, ttl);
            return true;
        });
    }

    public async Task RemoveSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

        var prefixedKey = GetPrefixedKey(sessionId);
        await _connectionProvider.ExecuteAsync(async db =>
        {
            await db.KeyDeleteAsync(prefixedKey);
            return true;
        });
    }

    public async Task SetSessionValueAsync(string sessionId, string key, object value, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

        var session = await GetSessionAsync(sessionId, cancellationToken);
        if (session == null)
            throw new InvalidOperationException($"Session '{sessionId}' not found");

        session[key] = value;
        await UpdateSessionAsync(sessionId, session, cancellationToken: cancellationToken);
    }

    public async Task<T?> GetSessionValueAsync<T>(string sessionId, string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

        var session = await GetSessionAsync(sessionId, cancellationToken);
        if (session == null || !session.TryGetValue(key, out var value))
            return null;

        if (value is T result)
            return result;

        if (value is JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
        }

        return null;
    }

    public async Task ExtendSessionAsync(string sessionId, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

        var prefixedKey = GetPrefixedKey(sessionId);
        await _connectionProvider.ExecuteAsync(async db =>
        {
            await db.KeyExpireAsync(prefixedKey, expiration);
            return true;
        });
    }

    private string GetPrefixedKey(string sessionId) => $"{_options.SessionKeyPrefix}{sessionId}";
}