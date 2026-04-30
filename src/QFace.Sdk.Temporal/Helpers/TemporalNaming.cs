namespace QFace.Sdk.Temporal.Helpers;

/// <summary>
/// Generic naming helpers for Temporal workflow IDs and task queue names.
/// No QimErp domain types. Domain-specific constants (TemporalConstants in
/// QimErp.Shared.Common) can call these or use them as a reference implementation.
///
/// Using shared helpers for IDs prevents silent mismatches between the app that
/// starts a workflow and the app that signals or queries it — a mismatch of even
/// one character results in the signal going to a non-existent workflow.
/// </summary>
public static class TemporalNaming
{
    /// <summary>
    /// Builds a stable workflow ID from a category, entity type, and entity ID.
    /// Format: "{category}-{entityType}-{entityId}"
    /// Example: WorkflowId("approval", "Employee", "3f2a1b...") → "approval-Employee-3f2a1b..."
    ///
    /// Use consistently in: IWorkflowStarter, IWorkflowSignaller, IWorkflowQueryClient,
    /// IWorkflowTerminator — any place that needs the same ID for the same workflow instance.
    /// </summary>
    public static string WorkflowId(string category, string entityType, string entityId)
        => $"{category.ToLowerInvariant()}-{entityType}-{entityId}";

    /// <summary>
    /// Builds a task queue name from a prefix and module/purpose.
    /// Format: "{prefix}-{module}"
    /// Example: TaskQueue("qimerp", "workflow-approvals") → "qimerp-workflow-approvals"
    /// </summary>
    public static string TaskQueue(string prefix, string module)
        => $"{prefix.ToLowerInvariant()}-{module.ToLowerInvariant()}";

    /// <summary>
    /// Extracts the host portion from a Temporal address for use as the TLS SNI domain.
    /// "acct.tmprl.cloud:7233" → "acct.tmprl.cloud"
    /// "localhost:7233"        → "localhost"
    /// </summary>
    public static string ExtractHost(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return address;
        var colonIndex = address.LastIndexOf(':');
        return colonIndex > 0 ? address[..colonIndex] : address;
    }

    /// <summary>
    /// Returns true when the address points to a local Temporal instance.
    /// Used by AddTemporalClient to auto-skip TLS for local dev without config.
    /// </summary>
    public static bool IsLocalAddress(string address)
    {
        var host = ExtractHost(address).ToLowerInvariant();
        return host is "localhost" or "127.0.0.1" or "::1" or "0.0.0.0";
    }

    /// <summary>
    /// Workflow-side helper for deriving the active <c>Temporal:TaskQueueSuffix</c>
    /// from the queue this workflow itself is running on. Use inside a Temporal
    /// workflow when constructing the queue name for a child workflow or
    /// <c>ActivityOptions.TaskQueue</c> — the suffix flows from the workflow's
    /// own queue, so child dispatches stay within the same environment.
    ///
    /// <para>Workflows cannot read <c>IConfiguration</c> deterministically, so
    /// this is the workflow-side equivalent of <c>TemporalOptions.WithTaskQueueSuffix</c>.</para>
    ///
    /// <para>Example, inside a workflow whose own queue is e.g. <c>"qimerp-iam-tenant-onboarding-local"</c>:</para>
    /// <code>
    /// var childQueue = TemporalNaming.SuffixedFromCurrentQueue(
    ///     baseQueue: "qimerp-corehr-employee-tenant-setup",
    ///     currentQueue: Workflow.Info.TaskQueue,
    ///     currentQueueBase: "qimerp-iam-tenant-onboarding");
    /// // → "qimerp-corehr-employee-tenant-setup-local"
    /// </code>
    /// </summary>
    /// <param name="baseQueue">The unsuffixed target queue name.</param>
    /// <param name="currentQueue">The queue this workflow is running on (e.g. <c>Workflow.Info.TaskQueue</c>).</param>
    /// <param name="currentQueueBase">The unsuffixed name of the queue this workflow registers on.</param>
    /// <returns><paramref name="baseQueue"/> with the suffix derived from <paramref name="currentQueue"/> appended.</returns>
    public static string SuffixedFromCurrentQueue(string baseQueue, string currentQueue, string currentQueueBase)
    {
        if (string.IsNullOrEmpty(currentQueue) || string.IsNullOrEmpty(currentQueueBase))
            return baseQueue;
        if (!currentQueue.StartsWith(currentQueueBase, StringComparison.Ordinal))
            return baseQueue;
        var suffix = currentQueue[currentQueueBase.Length..];
        return string.IsNullOrEmpty(suffix) ? baseQueue : baseQueue + suffix;
    }
}
