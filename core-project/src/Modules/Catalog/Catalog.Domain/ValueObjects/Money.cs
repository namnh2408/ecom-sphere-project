using Catalog.Domain.Exceptions;

namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value object for monetary amounts
/// </summary>
public class Money : IEquatable<Money>
{
    public decimal Amount { get; }

    private Money() { }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    /// <summary>
    /// Creates a Money value object
    /// </summary>
    public static Money Create(decimal amount)
    {
        if (amount < 0)
        {
            throw CatalogDomainException.InvalidPrice(amount);
        }

        return new Money(amount);
    }

    public bool Equals(Money? other)
    {
        return other != null && Amount == other.Amount;
    }

    public override bool Equals(object? obj)
    {
        return obj is Money money && Equals(money);
    }

    public override int GetHashCode()
    {
        return Amount.GetHashCode();
    }

    public override string ToString() => Amount.ToString("C");

    public static bool operator ==(Money? left, Money? right)
    {
        return left?.Equals(right) ?? right == null;
    }

    public static bool operator !=(Money? left, Money? right)
    {
        return !(left == right);
    }

    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Amount + right.Amount);
    }

    public static Money operator -(Money left, Money right)
    {
        return new Money(left.Amount - right.Amount);
    }

    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier);
    }
}