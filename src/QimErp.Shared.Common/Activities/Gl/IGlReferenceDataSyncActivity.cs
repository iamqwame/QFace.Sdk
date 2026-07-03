using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.Gl;

/// <summary>
/// Activity implemented by modules that maintain local GL reference read models
/// (ChartOfAccount, CostCenter, FiscalYear, FiscalPeriod).
/// Each module registers a worker on <c>qimerp-{module}-gl-sync</c>.
/// </summary>
public interface IGlReferenceDataSyncActivity
{
    [Activity]
    Task ProcessAsync(GlReferenceDataSyncRequest request, CancellationToken cancellationToken = default);
}
