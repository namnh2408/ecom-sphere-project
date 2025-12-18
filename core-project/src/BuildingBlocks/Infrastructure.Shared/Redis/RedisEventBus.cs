using System.Text.Json;
using BuildingBlocks.Abstractions;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Shared.Redis;

/// <summary>
/// Redis event bus implementation using Pub/Sub
/// </summary>
internal class RedisEventBus : IRedisEventBus, IAsyncDisposable
{
    private readonly IRedisConnectionProvider _connectionProvider;
    private readonly RedisOptions _options;
    private readonly Dictionary<string, Delegate> _subscriptions = new();

    public RedisEventBus(IRedisConnectionProvider connectionProvider, RedisOptions options)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var channelName = GetChannelName(typeof(T));
        var message = JsonSerializer.Serialize(@event);

        await _connectionProvider.ExecuteAsync(async db =>
        {
            var subscriber = _connectionProvider.Connection.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(channelName), message);
            return true;
        });
    }

    public async Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var channelName = GetChannelName(typeof(T));
        var subscriber = _connectionProvider.Connection.GetSubscriber();

        // Store the handler
        _subscriptions[channelName] = handler;

        // Subscribe to the channel
        await subscriber.SubscribeAsync(RedisChannel.Literal(channelName), async (channel, message) =>
        {
            if (!message.HasValue)
                return;

            try
            {
                var @event = JsonSerializer.Deserialize<T>(message.ToString());
                if (@event != null)
                {
                    await handler(@event, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Log the error - consider adding logging
                System.Diagnostics.Debug.WriteLine($"Error processing event from {channel}: {ex.Message}");
            }
        });
    }

    public async Task UnsubscribeAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        var channelName = GetChannelName(typeof(T));
        var subscriber = _connectionProvider.Connection.GetSubscriber();

        await subscriber.UnsubscribeAsync(RedisChannel.Literal(channelName));
        _subscriptions.Remove(channelName);
    }

    private string GetChannelName(Type eventType) => $"{_options.EventBusChannelPrefix}{eventType.Name}";

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        // Unsubscribe from all channels
        var subscriber = _connectionProvider.Connection.GetSubscriber();
        foreach (var channelName in _subscriptions.Keys)
        {
            await subscriber.UnsubscribeAsync(RedisChannel.Literal(channelName));
        }

        _subscriptions.Clear();
    }
}