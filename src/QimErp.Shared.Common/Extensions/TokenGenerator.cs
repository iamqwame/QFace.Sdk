using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;


namespace QimErp.Shared.Common.Extensions;

public static class TokenGenerator
{
    public static string GenerateToken(string issuer, string audience, IEnumerable<Claim> claims, string signingKey, TimeSpan expiration)
    {
        if (signingKey.IsEmpty() || signingKey.Length < 32)
        {
            throw new ArgumentException("Signing key must be at least 32 characters long.", nameof(signingKey));
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            Expires = now.Add(expiration),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}