namespace QimErp.Shared.Common.Constants;

/// <summary>
/// Redis cache key generators that are shared across multiple microservices.
/// Each service owns its own module-level cache constants (e.g. IamCacheConstants),
/// but keys that are READ by services other than the one that WRITES them belong here
/// so both sides stay in sync without a cross-module reference.
/// </summary>
public static class SharedCacheKeys
{
    private const string Prefix = "qface:qimerp:";

    /// <summary>
    /// Tenant branding snapshot — written by IAM on every tenant mutation, read by
    /// Payroll (payslip PDF), CoreHR, and any other service that needs the company
    /// name, email, and logo without an HTTP call back to IAM.
    ///
    /// TTL: 1 year (effectively permanent); invalidated immediately on tenant update.
    /// Key: qface:qimerp:{tenantId}:shared:tenant_branding
    /// </summary>
    public static string TenantBranding(string tenantId)
        => $"{Prefix}{tenantId}:shared:tenant_branding";

    public static string TenantBranding(Guid tenantId)
        => TenantBranding(tenantId.ToString());

    // TTL: 1 year — RequiredModuleMiddleware treats a missing entry as "module not installed", so a short expiry here 403s every gated request platform-wide until IAM happens to rebuild it. Written only by IAM on install/uninstall/onboarding refresh; do not shorten.
    public static string TenantModuleSnapshot(string tenantId)
        => $"{Prefix}{tenantId}:shared:tenant_module_snapshot";

    public static string TenantModuleSnapshot(Guid tenantId)
        => TenantModuleSnapshot(tenantId.ToString());

    // Same 1-year TTL reasoning as TenantModuleSnapshot. Read via ITenantPluginAccessService.
    public static string TenantPluginSnapshot(string tenantId)
        => $"{Prefix}{tenantId}:shared:tenant_plugin_snapshot";

    public static string TenantPluginSnapshot(Guid tenantId)
        => TenantPluginSnapshot(tenantId.ToString());

    // Written once by IAM at startup (PlatformAIConfigSeedService), re-written on every IAM
    // restart/redeploy. TTL: 1 year. Read by any service's CachedAIOptionsProvider that has
    // no local AI:* config of its own.
    public static string PlatformAIProviderConfig()
        => $"{Prefix}shared:platform_ai_provider_config";

    // Written by IAM's tenant AI-provider settings endpoint (TenantAIProviderSettingsService)
    // on every save, deleted on reset-to-default. TTL: 1 year (write-through on save means
    // staleness is a non-issue). Checked by CachedAIOptionsProvider before the local-config
    // and PlatformAIProviderConfig fallbacks — a tenant override always wins.
    public static string TenantAIProviderConfig(string tenantId)
        => $"{Prefix}{tenantId}:shared:tenant_ai_provider_config";

    public static string TenantAIProviderConfig(Guid tenantId)
        => TenantAIProviderConfig(tenantId.ToString());
}
