namespace QimErp.Shared.Common.Activities;

/// <summary>
/// One shared formatter for the deterministic workflow ids used when handing an entity
/// change off to a sync actor (e.g. <c>EmployeeSyncActor</c>). Replaces the same
/// hand-rolled <c>$"{operation}-sync-{id:N}-{timestamp}"</c> interpolation that used to be
/// copy-pasted at every call site.
/// </summary>
public static class EntitySyncWorkflowId
{
    public static string For(string operation, Guid entityId) =>
        $"{operation}-sync-{entityId:N}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
}
