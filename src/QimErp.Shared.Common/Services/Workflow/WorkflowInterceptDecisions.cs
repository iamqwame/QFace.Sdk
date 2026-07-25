namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Shared intercept-time decisions for update/delete workflow initiation.
/// </summary>
public static class WorkflowInterceptDecisions
{
    public static bool ShouldInitiateWorkflowOnUpdate(IWorkflowEnabled entity) =>
        entity.WorkflowStatus is WorkflowStatus.NotStarted
            or WorkflowStatus.Rejected
            or WorkflowStatus.Approved
            or WorkflowStatus.Returned;

    public static bool IsUpdateWorkflowBlockedByStatus(IWorkflowEnabled entity) =>
        entity.WorkflowStatus == WorkflowStatus.InProgress;

    public static bool ShouldStartCreateWorkflow(EntityWorkflowConfig config, IWorkflowEnabled entity) =>
        config.EnableWorkflowForCreate
        && !string.IsNullOrWhiteSpace(config.CreateWorkflowCode)
        && WorkflowTriggerConditionEvaluator.EvaluateAll(entity, config.CreateTriggerConditions);
}
