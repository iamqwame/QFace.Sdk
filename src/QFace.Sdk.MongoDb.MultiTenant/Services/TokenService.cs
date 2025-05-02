using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace QFace.Sdk.MongoDb.MultiTenant.Services;

/// <summary>
/// Interface for token service
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for a user in a tenant
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="tenantCode">Tenant code</param>
    /// <param name="email">User email</param>
    /// <param name="role">User role in tenant</param>
    /// <param name="permissions">User permissions in tenant</param>
    /// <returns>Token value and expiration</returns>
    (string Token, DateTime ExpiresAt) GenerateAccessToken(
        string userId,
        string tenantId,
        string tenantCode,
        string email,
        string role,
        List<string> permissions);
            
    /// <summary>
    /// Generates a refresh token
    /// </summary>
    /// <returns>Token value and expiration</returns>
    (string Token, DateTime ExpiresAt) GenerateRefreshToken();
        
    /// <summary>
    /// Validates an access token
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>Validation result with claims</returns>
    (bool IsValid, IDictionary<string, string> Claims, string? ErrorMessage) ValidateAccessToken(string token);
}

/// <summary>
/// JWT token service implementation
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly ILogger<JwtTokenService> _logger;

    /// <summary>
    /// Creates a new JWT token service
    /// </summary>
    public JwtTokenService(JwtOptions options, ILogger<JwtTokenService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates an access token
    /// </summary>
    public (string Token, DateTime ExpiresAt) GenerateAccessToken(
        string userId,
        string tenantId,
        string tenantCode,
        string email,
        string role,
        List<string> permissions)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tenant_id", tenantId),
            new Claim("tenant_code", tenantCode),
            new Claim(ClaimTypes.Role, role)
        };

        // Add permissions as claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var encodedToken = tokenHandler.WriteToken(token);

        _logger.LogDebug("Generated access token for user {UserId} in tenant {TenantId} expiring at {ExpiresAt}",
            userId, tenantId, expiresAt);

        return (encodedToken, expiresAt);
    }

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    public (string Token, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var refreshToken = Convert.ToBase64String(randomBytes);
        var expiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenLifetimeDays);

        return (refreshToken, expiresAt);
    }

    /// <summary>
    /// Validates an access token
    /// </summary>
    public (bool IsValid, IDictionary<string, string> Claims, string? ErrorMessage) ValidateAccessToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            var claimsDictionary = principal.Claims.ToDictionary(
                c => c.Type,
                c => c.Value,
                StringComparer.OrdinalIgnoreCase);

            return (true, claimsDictionary, null);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogInformation("Token validation failed: Token expired");
            return (false, new Dictionary<string, string>(), "Token has expired");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogWarning("Token validation failed: Invalid token signature");
            return (false, new Dictionary<string, string>(), "Invalid token signature");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return (false, new Dictionary<string, string>(), "Invalid token");
        }
    }
}



// <summary>