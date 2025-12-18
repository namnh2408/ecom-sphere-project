using BuildingBlocks.Abstractions;

namespace Users.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a user's password is changed
/// </summary>
public record UserPasswordChangedDomainEvent(
    Guid UserId,
    string Email,
    DateTime OccurredOnUtc) : IDomainEvent;