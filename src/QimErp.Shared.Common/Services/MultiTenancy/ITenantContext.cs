using QFace.Sdk.Temporal.Interceptors;

namespace QimErp.Shared.Common.Services.MultiTenancy;

public interface ITenantContext
{
    string? TenantId { get; }
    void SetTenant(string? tenantId);
}

/// <summary>
/// Implements both <see cref="ITenantContext"/> (used by EF global query filters) and
/// <see cref="ITenantScopeSetter"/> (used by <see cref="TenantContextActivityInterceptor"/>
/// to seed the ambient tenant for Temporal activity executions).
/// </summary>
public class TenantContext : ITenantContext, ITenantScopeSetter
{
    private static readonly AsyncLocal<string?> _tenantId = new();

    public string? TenantId => _tenantId.Value;

    public void SetTenant(string? tenantId)
    {
        _tenantId.Value = tenantId;
    }

    // ── ITenantScopeSetter ────────────────────────────────────────────────────
    void ITenantScopeSetter.SetTenantScope(string? tenantId) => SetTenant(tenantId);
    void ITenantScopeSetter.ClearTenantScope() => SetTenant(null);
}

public class DesignTimeTenantContext : ITenantContext
{
    public string? TenantId => null;
    
    public void SetTenant(string? tenantId) { }
}

