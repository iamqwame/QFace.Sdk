namespace QFace.Sdk.MongoDb.MultiTenant.Options;

/// <summary>
/// JWT options
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Secret key for signing tokens
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token lifetime in minutes
    /// </summary>
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token lifetime in days
    /// </summary>
    public int RefreshTokenLifetimeDays { get; set; } = 7;
}