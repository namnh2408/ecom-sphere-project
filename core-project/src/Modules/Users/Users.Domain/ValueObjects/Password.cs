using BuildingBlocks.Abstractions;

namespace Users.Domain.ValueObjects;

/// <summary>
/// Password value object - stores hashed password
/// </summary>
public class Password : IEquatable<Password>
{
    public string Hash { get; }
    public string Salt { get; }

    private Password(string hash, string salt)
    {
        Hash = hash;
        Salt = salt;
    }

    public static Result<Password> Create(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            return Result<Password>.Fail("PASSWORD_REQUIRED", "Password is required");
        }

        if (plainPassword.Length < 8)
        {
            return Result<Password>.Fail("PASSWORD_TOO_SHORT", "Password must be at least 8 characters");
        }

        if (plainPassword.Length > 128)
        {
            return Result<Password>.Fail("PASSWORD_TOO_LONG", "Password cannot exceed 128 characters");
        }

        // Password will be hashed in Infrastructure layer using bcrypt/PBKDF2
        // For now, we just validate the plain password
        string salt = GenerateSalt();
        string hash = HashPassword(plainPassword, salt);

        return Result<Password>.Success(new Password(hash, salt));
    }

    public static Password CreateFromHash(string hash, string salt)
    {
        return new Password(hash, salt);
    }

    public bool VerifyPassword(string plainPassword)
    {
        // Password verification will be done in Infrastructure layer
        // This is a placeholder for domain logic
        string hash = HashPassword(plainPassword, Salt);
        return hash == Hash;
    }

    private static string GenerateSalt()
    {
        // In production, this should use BCrypt.GenerateSalt() or similar
        return Guid.NewGuid().ToString("N")[..16];
    }

    private static string HashPassword(string plainPassword, string salt)
    {
        // In production, this should use BCrypt.HashPassword() or PBKDF2
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(plainPassword + salt));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool Equals(Password? other)
    {
        return other is not null && Hash == other.Hash;
    }

    public override bool Equals(object? obj)
    {
        return obj is Password password && Equals(password);
    }

    public override int GetHashCode()
    {
        return Hash.GetHashCode();
    }

    public static bool operator ==(Password? left, Password? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Password? left, Password? right)
    {
        return !(left == right);
    }
}