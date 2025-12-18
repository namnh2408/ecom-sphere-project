namespace BuildingBlocks.Abstractions;

/// <summary>
/// Event bus implementation using Redis Pub/Sub
/// </summary>
public interface IRedisEventBus : IEventBus
{
    /// <summary>
    /// Subscribes to events of a specific type
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    /// <param name="handler">The handler to execute when event is published</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the async operation</returns>
    Task SubscribeAsync<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Unsubscribes from events
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the async operation</returns>
    Task UnsubscribeAsync<T>(CancellationToken cancellationToken = default) where T : class;
}