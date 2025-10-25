using Microsoft.EntityFrameworkCore;
using ShopHub.Services.ProductService.Domain.Entities;
using ShopHub.Services.ProductService.Domain.Repositories;

namespace ShopHub.Services.ProductService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation cho Product
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _dbContext;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(
        ProductDbContext dbContext,
        ILogger<ProductRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Lấy sản phẩm theo ID
    /// </summary>
    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy tất cả sản phẩm
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Thêm sản phẩm mới
    /// </summary>
    public async Task AddAsync(
        Product entity,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Product {entity.Id} added");
    }

    /// <summary>
    /// Cập nhật sản phẩm
    /// </summary>
    public async Task UpdateAsync(
        Product entity,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Product {entity.Id} updated");
    }

    /// <summary>
    /// Xóa sản phẩm (soft delete)
    /// </summary>
    public async Task DeleteAsync(
        Product entity,
        CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        _dbContext.Products.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Product {entity.Id} deleted (soft)");
    }

    /// <summary>
    /// Xóa sản phẩm vĩnh viễn
    /// </summary>
    public async Task DeletePermanentlyAsync(
        Product entity,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogWarning($"Product {entity.Id} permanently deleted");
    }

    /// <summary>
    /// Kiểm tra sản phẩm có tồn tại không
    /// </summary>
    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(p => p.Id == id && !p.IsDeleted)
            .AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy sản phẩm theo SKU
    /// </summary>
    public async Task<Product?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Sku == sku && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy sản phẩm theo danh mục
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Tìm kiếm sản phẩm
    /// </summary>
    public async Task<IReadOnlyList<Product>> SearchAsync(
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.Description!.Contains(searchTerm) ||
                p.Sku!.Contains(searchTerm));
        }

        return await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy sản phẩm nổi bật
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive)
            .OrderByDescending(p => p.TotalSold)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Kiểm tra SKU có tồn tại không
    /// </summary>
    public async Task<bool> ExistsBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(p => p.Sku == sku && !p.IsDeleted)
            .AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Đếm sản phẩm trong danh mục
    /// </summary>
    public async Task<int> CountByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .CountAsync(cancellationToken);
    }
}