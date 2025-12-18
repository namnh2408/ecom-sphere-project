using BuildingBlocks.Abstractions;

namespace Users.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a role is removed from a user
/// </summary>
public record RoleRemovedFromUserDomainEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    DateTime OccurredOnUtc) : IDomainEvent;