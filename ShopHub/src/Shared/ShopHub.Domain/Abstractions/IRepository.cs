namespace ShopHub.Domain.Abstractions;

/// <summary>
/// Base repository interface cho tất cả aggregate roots
/// Repository pattern cung cấp abstraction cho data access
/// </summary>
/// <typeparam name="T">Kiểu aggregate root</typeparam>
public interface IRepository<T> where T : IAggregateRoot
{
    /// <summary>
    /// Lấy entity theo ID
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả entities
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm entity mới
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật entity
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa entity (soft delete)
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa vĩnh viễn entity
    /// </summary>
    Task DeletePermanentlyAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra entity có tồn tại không
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}