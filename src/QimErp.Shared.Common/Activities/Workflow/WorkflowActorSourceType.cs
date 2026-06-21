namespace QimErp.Shared.Common.Activities.Workflow;

/// <summary>
/// Discriminates the upstream system that owns an actor row in the Platform.Workflow
/// local projection. The Workflow module participates in approval flows regardless of
/// who the actor is — today employees, later customers, vendors, or external parties.
/// Add new values when a new source module starts producing actors.
/// </summary>
public enum WorkflowActorSourceType
{
    Employee = 0,
    Customer = 1,
    Vendor = 2,
    External = 3,
}
