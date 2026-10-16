namespace QimErp.Shared.Common.Sync;

public static class SyncSourceVersionGuard
{
    /// <summary>
    /// Returns true when the snapshot should be applied.
    /// <paramref name="eventVersion"/> &lt;= 0 means the publisher did not supply a version (legacy).
    /// </summary>
    public static bool ShouldApplySnapshot(long currentSourceVersion, long eventVersion)
        => eventVersion <= 0 || eventVersion > currentSourceVersion;
}
