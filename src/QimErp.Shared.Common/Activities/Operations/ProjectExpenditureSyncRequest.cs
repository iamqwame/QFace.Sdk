using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Activities.Operations;

public class ProjectExpenditureSyncRequest
{
    public string TenantId { get; set; } = string.Empty;
    public ApBillPostedEvent Posted { get; set; } = null!;
}
