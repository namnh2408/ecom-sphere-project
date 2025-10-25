namespace ShopHub.Domain.Abstractions;

/// <summary>
/// Interface đánh dấu một class là Aggregate Root
/// Aggregate Root quản lý domain events và transactional boundaries
/// </summary>
public interface IAggregateRoot : IEntity
{
    /// <summary>
    /// Danh sách các domain events chưa được phát hành
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Xóa tất cả domain events (sau khi phát hành)
    /// </summary>
    void ClearDomainEvents();
}