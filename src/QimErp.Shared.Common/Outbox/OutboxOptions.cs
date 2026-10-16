namespace QimErp.Shared.Common.Outbox;

/// <summary>
/// Configuration for the transactional outbox and Akka relay (DATA-09 / Appendix 1).
/// Bound from the <c>Outbox</c> configuration section.
/// </summary>
public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>When false, <see cref="IOutbox"/> still writes rows but the relay does not start.</summary>
    public bool RelayEnabled { get; set; } = true;

    /// <summary>Poll interval for <c>OutboxPollerActor</c>.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Lease duration applied when claiming a batch.</summary>
    public TimeSpan ClaimLease { get; set; } = TimeSpan.FromSeconds(30);

    public int BatchSize { get; set; } = 100;

    public int DispatchWorkerCount { get; set; } = 8;

    public int MaxAttempts { get; set; } = 10;

    /// <summary>
    /// EventType → transports. Example: <c>"EmployeeChangedEvent": ["temporal"]</c>.
    /// When an event type is absent, <see cref="DefaultTransports"/> is used.
    /// </summary>
    public Dictionary<string, string[]> Routing { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Default transports when Routing has no entry. Level-S modules should prefer
    /// <c>["temporal"]</c>; the SDK default is <c>inprocess</c> so existing MediatR
    /// handlers keep working until modules migrate.
    /// </summary>
    public string[] DefaultTransports { get; set; } = [OutboxTransports.InProcess];

    /// <summary>
    /// Temporal workflow type + task queue per event type (used by <see cref="Transports.TemporalEventTransport"/>).
    /// </summary>
    public Dictionary<string, OutboxTemporalRoute> Temporal { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>Temporal target for one outbox event type.</summary>
public sealed class OutboxTemporalRoute
{
    /// <summary>Workflow type name registered with Temporal workers.</summary>
    public string WorkflowType { get; set; } = string.Empty;

    /// <summary>Base task queue (SDK appends <c>Temporal:TaskQueueSuffix</c> when present).</summary>
    public string TaskQueue { get; set; } = string.Empty;
}
