namespace BuildingBlocks.Abstractions;

/// <summary>
/// Base class for domain entities with domain events
/// </summary>
/// <typeparam name="TId">The identifier type</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Domain events that have been raised by this entity
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event
    /// </summary>
    /// <param name="domainEvent">The domain event to raise</param>
    protected void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && EqualityComparer<TId>.Default.Equals(Id, entity.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return EqualityComparer<Entity<TId>>.Default.Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}