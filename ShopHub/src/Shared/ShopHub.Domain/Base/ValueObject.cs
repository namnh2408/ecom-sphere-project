namespace ShopHub.Domain.Base;

/// <summary>
/// Base class cho Value Objects
/// Value Objects không có định danh - chúng được đặc trưng bởi giá trị của chúng
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Trả về các properties được dùng để so sánh
    /// </summary>
    public abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// So sánh hai value objects
    /// </summary>
    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(
            other.GetEqualityComponents());
    }

    /// <summary>
    /// So sánh hai value objects (object overload)
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var valueObject = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(
            valueObject.GetEqualityComponents());
    }

    /// <summary>
    /// Lấy hash code dựa trên components
    /// </summary>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Operator == để so sánh
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Operator != để so sánh
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}