namespace BuildingBlocks.Abstractions;

/// <summary>
/// Message queue service using Redis
/// </summary>
public interface IRedisMessageQueue
{
    /// <summary>
    /// Enqueues a message
    /// </summary>
    Task<bool> EnqueueAsync<T>(string queue, T message, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Dequeues a message
    /// </summary>
    Task<T?> DequeueAsync<T>(string queue, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Peeks at a message without removing it
    /// </summary>
    Task<T?> PeekAsync<T>(string queue, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets the queue length
    /// </summary>
    Task<long> GetQueueLengthAsync(string queue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the queue
    /// </summary>
    Task ClearQueueAsync(string queue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes messages from queue
    /// </summary>
    Task ProcessQueueAsync<T>(
        string queue,
        Func<T, CancellationToken, Task> handler,
        int batchSize = 1,
        CancellationToken cancellationToken = default) where T : class;
}