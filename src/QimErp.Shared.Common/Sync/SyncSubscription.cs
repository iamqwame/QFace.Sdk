namespace QimErp.Shared.Common.Sync;

public sealed record SyncSubscription(
    SyncType Type,
    string? ModuleKey,
    string TaskQueue,
    string ActivitySuffix,
    bool RequiresModuleSelection = true);
