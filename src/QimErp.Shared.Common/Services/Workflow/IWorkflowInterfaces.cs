namespace QimErp.Shared.Common.Services.Workflow;

public interface IWorkflowEnabled
{
    WorkflowStatus WorkflowStatus { get; set; }
    Guid? CurrentWorkflowHistoryId { get; set; }
    string? CurrentWorkflowState { get; set; }
    string? WorkflowCode { get; set; }
    string? WorkflowComments { get; set; }
    DateTime? WorkflowInitiatedAt { get; set; }
    WorkflowDefinition WorkflowDefinition { get; set; }
    
    // Workflow Initiation Details
    string? WorkflowInitiatedByEmail { get; set; }
    string? WorkflowInitiatedByEmployeeId { get; set; }
    string? WorkflowInitiatedByName { get; set; }
    
    // Workflow Completion Details
    string? WorkflowCompletedByEmail { get; set; }
    string? WorkflowCompletedByEmployeeId { get; set; }
    string? WorkflowCompletedByName { get; set; }
    
    // Workflow Rejection Details
    string? WorkflowRejectionReason { get; set; }
    DateTime? WorkflowCompletedAt { get; set; }

    string EntityType { get; }
    bool IsWorkflowEnabled { get; }
    bool IsActive { get; }
    bool IsPendingApproval { get; }
    bool IsRejected { get; }
    bool IsWorkflowComplete { get; }
}

public interface IWorkflowService
{
    Task<bool> ShouldTriggerWorkflow(IWorkflowEnabled entity, string operation, string? module = null);
    Task InitiateWorkflowAsync(IWorkflowEnabled entity, string operation, WorkflowDefinition? workflowDefinition = null);
    Task UpdateWorkflowStateAsync(Guid workflowHistoryId, string newState, string? comments = null);
    Task CompleteWorkflowAsync(Guid workflowHistoryId, WorkflowStatus finalStatus, string completedByEmail, string? completedByEmployeeId = null, string? completedByName = null, string? comments = null);
}


