namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Process-local flag that signals "this SaveChanges is the workflow engine writing
/// entity workflow state" (approval/rejection processors). Read by
/// <see cref="QimErp.Shared.Common.Interceptors.AuditEntitySaveChangesInterceptor"/>
/// to skip workflow initiation and edit/delete validation for that save.
///
/// Without this, finalizing an approval (WorkflowStatus -> Approved) is itself seen
/// as a user UPDATE of a workflow-enabled entity and immediately re-initiates a
/// workflow, clobbering the final status back to InProgress — and advancing a step
/// (saving an entity that is legitimately InProgress) would be rejected by the
/// edit-blocked validation.
/// </summary>
public static class WorkflowEngineScope
{
    private static readonly AsyncLocal<bool> Active = new();

    public static bool IsActive => Active.Value;

    /// <summary>
    /// Enter an engine-write scope on the current async flow. Dispose to restore
    /// the previous value (re-entrant safe).
    /// </summary>
    public static IDisposable Enter()
    {
        var previous = Active.Value;
        Active.Value = true;
        return new Restorer(previous);
    }

    private sealed class Restorer(bool previous) : IDisposable
    {
        public void Dispose() => Active.Value = previous;
    }
}
