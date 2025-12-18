using BuildingBlocks.Abstractions;

namespace Catalog.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a category is updated
/// </summary>
public record CategoryUpdatedDomainEvent(
    Guid CategoryId,
    string Name,
    string? Description,
    DateTime OccurredOnUtc
) : IDomainEvent;