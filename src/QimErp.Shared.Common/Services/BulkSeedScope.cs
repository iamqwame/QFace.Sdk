namespace QimErp.Shared.Common.Services;

/// <summary>
/// Process-local flag that signals "we are inside a demo/bulk seed operation,
/// suppress heavyweight side effects on SaveChanges". Read by
/// <see cref="QimErp.Shared.Common.Interceptors.AuditEntitySaveChangesInterceptor"/>
/// to skip workflow initiation, status-change capture, and domain-event publishing
/// during high-volume seeding.
///
/// Without this, seeding 10k employees would fan out 10k EmployeeChangedEvents,
/// 10k workflow lookups, and 10k RabbitMQ publishes — saturating the bus.
/// </summary>
public static class BulkSeedScope
{
    private static readonly AsyncLocal<bool> Suppressed = new();

    public static bool IsSuppressed => Suppressed.Value;

    /// <summary>
    /// Enter a suppression scope on the current async flow. Dispose to restore
    /// the previous value (re-entrant safe).
    /// </summary>
    public static IDisposable Enter()
    {
        var previous = Suppressed.Value;
        Suppressed.Value = true;
        return new Restorer(previous);
    }

    private sealed class Restorer(bool previous) : IDisposable
    {
        public void Dispose() => Suppressed.Value = previous;
    }
}
