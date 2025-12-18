namespace BuildingBlocks.Abstractions;

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// When the domain event occurred
    /// </summary>
    DateTime OccurredOnUtc { get; }
}