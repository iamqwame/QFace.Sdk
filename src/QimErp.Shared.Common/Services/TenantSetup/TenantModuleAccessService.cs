using Microsoft.AspNetCore.Http;
using QimErp.Shared.Common.Constants;
using QimErp.Shared.Common.Contracts;
using QimErp.Shared.Common.Services.Cache;
using QimErp.Shared.Common.TenantSetup;

namespace QimErp.Shared.Common.Services.TenantSetup;

public sealed class TenantModuleAccessService(
    IDistributedCacheService cache,
    IHttpContextAccessor httpContextAccessor) : ITenantModuleAccessService
{
    private static string SnapshotItemKey(Guid tenantId) => $"tenant-module-snapshot:{tenantId}";
    public async Task<bool> IsModuleEnabledAsync(
        string? tenantId,
        string moduleKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return false;
        }

        // An empty moduleKey means "no module required", not "unknown tenant".
        if (string.IsNullOrWhiteSpace(moduleKey))
        {
            return true;
        }

        if (BaseModel.IncludedModuleKeys.Contains(moduleKey, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        var installed = await GetInstalledModuleKeysAsync(tenantId, cancellationToken);
        if (installed is null)
        {
            return false;
        }

        return ModuleGuard.IsExplicitlySelected(installed, moduleKey);
    }

    public async Task<IReadOnlyList<string>?> GetInstalledModuleKeysAsync(
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
            && cached is TenantModuleSnapshotEntry cachedSnapshot)
        {
            return cachedSnapshot.InstalledModuleKeys;
        }

        var snapshot = await cache.GetAsync<TenantModuleSnapshotEntry>(
            SharedCacheKeys.TenantModuleSnapshot(tenantGuid));

        if (httpContextAccessor.HttpContext is not null)
        {
            httpContextAccessor.HttpContext.Items[itemKey] = snapshot;
        }

        return snapshot?.InstalledModuleKeys;
    }
}
