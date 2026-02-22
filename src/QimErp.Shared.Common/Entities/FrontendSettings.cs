namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Frontend application URL and path configuration.
/// Bind from "FrontendSettings" section. Environment variables: FrontendSettings__BaseUrl, FrontendSettings__ActivationPath, etc.
/// </summary>
public class FrontendSettings
{
    public const string SectionName = "FrontendSettings";

    /// <summary>Base URL of the frontend application (e.g. https://app.qimerp.com).</summary>
    public string BaseUrl { get; set; } = "https://app.qimerp.com";

    /// <summary>Path for user activation links (e.g. /auth/activate).</summary>
    public string ActivationPath { get; set; } = "/auth/activate";

    /// <summary>Path for password reset links (e.g. /auth/reset-password).</summary>
    public string ResetPasswordPath { get; set; } = "/auth/reset-password";

    /// <summary>Path for login (e.g. /auth/login).</summary>
    public string LoginPath { get; set; } = "/auth/login";
}
