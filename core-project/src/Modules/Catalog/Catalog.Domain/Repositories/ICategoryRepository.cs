using Catalog.Domain.Categories;

namespace Catalog.Domain.Repositories;

/// <summary>
/// Repository for Category aggregate root
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Adds a new category to the repository
    /// </summary>
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a category by ID
    /// </summary>
    Task<Category?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a category by slug
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a category with the given name exists
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a category by ID
    /// </summary>
    Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all categories
    /// </summary>
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active categories (paginated)
    /// </summary>
    Task<(IEnumerable<Category> Categories, int TotalCount)> GetActiveAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves categories with product count
    /// </summary>
    Task<IEnumerable<(Category Category, int ProductCount)>> GetWithProductCountAsync(
        CancellationToken cancellationToken = default);
}