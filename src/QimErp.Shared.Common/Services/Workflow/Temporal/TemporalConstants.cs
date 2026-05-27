namespace QimErp.Shared.Common.Services.Workflow.Temporal;

public static class TemporalConstants
{
    /// <summary>
    /// Task queue used by the Platform Workflow Worker for:
    ///   - ApprovalWorkflow orchestration
    ///   - IWorkflowPlatformActivity (platform DB record)
    ///   - INotificationActivity (durable notifications)
    /// </summary>
    public const string TaskQueue = "qimerp-workflow-approvals";

    /// <summary>Temporal namespace — overridable via Temporal:Namespace config key.</summary>
    public const string DefaultNamespace = "qimerp";

    /// <summary>
    /// Task queue for <see cref="IApproverResolverActivity"/> — always CoreHR Employee worker.
    /// </summary>
    public const string ApproverResolverTaskQueue = "qimerp-employee-approvals";

    /// <summary>
    /// Per-module activity task queue. Each module's Temporal worker polls this queue for
    /// <see cref="IModuleApprovalActivity"/> implementations for its entity types.
    ///
    /// The module name MUST match the ApprovalWorkflowInput.Module value used by
    /// that module's entities when the workflow is triggered.
    ///
    /// CoreHR hosts all HR entity approval activities on the Employee worker queue.
    /// </summary>
    public static string ModuleTaskQueue(string module) =>
        module.ToLowerInvariant() switch
        {
            "hr" => ApproverResolverTaskQueue,
            _ => $"qimerp-{module.ToLowerInvariant()}-approvals"
        };

    /// <summary>
    /// Appends an environment task-queue suffix (e.g. "-local") when not already present.
    /// </summary>
    public static string ApplyTaskQueueSuffix(string baseQueue, string suffix)
    {
        if (string.IsNullOrEmpty(suffix))
            return baseQueue;

        return baseQueue.EndsWith(suffix, StringComparison.Ordinal)
            ? baseQueue
            : baseQueue + suffix;
    }

    /// <summary>
    /// Derives the suffix from a resolved workflow task queue (e.g. "-local").
    /// </summary>
    public static string ExtractTaskQueueSuffix(string resolvedQueue, string baseQueue)
    {
        if (resolvedQueue.StartsWith(baseQueue, StringComparison.Ordinal)
            && resolvedQueue.Length > baseQueue.Length)
        {
            return resolvedQueue[baseQueue.Length..];
        }

        return string.Empty;
    }

    /// <summary>
    /// Canonical Temporal workflow ID for an entity approval workflow.
    ///
    /// Used by ALL three signal sites — bridge (start), ApproveWorkflow (signal),
    /// RejectWorkflow (signal), BulkApproveWorkflow (signal).
    ///
    /// WARNING: Changing this format invalidates all in-flight workflow handles.
    /// Coordinate with a rolling deploy when modifying.
    /// </summary>
    public static string WorkflowId(string entityType, string entityId) =>
        $"approval-{entityType}-{entityId}";
}
