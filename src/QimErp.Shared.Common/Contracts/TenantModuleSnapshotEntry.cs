namespace QimErp.Shared.Common.Contracts;

/// <summary>
/// Cross-service Redis snapshot of a tenant's installed module keys.
/// Written by IAM to <see cref="Constants.SharedCacheKeys.TenantModuleSnapshot(string)"/>.
/// </summary>
public sealed record TenantModuleSnapshotEntry(
    int Revision,
    IReadOnlyList<string> InstalledModuleKeys);
