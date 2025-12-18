using BuildingBlocks.Abstractions;

namespace Catalog.Domain.DomainEvents;

/// <summary>
/// Domain event raised when product stock is updated
/// </summary>
public record StockUpdatedDomainEvent(
    Guid ProductId,
    string Sku,
    int OldQuantity,
    int NewQuantity,
    string Reason,
    DateTime OccurredOnUtc
) : IDomainEvent;