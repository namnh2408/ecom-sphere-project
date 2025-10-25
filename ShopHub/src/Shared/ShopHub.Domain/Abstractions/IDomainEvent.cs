namespace ShopHub.Domain.Abstractions;

/// <summary>
/// Interface đánh dấu một class là Domain Event
/// Domain events được dùng để ghi lại những sự kiện quan trọng trong domain
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Định danh duy nhất của event
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Thời gian sự kiện xảy ra
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Phiên bản của event (cho versioning)
    /// </summary>
    int Version => 1;
}