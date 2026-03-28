using QimErp.Shared.Common.Workflow.Entities;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Dedicated activity interface for resolving workflow step approvers to concrete employee records.
///
/// Always executed on the "qimerp-employee-approvals" task queue (CoreHR Employee worker),
/// since employees are the single source of approver data regardless of which module's entity
/// is being approved.
///
/// Separating this from IModuleApprovalActivity ensures no other module needs a reference
/// to Employee.Shared — approver resolution is always delegated to HrApprovalActivity.
/// </summary>
public interface IApproverResolverActivity
{
    Task<List<ResolvedApprover>> ResolveApproversAsync(ApprovalWorkflowInput input, WorkflowStep step);
}
