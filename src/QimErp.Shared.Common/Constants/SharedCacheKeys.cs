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
}
