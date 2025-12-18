using BuildingBlocks.Abstractions;
using Catalog.Domain.DomainEvents;
using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Categories;

/// <summary>
/// Category aggregate root - represents a product category
/// </summary>
public class Category : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Slug { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    private Category() { }

    private Category(Guid id, string name, string? description, string slug) : base(id)
    {
        Name = name;
        Description = description;
        Slug = slug;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new category
    /// </summary>
    public static Category Create(string name, string? description = null)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            throw CatalogDomainException.InvalidCategoryName(name);
        }

        var categoryId = Guid.NewGuid();
        var slug = GenerateSlug(name);
        
        var category = new Category(
            categoryId,
            name.Trim(),
            description?.Trim(),
            slug
        );

        // Raise domain event
        category.Raise(new CategoryCreatedDomainEvent(
            categoryId,
            category.Name,
            category.Description,
            DateTime.UtcNow
        ));

        return category;
    }

    /// <summary>
    /// Updates category information
    /// </summary>
    public void Update(string name, string? description = null)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            throw CatalogDomainException.InvalidCategoryName(name);
        }

        Name = name.Trim();
        Description = description?.Trim();
        Slug = GenerateSlug(name);
        UpdatedAtUtc = DateTime.UtcNow;

        // Raise domain event
        Raise(new CategoryUpdatedDomainEvent(
            Id,
            Name,
            Description,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Deactivates the category
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the category
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft deletes the category
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        Raise(new CategoryDeletedDomainEvent(
            Id,
            Name,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Restores a soft-deleted category
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates a URL-friendly slug from the category name
    /// </summary>
    private static string GenerateSlug(string name)
    {
        return name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');
    }

    public override string ToString() => $"{Name}";
}