using BuildingBlocks.Abstractions;

namespace Catalog.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a product is updated
/// </summary>
public record ProductUpdatedDomainEvent(
    Guid ProductId,
    string Name,
    string Sku,
    decimal Price,
    string? Description,
    DateTime OccurredOnUtc
) : IDomainEvent;