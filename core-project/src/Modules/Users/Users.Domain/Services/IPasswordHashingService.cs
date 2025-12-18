namespace Users.Domain.Services;

/// <summary>
/// Domain service for password hashing and verification
/// Implementation should be in Infrastructure layer
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Hashes a plain password with a salt
    /// </summary>
    string HashPassword(string plainPassword, string salt);

    /// <summary>
    /// Verifies a plain password against a hash
    /// </summary>
    bool VerifyPassword(string plainPassword, string hash, string salt);

    /// <summary>
    /// Generates a new salt for password hashing
    /// </summary>
    string GenerateSalt();
}