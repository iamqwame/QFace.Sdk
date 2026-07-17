namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Frontend application URL and path configuration.
/// Bind from "FrontendSettings" section. Environment variables: FrontendSettings__BaseUrl, FrontendSettings__ActivationPath, etc.
/// No hardcoded fallback domain — every environment (including local dev) must set
/// FrontendSettings:BaseUrl explicitly, or links render with an empty/relative base
/// instead of silently pointing at production.
/// </summary>
public class FrontendSettings
{
    public const string SectionName = "FrontendSettings";

    /// <summary>Base URL of the frontend application. Must be set per environment.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Path for user activation links (e.g. /auth/activate).</summary>
    public string ActivationPath { get; set; } = "/auth/activate";

    /// <summary>Path for password reset links (e.g. /auth/reset-password).</summary>
    public string ResetPasswordPath { get; set; } = "/auth/reset-password";

    /// <summary>Path for login (e.g. /auth/login).</summary>
    public string LoginPath { get; set; } = "/auth/login";
}
