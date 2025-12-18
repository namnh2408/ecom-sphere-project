namespace Users.Domain.Services;

/// <summary>
/// Domain service for JWT token generation
/// Implementation should be in Infrastructure layer
/// </summary>
public interface ITokenGenerationService
{
    /// <summary>
    /// Generates a JWT token for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="email">The user email</param>
    /// <param name="roles">The user's role IDs</param>
    /// <param name="expiresIn">Token expiration time in minutes</param>
    /// <returns>The generated JWT token</returns>
    string GenerateToken(Guid userId, string email, IEnumerable<Guid> roles, int expiresIn = 60);

    /// <summary>
    /// Validates and extracts claims from a JWT token
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>Dictionary of claims if valid; null if invalid</returns>
    Dictionary<string, string>? ValidateToken(string token);

    /// <summary>
    /// Extracts claims from a token without validating expiration
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns>Dictionary of claims if token is valid; null if invalid</returns>
    Dictionary<string, string>? ExtractClaimsWithoutValidation(string token);
}