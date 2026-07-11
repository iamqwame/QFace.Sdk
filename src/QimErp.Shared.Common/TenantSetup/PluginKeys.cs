namespace QimErp.Shared.Common.TenantSetup;

/// <summary>
/// Canonical App Store catalog item keys for plugins. Must match <c>AppStoreCatalogSeeder</c>.
/// </summary>
public static class PluginKeys
{
    public const string SsnitFiling = "ssnit-filing";
    public const string GhIpssExport = "ghipss-export";
    public const string PayslipWhatsapp = "payslip-whatsapp";
    public const string BiometricSync = "biometric-sync";
    public const string Esign = "esign";
    public const string ChatNotify = "chat-notify";
    public const string SmsNotify = "sms-notify";
    public const string ConferenceNotify = "conference-notify";
    public const string WebhookNotify = "webhook-notify";

    public static readonly string[] All =
    [
        SsnitFiling,
        GhIpssExport,
        PayslipWhatsapp,
        BiometricSync,
        Esign,
        ChatNotify,
        SmsNotify,
        ConferenceNotify,
        WebhookNotify,
    ];
}
