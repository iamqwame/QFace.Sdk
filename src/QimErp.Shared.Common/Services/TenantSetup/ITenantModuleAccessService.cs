namespace QimErp.Shared.Common.Services.TenantSetup;

public interface ITenantModuleAccessService
{
    Task<bool> IsModuleEnabledAsync(string? tenantId, string moduleKey, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>?> GetInstalledModuleKeysAsync(
        string? tenantId,
        CancellationToken cancellationToken = default);
}
