using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace QimErp.Shared.DataIntegrity.Results;

/// <summary>Tenant ids must never become metric tags.</summary>
internal sealed class IntegrityMetrics : IDisposable
{
    public const string MeterName = "QimErp.Shared.DataIntegrity";

    private readonly Meter _meter = new(MeterName);
    private readonly ConcurrentDictionary<string, long> _found = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, long> _lastRun = new(StringComparer.Ordinal);
    private readonly Counter<long> _failures;
    private readonly Histogram<double> _runDuration;

    public IntegrityMetrics()
    {
        _failures = _meter.CreateCounter<long>("data_integrity_check_failures", description: "Integrity checks that timed out or failed");
        _runDuration = _meter.CreateHistogram<double>("data_integrity_run_duration_seconds", unit: "s", description: "Duration of one integrity run per module");
        _meter.CreateObservableGauge(
            "data_integrity_found",
            () => _found.Select(f => new Measurement<long>(f.Value, new KeyValuePair<string, object?>("check", f.Key))),
            description: "Offending rows found by the last completed run of each check");
        _meter.CreateObservableGauge(
            "data_integrity_last_run_unixtime",
            () => _lastRun.Select(r => new Measurement<long>(r.Value, new KeyValuePair<string, object?>("module", r.Key))),
            description: "Unix time of the last integrity run per module that did not fail overall");
    }

    public void SetFound(string check, long count) => _found[check] = count;

    public void RecordFailure(string check, string reason)
        => _failures.Add(1, new KeyValuePair<string, object?>("check", check), new KeyValuePair<string, object?>("reason", reason));

    public void RecordRunDuration(string module, double durationSeconds)
        => _runDuration.Record(durationSeconds, new KeyValuePair<string, object?>("module", module));

    public void RecordRunCompleted(string module, DateTimeOffset completedAt) => _lastRun[module] = completedAt.ToUnixTimeSeconds();

    public void Dispose() => _meter.Dispose();
}
