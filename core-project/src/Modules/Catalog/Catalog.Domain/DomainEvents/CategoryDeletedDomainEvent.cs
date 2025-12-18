using BuildingBlocks.Abstractions;

namespace Catalog.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a category is deleted (soft-deleted)
/// </summary>
public record CategoryDeletedDomainEvent(
    Guid CategoryId,
    string Name,
    DateTime OccurredOnUtc
) : IDomainEvent;