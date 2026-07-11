namespace QimErp.Shared.Common.TenantSetup;

/// <summary>
/// Modules and platform surfaces always provisioned for every tenant at signup.
/// </summary>
public static class BaseModel
{
    public static readonly string[] IncludedModuleKeys =
    [
        ModuleKeys.CoreHR,
        ModuleKeys.Leave,
    ];

    public static readonly string[] AppStoreItemKeys =
    [
        "core-hr",
        "leave",
    ];

    /// <summary>Non-billable surfaces always available when authenticated.</summary>
    public static readonly string[] PlatformCapabilities =
    [
        "iam",
        "app-store",
        "platform",
    ];
}
