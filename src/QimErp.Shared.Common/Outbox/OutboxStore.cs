using Microsoft.Extensions.Logging;

namespace QimErp.Shared.Common.Outbox;

/// <summary>
/// Claims, completes, and retries outbox rows. Uses <c>FOR UPDATE SKIP LOCKED</c> on
/// PostgreSQL; falls back to a tracked LINQ claim for InMemory / non-relational providers.
/// </summary>
public sealed class OutboxStore(DbContext db, ILogger<OutboxStore> logger)
{
    public async Task<List<OutboxMessage>> ClaimAsync(
        string node,
        int batchSize,
        TimeSpan lease,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var lockedUntil = now + lease;

        if (CanUsePostgresClaim())
        {
            try
            {
                return await ClaimWithSqlAsync(node, batchSize, lease, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "PostgreSQL outbox claim failed; falling back to LINQ claim. Node={Node}", node);
            }
        }

        return await ClaimWithLinqAsync(node, batchSize, now, lockedUntil, cancellationToken);
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken)
    {
        if (CanUseExecuteUpdate())
        {
            await db.Set<OutboxMessage>()
                .Where(m => m.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.ProcessedAt, DateTimeOffset.UtcNow)
                    .SetProperty(m => m.LockedUntil, (DateTimeOffset?)null)
                    .SetProperty(m => m.LockedBy, (string?)null),
                    cancellationToken);
            return;
        }

        var row = await db.Set<OutboxMessage>().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (row is null) return;
        row.ProcessedAt = DateTimeOffset.UtcNow;
        row.LockedUntil = null;
        row.LockedBy = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ScheduleRetryAsync(
        Guid id,
        int attempts,
        string error,
        DateTimeOffset next,
        bool dead,
        CancellationToken cancellationToken)
    {
        var truncated = Truncate(error, 2000);
        if (CanUseExecuteUpdate())
        {
            await db.Set<OutboxMessage>()
                .Where(m => m.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.Attempts, attempts)
                    .SetProperty(m => m.LastError, truncated)
                    .SetProperty(m => m.NextAttemptAt, next)
                    .SetProperty(m => m.LockedUntil, (DateTimeOffset?)null)
                    .SetProperty(m => m.LockedBy, (string?)null)
                    .SetProperty(m => m.DeadAt, dead ? DateTimeOffset.UtcNow : (DateTimeOffset?)null),
                    cancellationToken);
            return;
        }

        var row = await db.Set<OutboxMessage>().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (row is null) return;
        row.Attempts = attempts;
        row.LastError = truncated;
        row.NextAttemptAt = next;
        row.LockedUntil = null;
        row.LockedBy = null;
        row.DeadAt = dead ? DateTimeOffset.UtcNow : null;
        await db.SaveChangesAsync(cancellationToken);
    }

    private bool CanUsePostgresClaim()
    {
        var provider = db.Database.ProviderName ?? string.Empty;
        return db.Database.IsRelational()
               && provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);
    }

    private bool CanUseExecuteUpdate() => db.Database.IsRelational();

    private async Task<List<OutboxMessage>> ClaimWithSqlAsync(
        string node,
        int batchSize,
        TimeSpan lease,
        CancellationToken cancellationToken)
    {
        FormattableString sql = $"""
            UPDATE "OutboxMessages" m
               SET "LockedUntil" = now() + ({lease.TotalSeconds} * interval '1 second'),
                   "LockedBy" = {node}
             WHERE m."Id" IN (
                   SELECT "Id" FROM "OutboxMessages"
                    WHERE "ProcessedAt" IS NULL AND "DeadAt" IS NULL
                      AND "NextAttemptAt" <= now()
                      AND ("LockedUntil" IS NULL OR "LockedUntil" < now())
                    ORDER BY "OccurredAt"
                    LIMIT {batchSize}
                    FOR UPDATE SKIP LOCKED)
            RETURNING m.*
            """;

        return await db.Set<OutboxMessage>()
            .FromSql(sql)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<OutboxMessage>> ClaimWithLinqAsync(
        string node,
        int batchSize,
        DateTimeOffset now,
        DateTimeOffset lockedUntil,
        CancellationToken cancellationToken)
    {
        var candidates = await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null
                        && m.DeadAt == null
                        && m.NextAttemptAt <= now
                        && (m.LockedUntil == null || m.LockedUntil < now))
            .OrderBy(m => m.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        foreach (var msg in candidates)
        {
            msg.LockedBy = node;
            msg.LockedUntil = lockedUntil;
        }

        if (candidates.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        return candidates;
    }

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max];
}
