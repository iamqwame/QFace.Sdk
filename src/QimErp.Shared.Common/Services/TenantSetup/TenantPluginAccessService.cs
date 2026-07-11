using Microsoft.AspNetCore.Http;
using QimErp.Shared.Common.Constants;
using QimErp.Shared.Common.Contracts;
using QimErp.Shared.Common.Services.Cache;
using QimErp.Shared.Common.TenantSetup;

namespace QimErp.Shared.Common.Services.TenantSetup;

public sealed class TenantPluginAccessService(
    IDistributedCacheService cache,
    IHttpContextAccessor httpContextAccessor) : ITenantPluginAccessService
{
    private static string SnapshotItemKey(Guid tenantId) => $"tenant-plugin-snapshot:{tenantId}";

    public async Task<bool> IsPluginEnabledAsync(
        string? tenantId,
        string pluginKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(pluginKey))
        {
            return false;
        }

        var installed = await GetInstalledPluginKeysAsync(tenantId, cancellationToken);
        if (installed is null)
        {
            return false;
        }

        return PluginAccessGuard.IsInstalled(installed, pluginKey);
    }

    public async Task<IReadOnlyList<string>?> GetInstalledPluginKeysAsync(
        string? tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return null;
        }

        if (!Guid.TryParse(tenantId, out var tenantGuid))
        {
            return null;
        }

        var itemKey = SnapshotItemKey(tenantGuid);
        if (httpContextAccessor.HttpContext?.Items.TryGetValue(itemKey, out var cached) == true
            && cached is TenantPluginSnapshotEntry cachedSnapshot)
        {
            return cachedSnapshot.InstalledPluginKeys;
        }

        var snapshot = await cache.GetAsync<TenantPluginSnapshotEntry>(
            SharedCacheKeys.TenantPluginSnapshot(tenantGuid));

        if (httpContextAccessor.HttpContext is not null)
        {
            httpContextAccessor.HttpContext.Items[itemKey] = snapshot;
        }

        return snapshot?.InstalledPluginKeys;
    }
}
