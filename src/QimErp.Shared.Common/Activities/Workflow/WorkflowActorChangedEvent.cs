namespace QimErp.Shared.Common.Activities.Workflow;

/// <summary>
/// Payload describing a single actor (initiator/approver/observer) that participates in
/// Platform.Workflow approvals. Produced by any source module that wants its principals
/// usable in workflows.
///
/// Today: CoreHr employees. Tomorrow: customers, vendors, external auditors — all map
/// onto this contract by setting <see cref="SourceType"/> and <see cref="SourceId"/>.
///
/// The Platform.Workflow.WebApi worker consumes this via <see cref="IWorkflowActorSyncActivity"/>
/// and upserts into its local <c>Actors</c> table keyed by (TenantId, SourceType, SourceId).
/// </summary>
public class WorkflowActorChangedEvent
{
    /// <summary>Origin system that owns the source row.</summary>
    public WorkflowActorSourceType SourceType { get; set; }

    /// <summary>Primary key of the actor in the source system (e.g. CoreHr EmployeeId).</summary>
    public Guid SourceId { get; set; }

    /// <summary>Tenant scope — required for all operations.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Email of the actor. Required — used as a fallback lookup key in Workflow.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Human-readable display name (e.g. full name for employees, business name for vendors).</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Optional avatar / logo URL.</summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>Whether the actor is currently active in the source system.</summary>
    public bool IsActive { get; set; } = true;

    // ── Organizational context (employee-shaped; null for non-employee sources) ──
    public Guid? OrganizationalUnitId { get; set; }
    public string? OrganizationalUnitCode { get; set; }
    public string? OrganizationalUnitName { get; set; }

    // ── Supervisor chain (employee-shaped; null for non-employee sources) ────────
    /// <summary>SourceType of the supervisor — defaults to the same as <see cref="SourceType"/> when relevant.</summary>
    public WorkflowActorSourceType? SupervisorSourceType { get; set; }

    /// <summary>SourceId of the supervisor in their own source system.</summary>
    public Guid? SupervisorSourceId { get; set; }

    /// <summary>Email of the supervisor — useful fallback when SourceId isn't known.</summary>
    public string? SupervisorEmail { get; set; }
}
