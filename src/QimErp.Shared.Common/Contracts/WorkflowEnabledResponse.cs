namespace QimErp.Shared.Common.Contracts;

public class ApiBaseResponse
{
    public string Status { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByEmail { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime Created { get; set; }

    public string LastModifiedByUserId { get; set; } = string.Empty;
    public string LastModifiedByEmail { get; set; } = string.Empty;
    public string LastModifiedByName { get; set; } = string.Empty;
    public DateTime? LastModified { get; set; }
}

/// <summary>
/// Base response class for entities that support workflow functionality
/// Note: Workflow properties have been commented out to reduce response size.
/// Use IWorkflowEnabled entity directly if workflow information is needed.
/// </summary>
public abstract class WorkflowEnabledResponse: ApiBaseResponse
{
    // Workflow properties commented out to reduce response payload size
    
    // /// <summary>
    // /// Flag to control whether workflow properties should be included in the response.
    // /// Default is false to exclude workflow properties by default.
    // /// </summary>
    // public bool IncludeWorkflowProperties { get; set; } = false;

    // // Core Workflow Properties
    // public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.NotStarted;
    // public Guid? CurrentWorkflowHistoryId { get; set; }
    // public string? CurrentWorkflowState { get; set; }
    // public string? WorkflowComments { get; set; }
    // public DateTime? WorkflowInitiatedAt { get; set; }
    // public string? WorkflowInitiatedBy { get; set; }
    // public string? WorkflowRejectionReason { get; set; }
    // public DateTime? WorkflowCompletedAt { get; set; }
    // public string? WorkflowCompletedBy { get; set; }

    // // Workflow Metadata
    // public string EntityType { get; set; } = string.Empty;
    // public bool IsWorkflowEnabled { get; set; } = true;

    // // Computed Status Properties
    // public bool IsActive => WorkflowStatus is WorkflowStatus.Approved or WorkflowStatus.NotStarted;
    // public bool IsPendingApproval => WorkflowStatus == WorkflowStatus.InProgress;
    // public bool IsRejected => WorkflowStatus == WorkflowStatus.Rejected;
    // public bool IsWorkflowComplete => WorkflowStatus is WorkflowStatus.Approved or WorkflowStatus.Rejected;
    // public bool CanBeEdited => WorkflowStatus is WorkflowStatus.NotStarted or WorkflowStatus.Rejected;
    // public bool CanBeDeleted => WorkflowStatus != WorkflowStatus.Approved;
    // public bool RequiresApproval => WorkflowStatus == WorkflowStatus.InProgress;

    // // Display Properties
    // public string WorkflowStatusDisplay => WorkflowStatus.ToString();
    // public string WorkflowInitiatedAtDisplay => WorkflowInitiatedAt?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;
    // public string WorkflowCompletedAtDisplay => WorkflowCompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty;

    // // Workflow Actions (for UI)
    // public bool CanInitiateWorkflow => WorkflowStatus == WorkflowStatus.NotStarted && IsWorkflowEnabled;
    // public bool CanApprove => WorkflowStatus == WorkflowStatus.InProgress;
    // public bool CanReject => WorkflowStatus == WorkflowStatus.InProgress;
    // public bool CanCancel => WorkflowStatus == WorkflowStatus.InProgress;
    // public bool ShowWorkflowActions => IsWorkflowEnabled && WorkflowStatus != WorkflowStatus.NotStarted;
}
