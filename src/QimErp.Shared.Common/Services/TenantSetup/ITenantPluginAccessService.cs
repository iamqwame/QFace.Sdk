namespace QimErp.Shared.Common.Services.TenantSetup;

public interface ITenantPluginAccessService
{
    Task<bool> IsPluginEnabledAsync(
        string? tenantId,
        string pluginKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>?> GetInstalledPluginKeysAsync(
        string? tenantId,
        CancellationToken cancellationToken = default);
}
