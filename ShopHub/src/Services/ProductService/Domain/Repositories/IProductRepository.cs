using ShopHub.Domain.Abstractions;
using ShopHub.Services.ProductService.Domain.Entities;

namespace ShopHub.Services.ProductService.Domain.Repositories;

/// <summary>
/// Repository interface cho Product aggregate
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Lấy sản phẩm theo SKU
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy sản phẩm theo danh mục
    /// </summary>
    Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm kiếm sản phẩm
    /// </summary>
    Task<IReadOnlyList<Product>> SearchAsync(
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy sản phẩm nổi bật
    /// </summary>
    Task<IReadOnlyList<Product>> GetFeaturedAsync(
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra sản phẩm có tồn tại theo SKU
    /// </summary>
    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm sản phẩm trong danh mục
    /// </summary>
    Task<int> CountByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}