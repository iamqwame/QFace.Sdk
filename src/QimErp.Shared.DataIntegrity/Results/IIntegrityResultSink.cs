namespace QimErp.Shared.DataIntegrity;

public interface IIntegrityResultSink
{
    Task WriteAsync(IntegrityRunInfo run, IntegrityCheckOutcome outcome, CancellationToken cancellationToken);
}
