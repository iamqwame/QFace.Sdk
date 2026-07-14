namespace QimErp.Shared.Common.Sync;

/// <summary>Parent Temporal task queue bases for module-scoped sync workflows.</summary>
public static class ModuleSyncWorkflowQueues
{
    public const string EmployeeSyncParent = "qimerp-corehr-employee-sync";
    public const string GlReferenceDataSyncParent = "qimerp-accounting-gl-reference-sync";
    public const string AssignmentChangedParent = "qimerp-corehr-assignment-changed";
    public const string JournalEntryPostedParent = "qimerp-accounting-journal-entry-posted-sync";
    public const string TenantReferenceSyncParent = "qimerp-iam-tenant-reference-sync";

    public static string Suffix(string parentQueueBase, string? currentQueue) =>
        currentQueue?.StartsWith(parentQueueBase, StringComparison.Ordinal) == true
            ? currentQueue[parentQueueBase.Length..]
            : "";
}
