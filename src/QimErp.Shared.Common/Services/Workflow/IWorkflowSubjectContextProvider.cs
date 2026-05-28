namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Optional contract for workflow subjects whose approver resolution should target
/// a related entity (e.g. parent employee) rather than the subject row itself.
/// </summary>
public interface IWorkflowSubjectContextProvider
{
    string? GetWorkflowSubjectContextType();
    string? GetWorkflowSubjectContextId();
}
