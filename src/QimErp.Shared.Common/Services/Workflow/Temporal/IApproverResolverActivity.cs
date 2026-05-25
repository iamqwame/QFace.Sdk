using Temporalio.Activities;
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
    [Activity]
    Task<List<ResolvedApprover>> ResolveApproversAsync(ApprovalWorkflowInput input, WorkflowStep step);

    /// <summary>
    /// Resolves the initiator's Employee record (full name, profile picture, employee code,
    /// office email) from their login email. Used by ApprovalWorkflow when creating the
    /// workflow record so the persisted row carries the proper Employee identity instead of
    /// whatever the JWT happened to carry.
    ///
    /// Returns <c>null</c> when no matching Employee exists for the email (system actor,
    /// service user, or a tenant where the initiator hasn't been imported as an Employee).
    /// </summary>
    [Activity]
    Task<ResolvedApprover?> ResolveInitiatorByEmailAsync(string email);
}
