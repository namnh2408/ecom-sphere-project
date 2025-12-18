namespace Catalog.Domain.Exceptions;

/// <summary>
/// Base exception for Catalog domain
/// </summary>
public class CatalogDomainException : Exception
{
    public string Code { get; }

    public CatalogDomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    public static CatalogDomainException InvalidPrice(decimal price)
        => new("INVALID_PRICE", $"Price cannot be negative. Provided price: {price}");

    public static CatalogDomainException InvalidStock(int quantity)
        => new("INVALID_STOCK", $"Stock quantity cannot be negative. Provided quantity: {quantity}");

    public static CatalogDomainException InvalidProductName(string name)
        => new("INVALID_PRODUCT_NAME", $"Product name must be between 3 and 500 characters. Provided: {name}");

    public static CatalogDomainException InvalidSku(string sku)
        => new("INVALID_SKU", $"SKU is invalid. Provided: {sku}");

    public static CatalogDomainException InvalidCategoryName(string name)
        => new("INVALID_CATEGORY_NAME", $"Category name must be between 1 and 200 characters. Provided: {name}");

    public static CatalogDomainException ProductNotFound(Guid productId)
        => new("PRODUCT_NOT_FOUND", $"Product with ID {productId} was not found");

    public static CatalogDomainException CategoryNotFound(Guid categoryId)
        => new("CATEGORY_NOT_FOUND", $"Category with ID {categoryId} was not found");

    public static CatalogDomainException ProductOutOfStock(Guid productId)
        => new("PRODUCT_OUT_OF_STOCK", $"Product with ID {productId} is out of stock");

    public static CatalogDomainException InsufficientStock(Guid productId, int available, int requested)
        => new("INSUFFICIENT_STOCK", $"Product {productId} has only {available} units available, but {requested} requested");

    public static CatalogDomainException DuplicateSku(string sku)
        => new("DUPLICATE_SKU", $"A product with SKU '{sku}' already exists");
}