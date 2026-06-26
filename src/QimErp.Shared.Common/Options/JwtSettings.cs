namespace QimErp.Shared.Common.Options;

public class JwtSettings
{
    public required string Secret { get; set; }
    public int ExpirationMinutes { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}