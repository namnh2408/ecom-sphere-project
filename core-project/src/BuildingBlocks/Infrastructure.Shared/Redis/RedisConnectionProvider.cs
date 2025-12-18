using BuildingBlocks.Abstractions;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Shared.Redis;

/// <summary>
/// Redis connection provider implementation
/// </summary>
internal class RedisConnectionProvider : IRedisConnectionProvider, IAsyncDisposable
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly RedisOptions _options;

    public RedisConnectionProvider(IConnectionMultiplexer connectionMultiplexer, RedisOptions options)
    {
        _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _database = _connectionMultiplexer.GetDatabase(_options.DefaultDatabase);
    }

    public IConnectionMultiplexer Connection => _connectionMultiplexer;

    public IDatabase Database => _database;

    public bool IsConnected => _connectionMultiplexer.IsConnected;

    public async Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> operation)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Redis connection is not established");

        return await operation(_database);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_connectionMultiplexer != null)
        {
            await _connectionMultiplexer.CloseAsync();
            _connectionMultiplexer.Dispose();
        }
    }
}