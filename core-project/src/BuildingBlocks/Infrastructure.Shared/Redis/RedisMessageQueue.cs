using System.Text.Json;
using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Infrastructure.Shared.Redis;

/// <summary>
/// Message queue service implementation using Redis
/// </summary>
internal class RedisMessageQueue : IRedisMessageQueue
{
    private readonly IRedisConnectionProvider _connectionProvider;
    private readonly RedisOptions _options;

    public RedisMessageQueue(IRedisConnectionProvider connectionProvider, RedisOptions options)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<bool> EnqueueAsync<T>(string queue, T message, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(queue))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queue));

        if (message == null)
            throw new ArgumentNullException(nameof(message));

        var queueKey = GetQueueKey(queue);
        var serialized = JsonSerializer.Serialize(message);

        return await _connectionProvider.ExecuteAsync(async db =>
        {
            var result = await db.ListRightPushAsync(queueKey, serialized);
            return result > 0;
        });
    }

    public async Task<T?> DequeueAsync<T>(string queue, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(queue))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queue));

        var queueKey = GetQueueKey(queue);

        return await _connectionProvider.ExecuteAsync(async db =>
        {
            var value = await db.ListLeftPopAsync(queueKey);
            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(value.ToString());
        });
    }

    public async Task<T?> PeekAsync<T>(string queue, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(queue))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queue));

        var queueKey = GetQueueKey(queue);

        return await _connectionProvider.ExecuteAsync(async db =>
        {
            var value = await db.ListGetByIndexAsync(queueKey, 0);
            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(value.ToString());
        });
    }

    public async Task<long> GetQueueLengthAsync(string queue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queue))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queue));

        var queueKey = GetQueueKey(queue);

        return await _connectionProvider.ExecuteAsync(async db =>
        {
            return await db.ListLengthAsync(queueKey);
        });
    }

    public async Task ClearQueueAsync(string queue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queue))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queue));

        var queueKey = GetQueueKey(queue);

        await _connectionProvider.ExecuteAsync(async db =>
        {
            await db.KeyDeleteAsync(queueKey);
            return true;
        });
    }

    public async Task ProcessQueueAsync<T>(
        string queue,
        Func<T, CancellationToken, Task> handler,
        int batchSize = 1,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(queue))
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queue));

        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        while (!cancellationToken.IsCancellationRequested)
        {
            var messages = new List<T>();

            for (int i = 0; i < batchSize; i++)
            {
                var message = await DequeueAsync<T>(queue, cancellationToken);
                if (message == null)
                    break;

                messages.Add(message);
            }

            if (messages.Count == 0)
            {
                // Wait a bit before trying again
                await Task.Delay(100, cancellationToken);
                continue;
            }

            foreach (var message in messages)
            {
                try
                {
                    await handler(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log the error - consider adding logging
                    System.Diagnostics.Debug.WriteLine($"Error processing message from queue '{queue}': {ex.Message}");
                    
                    // Re-enqueue on error
                    await EnqueueAsync(queue, message, cancellationToken);
                }
            }
        }
    }

    private string GetQueueKey(string queue) => $"{_options.MessageQueuePrefix}{queue}";
}