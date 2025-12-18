using BuildingBlocks.Abstractions;

namespace Catalog.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a product is created
/// </summary>
public record ProductCreatedDomainEvent(
    Guid ProductId,
    string Name,
    string Sku,
    decimal Price,
    string? Description,
    DateTime OccurredOnUtc
) : IDomainEvent;