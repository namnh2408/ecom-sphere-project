using BuildingBlocks.Abstractions;
using Catalog.Domain.DomainEvents;
using Catalog.Domain.Exceptions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Products;

/// <summary>
/// Product aggregate root - represents a product in the catalog
/// </summary>
public class Product : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public ProductSku Sku { get; private set; } = null!;
    public string? Description { get; private set; }
    public Money Price { get; private set; } = null!;
    public Money? CostPrice { get; private set; }
    public Stock Stock { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public string Status { get; private set; } = ProductStatus.Active; // Active, Inactive, Discontinued
    public string? PrimaryImagePath { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    private Product() { }

    private Product(
        Guid id,
        string name,
        ProductSku sku,
        string? description,
        Money price,
        Money? costPrice,
        Stock stock,
        Guid categoryId,
        string? primaryImagePath
    ) : base(id)
    {
        Name = name;
        Sku = sku;
        Description = description;
        Price = price;
        CostPrice = costPrice;
        Stock = stock;
        CategoryId = categoryId;
        PrimaryImagePath = primaryImagePath;
        Status = ProductStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    public static Product Create(
        string name,
        string sku,
        string? description,
        decimal price,
        decimal? costPrice,
        int stockQuantity,
        Guid categoryId,
        string? primaryImagePath = null
    )
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 500)
        {
            throw CatalogDomainException.InvalidProductName(name);
        }

        // Validate price
        var priceValue = Money.Create(price);

        // Validate cost price if provided
        Money? costPriceValue = null;
        if (costPrice.HasValue)
        {
            costPriceValue = Money.Create(costPrice.Value);
        }

        // Validate stock
        var stockValue = Stock.Create(stockQuantity);

        // Validate SKU
        var skuValue = ProductSku.Create(sku);

        var productId = Guid.NewGuid();
        var product = new Product(
            productId,
            name.Trim(),
            skuValue,
            description?.Trim(),
            priceValue,
            costPriceValue,
            stockValue,
            categoryId,
            primaryImagePath
        );

        // Raise domain event
        product.Raise(new ProductCreatedDomainEvent(
            productId,
            product.Name,
            product.Sku.Value,
            product.Price.Amount,
            product.Description,
            DateTime.UtcNow
        ));

        return product;
    }

    /// <summary>
    /// Updates product information
    /// </summary>
    public void Update(
        string name,
        string? description,
        decimal price,
        decimal? costPrice,
        Guid categoryId,
        string? primaryImagePath = null
    )
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 500)
        {
            throw CatalogDomainException.InvalidProductName(name);
        }

        // Validate prices
        var priceValue = Money.Create(price);
        Money? costPriceValue = null;
        if (costPrice.HasValue)
        {
            costPriceValue = Money.Create(costPrice.Value);
        }

        Name = name.Trim();
        Description = description?.Trim();
        Price = priceValue;
        CostPrice = costPriceValue;
        CategoryId = categoryId;
        if (!string.IsNullOrWhiteSpace(primaryImagePath))
        {
            PrimaryImagePath = primaryImagePath;
        }
        UpdatedAtUtc = DateTime.UtcNow;

        // Raise domain event
        Raise(new ProductUpdatedDomainEvent(
            Id,
            Name,
            Sku.Value,
            Price.Amount,
            Description,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Updates product stock
    /// </summary>
    public void UpdateStock(int newQuantity, string reason)
    {
        if (newQuantity < 0)
        {
            throw CatalogDomainException.InvalidStock(newQuantity);
        }

        var oldQuantity = Stock.Quantity;
        Stock = Stock.Create(newQuantity);
        UpdatedAtUtc = DateTime.UtcNow;

        // Raise domain event
        Raise(new StockUpdatedDomainEvent(
            Id,
            Sku.Value,
            oldQuantity,
            newQuantity,
            reason,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Reserves stock (decreases stock quantity)
    /// </summary>
    public void ReserveStock(int quantity, string reason = "Order")
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        var oldQuantity = Stock.Quantity;
        Stock = Stock.Decrease(quantity);
        UpdatedAtUtc = DateTime.UtcNow;

        Raise(new StockUpdatedDomainEvent(
            Id,
            Sku.Value,
            oldQuantity,
            Stock.Quantity,
            reason,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Releases reserved stock (increases stock quantity)
    /// </summary>
    public void ReleaseStock(int quantity, string reason = "Cancellation")
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        var oldQuantity = Stock.Quantity;
        Stock = Stock.Increase(quantity);
        UpdatedAtUtc = DateTime.UtcNow;

        Raise(new StockUpdatedDomainEvent(
            Id,
            Sku.Value,
            oldQuantity,
            Stock.Quantity,
            reason,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Deactivates the product
    /// </summary>
    public void Deactivate()
    {
        if (Status == ProductStatus.Discontinued)
        {
            throw new InvalidOperationException("Cannot deactivate a discontinued product");
        }

        Status = ProductStatus.Inactive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the product
    /// </summary>
    public void Reactivate()
    {
        if (Status == ProductStatus.Discontinued)
        {
            throw new InvalidOperationException("Cannot reactivate a discontinued product");
        }

        Status = ProductStatus.Active;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks product as discontinued
    /// </summary>
    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft deletes the product
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        Raise(new ProductDeletedDomainEvent(
            Id,
            Name,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Restores a soft-deleted product
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public override string ToString() => $"{Name} ({Sku})";
}

/// <summary>
/// Product status constants
/// </summary>
public static class ProductStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Discontinued = "Discontinued";

    public static readonly List<string> AllStatuses = new() { Active, Inactive, Discontinued };
}