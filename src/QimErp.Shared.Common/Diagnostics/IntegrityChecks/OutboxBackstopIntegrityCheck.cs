namespace QimErp.Shared.Common.Diagnostics.IntegrityChecks;

/// <summary>
/// DATA-06 sample check: counts <see cref="OutboxMessage"/> rows left unprocessed past
/// <see cref="_staleAfter"/>, i.e. the backstop for
/// <see cref="Outbox.OutboxProcessorBackgroundService{TContext}"/> itself having stalled
/// (crashed, deadlocked, or never started). A healthy processor keeps this at zero; a nonzero
/// count means the live sweep is not keeping up and needs attention.
/// </summary>
public sealed class OutboxBackstopIntegrityCheck<TContext> : IIntegrityCheck
    where TContext : DbContext
{
    public const string CheckName = "outbox.backstop.stuck_messages";
    private const int SampleSize = 10;

    private readonly TContext _context;
    private readonly TimeSpan _staleAfter;

    public OutboxBackstopIntegrityCheck(TContext context, TimeSpan? staleAfter = null)
    {
        _context = context;
        _staleAfter = staleAfter ?? TimeSpan.FromHours(1);
    }

    public string Name => CheckName;

    public async Task<IntegrityCheckResult> RunAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - _staleAfter;
        var stuck = _context.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null && m.DeadAt == null && m.OccurredAt < cutoff);

        var count = await stuck.LongCountAsync(cancellationToken);
        if (count == 0)
            return IntegrityCheckResult.Clean(Name);

        var sampleIds = await stuck
            .OrderBy(m => m.OccurredAt)
            .Take(SampleSize)
            .Select(m => m.Id.ToString())
            .ToListAsync(cancellationToken);

        return new IntegrityCheckResult(Name, count, sampleIds);
    }
}
