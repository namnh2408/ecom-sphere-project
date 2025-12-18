namespace BuildingBlocks.Abstractions;

/// <summary>
/// Transactional outbox for reliable message delivery
/// </summary>
public interface IOutbox
{
    /// <summary>
    /// Enqueues a message for later delivery
    /// </summary>
    /// <param name="message">The message to enqueue</param>
    /// <param name="type">The message type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the async operation</returns>
    Task EnqueueAsync(object message, string type, CancellationToken cancellationToken = default);
}