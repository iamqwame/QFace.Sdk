using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Activities.Gl;

public enum GlReferenceEntityType
{
    ChartOfAccount,
    CostCenter,
    FiscalYear,
    FiscalPeriod
}

public enum GlReferenceSyncOperation
{
    CreatedOrUpdated,
    Deleted
}

/// <summary>
/// Payload for <see cref="IGlReferenceDataSyncWorkflow"/> fan-out activities.
/// GL Core starts the workflow after reference data changes; each module upserts
/// or deactivates its local read-model copy.
/// </summary>
public class GlReferenceDataSyncRequest
{
    public GlReferenceEntityType EntityType { get; set; }
    public GlReferenceSyncOperation Operation { get; set; }
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Resolved by orchestrator from IAM; activities no-op when target module is not installed.</summary>
    public List<string>? SelectedModules { get; set; }

    public ChartOfAccountUpdatedEvent? ChartOfAccountUpdated { get; set; }
    public ChartOfAccountDeletedEvent? ChartOfAccountDeleted { get; set; }
    public CostCenterUpdatedEvent? CostCenterUpdated { get; set; }
    public CostCenterDeletedEvent? CostCenterDeleted { get; set; }
    public FiscalYearUpdatedEvent? FiscalYearUpdated { get; set; }
    public FiscalPeriodUpdatedEvent? FiscalPeriodUpdated { get; set; }
}
