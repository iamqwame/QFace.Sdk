using QimErp.Shared.Common.Services.Workflow;

namespace QimErp.Shared.Common.Workflow.Extensions;

public static class WorkflowEnabledExtensions
{
    public static void InitiateWorkflow(this IWorkflowEnabled entity, string initiatedByEmail, string? initiatedByEmployeeId = null, string? initiatedByName = null, string? comments = null)
    {
        entity.WorkflowStatus = WorkflowStatus.InProgress;
        entity.WorkflowInitiatedAt = DateTime.UtcNow;
        entity.WorkflowInitiatedByEmail = initiatedByEmail;
        entity.WorkflowInitiatedByEmployeeId = initiatedByEmployeeId;
        entity.WorkflowInitiatedByName = initiatedByName;
        entity.WorkflowComments = comments;
    }
    
    public static void CompleteWorkflow(this IWorkflowEnabled entity, WorkflowStatus finalStatus, string completedByEmail, string? completedByEmployeeId = null, string? completedByName = null, string? comments = null)
    {
        entity.WorkflowStatus = finalStatus;
        entity.WorkflowCompletedAt = DateTime.UtcNow;
        entity.WorkflowCompletedByEmail = completedByEmail;
        entity.WorkflowCompletedByEmployeeId = completedByEmployeeId;
        entity.WorkflowCompletedByName = completedByName;
        if (!comments.IsEmpty())
            entity.WorkflowComments = comments;
        
        if (finalStatus == WorkflowStatus.Rejected)
            entity.WorkflowRejectionReason = comments;
    }

    public static void UpdateWorkflowState(this IWorkflowEnabled entity, string newState, string? comments = null)
    {
        entity.CurrentWorkflowState = newState;
        if (!comments.IsEmpty())
            entity.WorkflowComments = comments;
    }
    
    public static void RejectWorkflow(this IWorkflowEnabled entity, string rejectedByEmail, string? rejectedByEmployeeId = null, string? rejectedByName = null, string reason = "")
    {
        entity.WorkflowStatus = WorkflowStatus.Rejected;
        entity.WorkflowRejectionReason = reason;
        entity.WorkflowCompletedAt = DateTime.UtcNow;
        entity.WorkflowCompletedByEmail = rejectedByEmail;
        entity.WorkflowCompletedByEmployeeId = rejectedByEmployeeId;
        entity.WorkflowCompletedByName = rejectedByName;
        entity.WorkflowComments = reason;
    }
    
    public static void ApproveWorkflow(this IWorkflowEnabled entity, string approvedByEmail, string? approvedByEmployeeId = null, string? approvedByName = null, string? comments = null)
    {
        entity.WorkflowStatus = WorkflowStatus.Approved;
        entity.WorkflowCompletedAt = DateTime.UtcNow;
        entity.WorkflowCompletedByEmail = approvedByEmail;
        entity.WorkflowCompletedByEmployeeId = approvedByEmployeeId;
        entity.WorkflowCompletedByName = approvedByName;
        if (!comments.IsEmpty())
            entity.WorkflowComments = comments;
    }
    
    public static bool CanBeEdited(this IWorkflowEnabled entity)
    {
        return entity.WorkflowStatus == WorkflowStatus.NotStarted || 
               entity.WorkflowStatus == WorkflowStatus.Rejected;
    }
    
    public static bool CanBeDeleted(this IWorkflowEnabled entity)
    {
        return entity.WorkflowStatus != WorkflowStatus.Approved;
    }
    
    public static bool RequiresApproval(this IWorkflowEnabled entity)
    {
        return entity.WorkflowStatus == WorkflowStatus.InProgress;
    }
}