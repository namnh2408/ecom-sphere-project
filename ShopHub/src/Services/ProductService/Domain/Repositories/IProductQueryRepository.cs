using ShopHub.Services.ProductService.Domain.Entities;

namespace ShopHub.Services.ProductService.Domain.Repositories;

/// <summary>
/// Query Repository interface cho Product (chỉ đọc dữ liệu)
/// Sử dụng Dapper để optimize performance
/// </summary>
public interface IProductQueryRepository
{
    /// <summary>
    /// Tìm kiếm sản phẩm với phân trang
    /// </summary>
    Task<IReadOnlyList<Product>> SearchAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tổng số sản phẩm tìm kiếm (cho phân trang)
    /// </summary>
    Task<int> GetSearchCountAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy sản phẩm theo ID
    /// </summary>
    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy sản phẩm theo SKU
    /// </summary>
    Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy sản phẩm theo danh mục
    /// </summary>
    Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy sản phẩm nổi bật (best sellers)
    /// </summary>
    Task<IReadOnlyList<Product>> GetFeaturedAsync(
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra sản phẩm có tồn tại
    /// </summary>
    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm sản phẩm theo danh mục
    /// </summary>
    Task<int> CountByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả sản phẩm với phân trang
    /// </summary>
    Task<IReadOnlyList<Product>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        string? sortDirection = "asc",
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm tổng số sản phẩm
    /// </summary>
    Task<int> GetTotalCountAsync(
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}