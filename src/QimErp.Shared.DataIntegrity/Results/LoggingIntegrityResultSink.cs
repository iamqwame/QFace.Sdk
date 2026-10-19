using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QimErp.Shared.DataIntegrity.Results;

internal sealed class LoggingIntegrityResultSink(
    ILogger<LoggingIntegrityResultSink> logger,
    IOptions<DataIntegrityOptions> options) : IIntegrityResultSink
{
    public Task WriteAsync(IntegrityRunInfo run, IntegrityCheckOutcome outcome, CancellationToken cancellationToken)
    {
        var partial = outcome.Status is IntegrityCheckStatus.TimedOut or IntegrityCheckStatus.Failed;
        var ordered = outcome.Findings
            .OrderByDescending(f => f.Count)
            .ThenBy(f => f.TenantId, StringComparer.Ordinal)
            .ToList();
        var logged = ordered.Take(options.Value.MaxTenantsLoggedPerCheck).ToList();

        foreach (var finding in logged)
        {
            if (partial)
            {
                logger.LogWarning(
                    "data_integrity_found check={Check} tenantId={TenantId} count={Count} sampleIds={SampleIds} runId={RunId} partial={Partial}",
                    outcome.Check, finding.TenantId, finding.Count, finding.SampleIds, run.RunId, true);
            }
            else
            {
                logger.LogWarning(
                    "data_integrity_found check={Check} tenantId={TenantId} count={Count} sampleIds={SampleIds} runId={RunId}",
                    outcome.Check, finding.TenantId, finding.Count, finding.SampleIds, run.RunId);
            }
        }

        if (ordered.Count > logged.Count)
        {
            var rest = ordered.Skip(logged.Count).ToList();
            logger.LogWarning(
                "data_integrity_found_more check={Check} additionalTenants={Tenants} additionalCount={Count} runId={RunId}",
                outcome.Check, rest.Count, rest.Sum(f => f.Count), run.RunId);
        }

        if (partial)
        {
            logger.LogWarning(
                "data_integrity_check_failed check={Check} status={Status} reason={Reason} runId={RunId}",
                outcome.Check, outcome.Status, outcome.FailureReason, run.RunId);
        }
        else
        {
            logger.LogInformation(
                "data_integrity_check check={Check} status={Status} count={Count} durationMs={DurationMs} runId={RunId}",
                outcome.Check, outcome.Status, outcome.Count, (long)(outcome.DurationSeconds * 1000), run.RunId);
        }

        return Task.CompletedTask;
    }
}
