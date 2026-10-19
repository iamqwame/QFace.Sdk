using System.Diagnostics.Metrics;

namespace QimErp.Shared.Common.Outbox;

/// <summary>
/// DATA-32 outbox drift signals (enqueued / published / processed / failed / batch size / pending sample).
/// </summary>
public static class OutboxMetrics
{
    public const string MeterName = "QimErp.Shared.Common.Outbox";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> Enqueued =
        Meter.CreateCounter<long>("outbox.messages.enqueued", description: "Outbox rows written at save");
    private static readonly Counter<long> Published =
        Meter.CreateCounter<long>("outbox.messages.published", description: "In-process MediatR publishes succeeded");
    private static readonly Counter<long> Processed =
        Meter.CreateCounter<long>("outbox.messages.processed", description: "Backstop processor dispatches succeeded");
    private static readonly Counter<long> Failed =
        Meter.CreateCounter<long>("outbox.messages.failed", description: "Outbox publish or dispatch failures");
    private static readonly Histogram<long> BatchSize =
        Meter.CreateHistogram<long>("outbox.batch.size", description: "Pending ids claimed per processor sweep");

    private static long _pendingSample;

    static OutboxMetrics()
    {
        Meter.CreateObservableGauge(
            "outbox.messages.pending",
            () => Volatile.Read(ref _pendingSample),
            description: "Last observed unprocessed count from a processor sweep batch claim");
    }

    public static void RecordEnqueued(int count = 1)
    {
        if (count > 0)
            Enqueued.Add(count);
    }

    public static void RecordPublished(int count = 1)
    {
        if (count > 0)
            Published.Add(count);
    }

    public static void RecordProcessed(int count = 1)
    {
        if (count > 0)
            Processed.Add(count);
    }

    public static void RecordFailed(int count = 1)
    {
        if (count > 0)
            Failed.Add(count);
    }

    public static void RecordSweepBatch(int claimedPending)
    {
        if (claimedPending < 0)
            return;

        BatchSize.Record(claimedPending);
        Volatile.Write(ref _pendingSample, claimedPending);
    }
}
