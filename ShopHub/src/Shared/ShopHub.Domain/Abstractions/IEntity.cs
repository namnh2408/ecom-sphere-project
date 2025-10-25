namespace ShopHub.Domain.Abstractions;

/// <summary>
/// Interface đánh dấu một class là Entity
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Định danh duy nhất của entity
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Người tạo
    /// </summary>
    string? CreatedBy { get; }

    /// <summary>
    /// Thời gian cập nhật cuối cùng
    /// </summary>
    DateTime? UpdatedAt { get; }

    /// <summary>
    /// Người cập nhật
    /// </summary>
    string? UpdatedBy { get; }

    /// <summary>
    /// Cờ xóa mềm (soft delete)
    /// </summary>
    bool IsDeleted { get; }
}