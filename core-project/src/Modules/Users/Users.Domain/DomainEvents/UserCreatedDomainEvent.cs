using BuildingBlocks.Abstractions;

namespace Users.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a user is created
/// </summary>
public record UserCreatedDomainEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime OccurredOnUtc) : IDomainEvent;