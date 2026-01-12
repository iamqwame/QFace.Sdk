namespace QimErp.Shared.Common.Services.MultiTenancy;

public interface ITenantContext
{
    string? TenantId { get; }
    void SetTenant(string? tenantId);
}

public class TenantContext : ITenantContext
{
    private static readonly AsyncLocal<string?> _tenantId = new();

    public string? TenantId => _tenantId.Value;

    public void SetTenant(string? tenantId)
    {
        _tenantId.Value = tenantId;
    }
}

public class DesignTimeTenantContext : ITenantContext
{
    public string? TenantId => null;
    
    public void SetTenant(string? tenantId) { }
}

