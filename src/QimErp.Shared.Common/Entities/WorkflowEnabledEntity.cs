using QimErp.Shared.Common.Services.Workflow;

namespace QimErp.Shared.Common.Entities;

public abstract class WorkflowEnabledEntity : AuditableEntity,IWorkflowEnabled
{
    public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.NotStarted;
    public Guid? CurrentWorkflowHistoryId { get; set; }
    public Guid? CurrentWorkflowInstanceId { get; set; }
    public string? CurrentWorkflowState { get; set; }
    public string? WorkflowCode { get; set; }
    public string? WorkflowComments { get; set; }
    public DateTime? WorkflowInitiatedAt { get; set; }
    // Workflow Initiation Details
    public string? WorkflowInitiatedByEmail { get; set; }
    public string? WorkflowInitiatedByEmployeeId { get; set; }
    public string? WorkflowInitiatedByName { get; set; }
    
    // Workflow Completion Details
    public string? WorkflowCompletedByEmail { get; set; }
    public string? WorkflowCompletedByEmployeeId { get; set; }
    public string? WorkflowCompletedByName { get; set; }
    
    // Workflow Rejection Details
    public string? WorkflowRejectionReason { get; set; }
    public DateTime? WorkflowCompletedAt { get; set; }

    public WorkflowDefinition WorkflowDefinition { get; set; } = new();

    public virtual string EntityType => GetType().Name;
    public virtual bool IsWorkflowEnabled => !_isWorkflowDisabledForSeeding;
    
    // Temporary flag to disable workflow during seeding
    private bool _isWorkflowDisabledForSeeding = false;
    
    /// <summary>
    /// Temporarily disables workflow for seeding purposes
    /// </summary>
    public void DisableWorkflowForSeeding()
    {
        _isWorkflowDisabledForSeeding = true;
        WorkflowStatus = WorkflowStatus.NotStarted;
    }
    
    /// <summary>
    /// Re-enables workflow after seeding with proper finalization
    /// </summary>
    public void EnableWorkflowAfterSeeding()
    {
        // First disable the seeding flag to restore normal workflow behavior
        _isWorkflowDisabledForSeeding = false;
        
        // Then set the final approved state
        WorkflowStatus = WorkflowStatus.Approved;
        WorkflowCompletedAt = DateTime.UtcNow;
        WorkflowCompletedByEmail = "system@qimerp.com";
        WorkflowCompletedByEmployeeId = "system";
        WorkflowCompletedByName = "System";
        WorkflowComments = "Auto-approved during seeding";
    }
    
    /// <summary>
    /// Keep seeding mode enabled but update workflow status for intermediate operations
    /// </summary>
    public void UpdateWorkflowStatusDuringSeeding(WorkflowStatus status)
    {
        // Keep seeding flag enabled but allow status updates
        WorkflowStatus = status;
        if (status == WorkflowStatus.Approved)
        {
            WorkflowCompletedAt = DateTime.UtcNow;
            WorkflowCompletedByEmail = "system@qimerp.com";
            WorkflowCompletedByEmployeeId = "system";
            WorkflowCompletedByName = "System";
            WorkflowComments = "Intermediate approval during seeding";
        }
    }

    // Computed Properties
    public bool IsActive => WorkflowStatus is WorkflowStatus.Approved or WorkflowStatus.NotStarted ||
                            DataStatus == DataState.Active;
    public bool IsPendingApproval => WorkflowStatus == WorkflowStatus.InProgress;
    public bool IsRejected => WorkflowStatus == WorkflowStatus.Rejected;
    public bool IsWorkflowComplete => WorkflowStatus is WorkflowStatus.Approved or WorkflowStatus.Rejected;

    // Workflow Helper Methods
    public void InitiateWorkflow(string initiatedByEmail, string? initiatedByEmployeeId = null, string? initiatedByName = null, string? comments = null)
    {
        WorkflowStatus = WorkflowStatus.InProgress;
        WorkflowInitiatedAt = DateTime.UtcNow;
        WorkflowInitiatedByEmail = initiatedByEmail;
        WorkflowInitiatedByEmployeeId = initiatedByEmployeeId;
        WorkflowInitiatedByName = initiatedByName;
        WorkflowComments = comments;
    }

    public void CompleteWorkflow(WorkflowStatus finalStatus, string completedByEmail, string? completedByEmployeeId = null, string? completedByName = null, string? comments = null)
    {
        WorkflowStatus = finalStatus;
        WorkflowCompletedAt = DateTime.UtcNow;
        WorkflowCompletedByEmail = completedByEmail;
        WorkflowCompletedByEmployeeId = completedByEmployeeId;
        WorkflowCompletedByName = completedByName;
        if (!comments.IsEmpty())
            WorkflowComments = comments;

        if (finalStatus == WorkflowStatus.Rejected)
            WorkflowRejectionReason = comments;
    }

    public void UpdateWorkflowState(string newState, string? comments = null)
    {
        CurrentWorkflowState = newState;
        if (!comments.IsEmpty())
            WorkflowComments = comments;
    }
    
    public void RejectWorkflow(string rejectedByEmail, string? rejectedByEmployeeId = null, string? rejectedByName = null, string reason = "")
    {
        WorkflowStatus = WorkflowStatus.Rejected;
        WorkflowRejectionReason = reason;
        WorkflowCompletedAt = DateTime.UtcNow;
        WorkflowCompletedByEmail = rejectedByEmail;
        WorkflowCompletedByEmployeeId = rejectedByEmployeeId;
        WorkflowCompletedByName = rejectedByName;
        WorkflowComments = reason;
    }
    
    public void ApproveWorkflow(string approvedByEmail, string? approvedByEmployeeId = null, string? approvedByName = null, string? comments = null)
    {
        WorkflowStatus = WorkflowStatus.Approved;
        WorkflowCompletedAt = DateTime.UtcNow;
        WorkflowCompletedByEmail = approvedByEmail;
        WorkflowCompletedByEmployeeId = approvedByEmployeeId;
        WorkflowCompletedByName = approvedByName;
        if (!comments.IsEmpty())
            WorkflowComments = comments;
    }
    
    // Status Checks
    public bool CanBeEdited()
    {
        // During seeding, allow editing regardless of workflow status
        if (_isWorkflowDisabledForSeeding)
        {
            return true;
        }
        
        return WorkflowStatus is WorkflowStatus.NotStarted or WorkflowStatus.Rejected;
    }
    
    public bool CanBeDeleted()
    {
        // During seeding, allow deletion regardless of workflow status
        if (_isWorkflowDisabledForSeeding)
        {
            return true;
        }
        
        return WorkflowStatus != WorkflowStatus.Approved;
    }
    
    public bool RequiresApproval()
    {
        return WorkflowStatus == WorkflowStatus.InProgress;
    }
    
    // Get the appropriate workflow step for current entity state
    public WorkflowStep? GetCurrentWorkflowStep()
    {
        return WorkflowDefinition.Steps
            .Where(step => EvaluateStepConditions(step.Conditions))
            .OrderBy(step => step.Order)
            .FirstOrDefault();
    }
    
    // Check if entity should auto-approve based on its workflow definition
    public bool ShouldAutoApprove()
    {
        return WorkflowDefinition.AutoApproval.Enabled && 
               EvaluateStepConditions(WorkflowDefinition.AutoApproval.Conditions);
    }
    
    private bool EvaluateStepConditions(List<WorkflowCondition> conditions)
    {
        if (conditions.Count == 0)
        {
            return true;
        }
    
        foreach (var condition in conditions)
        {
            var property = GetType().GetProperty(condition.Field);
            if (property == null) continue;
            
            var value = property.GetValue(this);
            if (!EvaluateCondition(value, condition.Operator, condition.Value))
                return false;
        }
        
        return true;
    }
    
    private bool EvaluateCondition(object? actualValue, WorkflowOperators @operator, object expectedValue)
    {
        return @operator switch
        {
            WorkflowOperators.Equals => Equals(actualValue, expectedValue),
            WorkflowOperators.NotEquals => !Equals(actualValue, expectedValue),
            WorkflowOperators.GreaterThan => CompareValues(actualValue, expectedValue) > 0,
            WorkflowOperators.LessThan => CompareValues(actualValue, expectedValue) < 0,
            WorkflowOperators.GreaterThanOrEqual => CompareValues(actualValue, expectedValue) >= 0,
            WorkflowOperators.LessThanOrEqual => CompareValues(actualValue, expectedValue) <= 0,
            _ => false
        };
    }

    private int CompareValues(object? actualValue, object expectedValue)
    {
        if (actualValue is IComparable comparable && expectedValue is IComparable)
        {
            return comparable.CompareTo(expectedValue);
        }
        return 0;
    }
}
