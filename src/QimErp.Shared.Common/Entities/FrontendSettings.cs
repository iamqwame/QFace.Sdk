namespace QimErp.Shared.Common.Entities;

public class FrontendSettings
{
    public const string SectionName = "FrontendSettings";
    
    public string BaseUrl { get; set; } = "https://app.qimerp.com";
    public string ActivationPath { get; set; } = "/auth/activate";
    public string ResetPasswordPath { get; set; } = "/auth/reset-password";
    public string LoginPath { get; set; } = "/auth/login";
}
