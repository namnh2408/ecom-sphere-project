using Catalog.Domain.Exceptions;

namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value object for product stock quantity
/// </summary>
public class Stock : IEquatable<Stock>
{
    public const int LowStockThreshold = 10;

    public int Quantity { get; private set; }

    private Stock() { }

    private Stock(int quantity)
    {
        Quantity = quantity;
    }

    /// <summary>
    /// Creates a Stock value object
    /// </summary>
    public static Stock Create(int quantity)
    {
        if (quantity < 0)
        {
            throw CatalogDomainException.InvalidStock(quantity);
        }

        return new Stock(quantity);
    }

    /// <summary>
    /// Checks if stock is available
    /// </summary>
    public bool IsAvailable => Quantity > 0;

    /// <summary>
    /// Checks if stock is low
    /// </summary>
    public bool IsLow => Quantity > 0 && Quantity <= LowStockThreshold;

    /// <summary>
    /// Increases stock by the given quantity
    /// </summary>
    public Stock Increase(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        return new Stock(Quantity + quantity);
    }

    /// <summary>
    /// Decreases stock by the given quantity
    /// </summary>
    public Stock Decrease(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        if (Quantity < quantity)
        {
            throw CatalogDomainException.InsufficientStock(Guid.Empty, Quantity, quantity);
        }

        return new Stock(Quantity - quantity);
    }

    public bool Equals(Stock? other)
    {
        return other != null && Quantity == other.Quantity;
    }

    public override bool Equals(object? obj)
    {
        return obj is Stock stock && Equals(stock);
    }

    public override int GetHashCode()
    {
        return Quantity.GetHashCode();
    }

    public override string ToString() => Quantity.ToString();

    public static bool operator ==(Stock? left, Stock? right)
    {
        return left?.Equals(right) ?? right == null;
    }

    public static bool operator !=(Stock? left, Stock? right)
    {
        return !(left == right);
    }
}