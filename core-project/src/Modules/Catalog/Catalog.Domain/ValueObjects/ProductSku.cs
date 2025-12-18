using Catalog.Domain.Exceptions;

namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value object for product SKU (Stock Keeping Unit)
/// </summary>
public class ProductSku : IEquatable<ProductSku>
{
    public string Value { get; }

    private ProductSku() { }

    private ProductSku(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a ProductSku value object
    /// </summary>
    public static ProductSku Create(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Length > 50 || sku.Length < 3)
        {
            throw CatalogDomainException.InvalidSku(sku);
        }

        return new ProductSku(sku.Trim().ToUpperInvariant());
    }

    public bool Equals(ProductSku? other)
    {
        return other != null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ProductSku sku && Equals(sku);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;

    public static bool operator ==(ProductSku? left, ProductSku? right)
    {
        return left?.Equals(right) ?? right == null;
    }

    public static bool operator !=(ProductSku? left, ProductSku? right)
    {
        return !(left == right);
    }
}