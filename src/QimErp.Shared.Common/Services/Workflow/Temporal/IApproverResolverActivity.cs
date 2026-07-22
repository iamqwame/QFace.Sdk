using Temporalio.Activities;

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
    /// Takes a <see cref="ResolveInitiatorRequest"/> (not bare <c>string</c> args) because the
    /// Temporal worker's tenant-seeding interceptor extracts a <c>TenantId</c> property off the
    /// activity's argument object via reflection — a raw string parameter has no such property,
    /// so the interceptor would silently skip seeding <c>ITenantContext</c>, the tenant-scoped
    /// Employee lookup would return nothing (fail-closed tenant isolation), and callers would
    /// fall back to the stale JWT-derived name/picture this method exists to avoid.
    ///
    /// Returns <c>null</c> when no matching Employee exists for the email (system actor,
    /// service user, or a tenant where the initiator hasn't been imported as an Employee).
    /// </summary>
    [Activity]
    Task<ResolvedApprover?> ResolveInitiatorByEmailAsync(ResolveInitiatorRequest request);

    /// <summary>
    /// Resolves step notification tokens (Initiator, department, role, etc.) to employee emails.
    /// </summary>
    [Activity]
    Task<List<ResolvedApprover>> ResolveNotificationRecipientsAsync(
        ApprovalWorkflowInput input,
        WorkflowStep step,
        IReadOnlyList<string> recipientTokens);
}

/// <summary>
/// Argument object for <see cref="IApproverResolverActivity.ResolveInitiatorByEmailAsync"/>.
/// The public <c>TenantId</c> property is required — the Temporal tenant-seeding interceptor
/// reflects over activity arguments looking for exactly that property name.
/// </summary>
public record ResolveInitiatorRequest(string Email, string TenantId);
