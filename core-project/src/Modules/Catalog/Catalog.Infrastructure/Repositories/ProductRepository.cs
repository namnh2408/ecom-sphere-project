using Microsoft.EntityFrameworkCore;
using Catalog.Domain.Products;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Product aggregate root
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await Task.CompletedTask;
    }

    public async Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Sku.Value == sku.ToUpperInvariant(), cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Products.Where(p => p.Sku.Value == sku.ToUpperInvariant());
        
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(productId, cancellationToken);
        if (product != null)
        {
            product.SoftDelete();
            _context.Products.Update(product);
        }
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    public async Task<IEnumerable<Product>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.Status == ProductStatus.Active)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(
        Guid categoryId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId && p.Status == ProductStatus.Active)
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> SearchAsync(
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Where(p => p.Name.Contains(searchTerm) || 
                       (p.Description != null && p.Description.Contains(searchTerm)) ||
                       p.Sku.Value.Contains(searchTerm.ToUpperInvariant()));

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> FilterAsync(
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? status,
        bool? inStock,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount <= maxPrice.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (inStock.HasValue)
        {
            query = inStock.Value
                ? query.Where(p => p.Stock.Quantity > 0)
                : query.Where(p => p.Stock.Quantity == 0);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }
}