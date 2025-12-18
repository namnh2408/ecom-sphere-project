using Users.Domain.Services;

namespace Users.Infrastructure.Services;

public class BcryptPasswordHashingService : IPasswordHashingService
{
    public string HashPassword(string plainPassword, string salt)
    {
        // Use BCrypt for production
        // For now using simple implementation - replace with BCrypt.Net-Next in production
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    public bool VerifyPassword(string plainPassword, string hash, string salt)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, hash);
    }

    public string GenerateSalt()
    {
        // BCrypt generates salt internally
        return BCrypt.Net.BCrypt.GenerateSalt();
    }
}