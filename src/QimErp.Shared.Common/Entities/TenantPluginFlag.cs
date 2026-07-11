namespace QimErp.Shared.Common.Entities;

/// <summary>
/// One row per (tenant, plugin) — tracks whether an App Store plugin's capability is currently
/// enabled inside the owning module. Lives on the base <c>ApplicationDbContext</c> (same as
/// <see cref="AppSetting"/>) so every module gets it for free instead of each owning module
/// (Payroll, Leave, CoreHr, Workflow, ...) rolling its own copy. Toggled by that module's own
/// idempotent Enable/Disable Temporal activity, dispatched from IAM.Core's App Store install
/// and uninstall workflows.
/// </summary>
public sealed class TenantPluginFlag : GuidAuditableEntity
{
    public string PluginKey { get; private set; } = string.Empty;
    public bool Enabled { get; private set; }
    public DateTime? EnabledAt { get; private set; }
    public DateTime? DisabledAt { get; private set; }

    private TenantPluginFlag() { }

    public static TenantPluginFlag Create(string tenantId, string pluginKey)
    {
        var flag = new TenantPluginFlag
        {
            Id = CreateId(),
            PluginKey = pluginKey,
        };

        flag.WithTenantId(tenantId);
        flag.AsActive();
        return flag;
    }

    public void Enable(DateTime enabledAt)
    {
        Enabled = true;
        EnabledAt = enabledAt;
        DisabledAt = null;
    }

    public void Disable(DateTime disabledAt)
    {
        Enabled = false;
        DisabledAt = disabledAt;
    }
}
