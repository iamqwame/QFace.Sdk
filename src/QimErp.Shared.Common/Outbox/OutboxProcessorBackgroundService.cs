using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Services.Workflow;

namespace QimErp.Shared.Common.Outbox;

/// <summary>
/// Backstop sweeper for <see cref="OutboxMessage"/> rows left unprocessed by
/// <see cref="Interceptors.AuditEntitySaveChangesInterceptor"/>'s in-process dispatch (e.g. the
/// process crashed between commit and dispatch, or the immediate dispatch attempt failed).
/// Mirrors the poll / scope / process / sleep shape of
/// <c>QimErp.Platform.Workflow.WebApi.Services.WorkflowTimeoutBackgroundService</c>.
/// </summary>
public sealed class OutboxProcessorBackgroundService<TContext> : BackgroundService
    where TContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorBackgroundService<TContext>> _logger;
    private readonly OutboxProcessorOptions _options;

    public OutboxProcessorBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessorBackgroundService<TContext>> logger,
        IOptions<OutboxProcessorOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox processor started for {Context}. IntervalSeconds={IntervalSeconds}, BatchSize={BatchSize}",
            typeof(TContext).Name, _options.PollingIntervalSeconds, _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessPendingMessagesAsync(stoppingToken);
                if (processed > 0)
                {
                    _logger.LogInformation(
                        "Outbox processor dispatched {Processed} message(s) for {Context}",
                        processed, typeof(TContext).Name);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processor sweep failed for {Context}", typeof(TContext).Name);
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        _logger.LogInformation("Outbox processor stopped for {Context}", typeof(TContext).Name);
    }

    /// <summary>
    /// Processes one batch of unprocessed <see cref="OutboxMessage"/> rows. Public so tests can
    /// drive a single sweep deterministically without running the hosted-service loop.
    /// </summary>
    public async Task<int> ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        List<Guid> pendingIds;
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            pendingIds = await context.Set<OutboxMessage>()
                .Where(m => !m.IsProcessed)
                .OrderBy(m => m.OccurredAt)
                .Take(_options.BatchSize)
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);
        }

        OutboxMetrics.RecordSweepBatch(pendingIds.Count);

        var processedCount = 0;
        foreach (var id in pendingIds)
        {
            if (await ProcessSingleMessageAsync(id, cancellationToken))
                processedCount++;
        }

        return processedCount;
    }

    // One DI scope per row: isolates each message's DbContext/tenant context from the next,
    // the same way a Temporal activity gets its own scope per invocation.
    private async Task<bool> ProcessSingleMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var userContextService = scope.ServiceProvider.GetService<ICurrentUserService>();
        var tenantScopeSetter = scope.ServiceProvider.GetService<QFace.Sdk.Temporal.Interceptors.ITenantScopeSetter>();
        var companyScopeSetter = scope.ServiceProvider.GetService<QFace.Sdk.Temporal.Interceptors.ICompanyScopeSetter>();

        var message = await context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
        if (message is null || message.IsProcessed)
            return false;

        var envelope = OutboxEnvelope.Unwrap(message.Payload);
        if (envelope is null)
        {
            _logger.LogWarning("Outbox message {MessageId} has an unparsable envelope payload. Skipping.", messageId);
            return false;
        }

        // Tenant + company from envelope before dispatch — seeds the ambient user-context
        // AsyncLocal (ICurrentUserService) and both EF global-query-filter AsyncLocals
        // (ITenantScopeSetter, ICompanyScopeSetter), mirroring TenantContextActivityInterceptor's
        // Temporal activity setup. Without the company scope, redelivery would run with the
        // company filter Inactive — every company's rows visible — instead of the single
        // company the original save was scoped to.
        userContextService?.SetContext(envelope.TenantId, "outbox-processor@system", "Outbox Processor", "system");
        tenantScopeSetter?.SetTenantScope(envelope.TenantId);
        if (string.IsNullOrWhiteSpace(envelope.CompanyId))
            companyScopeSetter?.SetCompanyScope(null, null, filterActive: false);
        else
            companyScopeSetter?.SetCompanyScope(envelope.CompanyId, [envelope.CompanyId], filterActive: true);

        try
        {
            var dispatched = await DispatchAsync(message, envelope, scope.ServiceProvider, cancellationToken);
            if (!dispatched)
            {
                OutboxMetrics.RecordFailed();
                return false;
            }

            message.IsProcessed = true;
            await context.SaveChangesAsync(cancellationToken);
            OutboxMetrics.RecordProcessed();
            return true;
        }
        catch (Exception ex)
        {
            OutboxMetrics.RecordFailed();
            _logger.LogError(ex,
                "Failed to dispatch outbox message {MessageId} ({EventType})", messageId, message.EventType);
            return false;
        }
        finally
        {
            userContextService?.ClearContext();
            tenantScopeSetter?.ClearTenantScope();
            companyScopeSetter?.ClearCompanyScope();
        }
    }

    private async Task<bool> DispatchAsync(
        OutboxMessage message,
        OutboxEnvelope envelope,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var messageType = ResolveType(message.EventType);
        if (messageType is null)
        {
            _logger.LogWarning(
                "Outbox message {MessageId} has an unresolvable EventType '{EventType}'. Skipping.",
                message.Id, message.EventType);
            return false;
        }

        // Allow-list check BEFORE deserialization: EventType is a stored string resolved via
        // Type.GetType, so an attacker who can write to the OutboxMessage table (or forge this
        // string by any other route) must never get an arbitrary CLR type instantiated via
        // JsonSerializer.Deserialize. Only the two shapes this processor actually dispatches
        // are eligible.
        if (!IsDispatchableType(messageType))
        {
            _logger.LogWarning(
                "Outbox message {MessageId} resolved to disallowed type {Type}. Refusing to deserialize.",
                message.Id, messageType.FullName);
            return false;
        }

        object? payload;
        try
        {
            payload = JsonSerializer.Deserialize(envelope.Data, messageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to deserialize outbox message {MessageId} as {EventType}", message.Id, messageType.FullName);
            return false;
        }

        switch (payload)
        {
            case null:
                return false;

            // WorkflowEventMessage never implements IDomainEvent — route to the Temporal bridge.
            case WorkflowEventMessage workflowMessage:
            {
                var bridge = services.GetService<IWorkflowTriggerBridge>();
                if (bridge is null)
                {
                    _logger.LogWarning(
                        "IWorkflowTriggerBridge not registered. Cannot dispatch outbox message {MessageId}.",
                        message.Id);
                    return false;
                }

                return await bridge.TryTriggerTemporalWorkflowAsync(workflowMessage, cancellationToken);
            }

            case IDomainEvent domainEvent:
            {
                var publisher = services.GetService<IPublisher>();
                if (publisher is null)
                {
                    _logger.LogWarning(
                        "IPublisher not registered. Cannot dispatch outbox message {MessageId}.", message.Id);
                    return false;
                }

                await publisher.Publish(domainEvent, cancellationToken);
                return true;
            }

            default:
                _logger.LogWarning(
                    "Outbox message {MessageId} resolved to unsupported type {Type}.", message.Id, messageType.FullName);
                return false;
        }
    }

    private static bool IsDispatchableType(Type type) =>
        type == typeof(WorkflowEventMessage) || typeof(IDomainEvent).IsAssignableFrom(type);

    private static Type? ResolveType(string? eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return null;

        try
        {
            return Type.GetType(eventType, throwOnError: false);
        }
        catch
        {
            return null;
        }
    }
}
