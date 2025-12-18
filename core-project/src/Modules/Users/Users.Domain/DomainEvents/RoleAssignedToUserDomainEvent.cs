using BuildingBlocks.Abstractions;

namespace Users.Domain.DomainEvents;

/// <summary>
/// Domain event raised when a role is assigned to a user
/// </summary>
public record RoleAssignedToUserDomainEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    DateTime OccurredOnUtc) : IDomainEvent;