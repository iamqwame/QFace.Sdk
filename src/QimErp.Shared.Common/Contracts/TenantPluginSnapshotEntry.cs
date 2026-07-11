namespace QimErp.Shared.Common.Contracts;

/// <summary>
/// Cross-service Redis snapshot of a tenant's installed plugin item keys.
/// Written by IAM to <see cref="Constants.SharedCacheKeys.TenantPluginSnapshot(string)"/>.
/// </summary>
public sealed record TenantPluginSnapshotEntry(
    int Revision,
    IReadOnlyList<string> InstalledPluginKeys);
