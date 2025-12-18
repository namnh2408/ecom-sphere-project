namespace BuildingBlocks.Abstractions;

/// <summary>
/// Event bus for publishing events across modules
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all interested subscribers
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the async operation</returns>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}