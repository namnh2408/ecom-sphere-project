using Catalog.Domain.Products;

namespace Catalog.Domain.Repositories;

/// <summary>
/// Repository for Product aggregate root
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Adds a new product to the repository
    /// </summary>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product
    /// </summary>
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a product by ID
    /// </summary>
    Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a product by SKU
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a product with the given SKU exists
    /// </summary>
    Task<bool> ExistsBySkuAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product by ID
    /// </summary>
    Task DeleteAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all products (paginated)
    /// </summary>
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves active products only
    /// </summary>
    Task<IEnumerable<Product>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves products by category ID
    /// </summary>
    Task<IEnumerable<Product>> GetByCategoryIdAsync(
        Guid categoryId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by name or description
    /// </summary>
    Task<(IEnumerable<Product> Products, int TotalCount)> SearchAsync(
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters products by multiple criteria
    /// </summary>
    Task<(IEnumerable<Product> Products, int TotalCount)> FilterAsync(
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? status,
        bool? inStock,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}