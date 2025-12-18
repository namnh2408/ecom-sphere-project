using Microsoft.EntityFrameworkCore;
using Catalog.Domain.Categories;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Category aggregate root
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _context;

    public CategoryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Update(category);
        await Task.CompletedTask;
    }

    public async Task<Category?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories.Where(c => c.Name == name);
        
        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await GetByIdAsync(categoryId, cancellationToken);
        if (category != null)
        {
            category.SoftDelete();
            _context.Categories.Update(category);
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Category> Categories, int TotalCount)> GetActiveAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Categories.Where(c => c.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var categories = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (categories, totalCount);
    }

    public async Task<IEnumerable<(Category Category, int ProductCount)>> GetWithProductCountAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var result = new List<(Category, int)>();
        foreach (var category in categories)
        {
            var productCount = await _context.Products
                .Where(p => p.CategoryId == category.Id)
                .CountAsync(cancellationToken);

            result.Add((category, productCount));
        }

        return result;
    }
}