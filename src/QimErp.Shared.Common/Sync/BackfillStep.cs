namespace QimErp.Shared.Common.Sync;

/// <summary>
/// Describes one install-time backfill step registered in <see cref="ModuleSyncRegistry"/>.
/// Each backfill activity should return an <c>InstallSyncStepResult</c> shape
/// (see <c>QimErp.IAM.Core.Shared.Contracts.AppStore.InstallSyncStepResult</c>)
/// so IAM can persist a per-entity breakdown on <c>TenantAppStoreInstall.SyncReport</c>.
/// </summary>
public sealed record BackfillStep(
    SyncType SyncType,
    string StepName,
    string TaskQueue,
    string[] EntityKinds);
