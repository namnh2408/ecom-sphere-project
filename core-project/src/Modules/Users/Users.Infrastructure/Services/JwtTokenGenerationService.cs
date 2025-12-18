using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Users.Domain.Services;

namespace Users.Infrastructure.Services;

public class JwtTokenGenerationService : ITokenGenerationService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenGenerationService(string secretKey, string issuer = "UserService", string audience = "UserServiceAPI")
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateToken(Guid userId, string email, IEnumerable<Guid> roles, int expiresIn = 60)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Dictionary<string, string>? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var claims = principal?.Claims.ToDictionary(c => c.Type, c => c.Value);
            return claims;
        }
        catch
        {
            return null;
        }
    }

    public Dictionary<string, string>? ExtractClaimsWithoutValidation(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            
            if (jwtToken == null)
                return null;

            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
            return claims;
        }
        catch
        {
            return null;
        }
    }
}