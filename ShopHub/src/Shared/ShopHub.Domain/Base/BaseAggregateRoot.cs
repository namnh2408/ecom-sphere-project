namespace ShopHub.Domain.Base;

using Abstractions;

/// <summary>
/// Base class cho Aggregate Roots
/// Aggregate Root quản lý domain events và consistency boundaries
/// </summary>
public abstract class BaseAggregateRoot : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Danh sách domain events chưa được phát hành
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Danh sách domain events (read-only)
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Thêm domain event
    /// </summary>
    /// <param name="domainEvent">Domain event cần thêm</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Xóa tất cả domain events
    /// Được gọi sau khi events được phát hành
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}