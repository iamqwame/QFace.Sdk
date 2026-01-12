namespace QimErp.Shared.Common.Entities;

public abstract class GuidAuditableEntity : BaseAuditableEntity<Guid>
{
    protected static Guid CreateId()
    {
        return Guid.CreateVersion7();
    }
}

public abstract class GuidWorkflowAuditableEntity : WorkflowBaseAuditableEntity<Guid>
{
    protected static Guid CreateId()
    {
        return Guid.CreateVersion7();
    }
    
   
}