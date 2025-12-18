using System.Text.RegularExpressions;
using BuildingBlocks.Abstractions;

namespace Users.Domain.ValueObjects;

/// <summary>
/// Email value object with validation
/// </summary>
public class Email : IEquatable<Email>
{
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<Email>.Fail("EMAIL_REQUIRED", "Email is required");
        }

        email = email.Trim().ToLowerInvariant();

        if (email.Length > 255)
        {
            return Result<Email>.Fail("EMAIL_TOO_LONG", "Email cannot exceed 255 characters");
        }

        if (!Regex.IsMatch(email, EmailPattern))
        {
            return Result<Email>.Fail("EMAIL_INVALID", "Email format is invalid");
        }

        return Result<Email>.Success(new Email(email));
    }

    public bool Equals(Email? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Email email && Equals(email);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(Email? left, Email? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Email? left, Email? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Value;
    }
}