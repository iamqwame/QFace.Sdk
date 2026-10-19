namespace QimErp.Shared.DataIntegrity.Results;

internal sealed class MetricsIntegrityResultSink(IntegrityMetrics metrics) : IIntegrityResultSink
{
    public Task WriteAsync(IntegrityRunInfo run, IntegrityCheckOutcome outcome, CancellationToken cancellationToken)
    {
        switch (outcome.Status)
        {
            case IntegrityCheckStatus.Clean:
            case IntegrityCheckStatus.Findings:
                metrics.SetFound(outcome.Check, outcome.Count);
                break;
            case IntegrityCheckStatus.TimedOut:
            case IntegrityCheckStatus.Failed:
                metrics.RecordFailure(outcome.Check, outcome.FailureReason ?? "unknown");
                break;
        }

        return Task.CompletedTask;
    }
}
