namespace ShopHub.Domain.Base;

using Abstractions;

/// <summary>
/// Base class cho tất cả entities
/// Cung cấp các properties chung: Id, CreatedAt, UpdatedAt, IsDeleted
/// </summary>
public abstract class BaseEntity : IEntity
{
    /// <summary>
    /// Định danh duy nhất
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Người tạo
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// Thời gian cập nhật cuối cùng
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Người cập nhật
    /// </summary>
    public string? UpdatedBy { get; protected set; }

    /// <summary>
    /// Cờ xóa mềm
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Constructor protected - chỉ có các class con mới có thể khởi tạo
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    /// <summary>
    /// Override Equals dựa trên Id
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Override GetHashCode dựa trên Id
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Operator == để so sánh entities
    /// </summary>
    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null || right is null)
            return left is null && right is null;

        return left.Id == right.Id;
    }

    /// <summary>
    /// Operator != để so sánh entities
    /// </summary>
    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
}