namespace QimErp.Shared.Common.Services.Notifications;

/// <summary>
/// Resolves avatar URLs for email templates. Callers must always supply a non-empty URL —
/// never omit <c>AvatarUrl</c> when the template HTML renders a profile image.
/// </summary>
public static class EmailAvatarTokens
{
    public static string ResolveAvatarUrl(string? profilePictureUrl, string platformDefaultAvatarUrl)
    {
        if (!string.IsNullOrWhiteSpace(profilePictureUrl))
            return profilePictureUrl;

        if (string.IsNullOrWhiteSpace(platformDefaultAvatarUrl))
            throw new ArgumentException("Platform default avatar URL must not be empty.", nameof(platformDefaultAvatarUrl));

        return platformDefaultAvatarUrl;
    }

    /// <summary>
    /// Reads <c>EmailDefaults:PlatformDefaultAvatarUrl</c>, falling back to
    /// <c>{FrontendSettings:BaseUrl}/static/avatar-placeholder.png</c>.
    /// </summary>
    public static string ResolvePlatformDefault(IConfiguration configuration)
    {
        var configured = configuration["EmailDefaults:PlatformDefaultAvatarUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        var portal = configuration["FrontendSettings:BaseUrl"]?.TrimEnd('/')
                     ?? "https://app.qimerp.com";
        return $"{portal}/static/avatar-placeholder.png";
    }
}
