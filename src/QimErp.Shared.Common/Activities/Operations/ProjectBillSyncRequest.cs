using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Activities.Operations;

public class ProjectBillSyncRequest
{
    public string TenantId { get; set; } = string.Empty;
    public ProjectBillGeneratedEvent Generated { get; set; } = null!;
}
