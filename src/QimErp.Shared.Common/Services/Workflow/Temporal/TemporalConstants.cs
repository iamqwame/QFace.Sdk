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
    /// Per-module activity task queue. Each module's Temporal worker polls this queue for
    /// <see cref="IModuleApprovalActivity"/> implementations for its entity types.
    ///
    /// The module name MUST match the ApprovalWorkflowInput.Module value used by
    /// that module's entities when the workflow is triggered.
    ///
    /// Example: "Payroll" → "qimerp-payroll-approvals"
    /// </summary>
    public static string ModuleTaskQueue(string module) =>
        $"qimerp-{module.ToLowerInvariant()}-approvals";

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
