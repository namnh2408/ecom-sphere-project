using BuildingBlocks.Abstractions;

namespace Catalog.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a product is deleted (soft-deleted)
/// </summary>
public record ProductDeletedDomainEvent(
    Guid ProductId,
    string Name,
    DateTime OccurredOnUtc
) : IDomainEvent;