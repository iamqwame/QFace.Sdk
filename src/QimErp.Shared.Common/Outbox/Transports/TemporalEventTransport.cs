using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QFace.Sdk.Temporal.Options;
using QimErp.Shared.Common.Services.Workflow;
using Temporalio.Api.Enums.V1;
using Temporalio.Client;
using Temporalio.Exceptions;

namespace QimErp.Shared.Common.Outbox.Transports;

/// <summary>
/// Level-S default bus: starts a Temporal workflow with a deterministic Id
/// scoped by tenant, entity and persisted event identity. Duplicate starts are treated as success.
/// Workflow approval triggers use <see cref="IOutboxTemporalHandler"/> when registered.
/// </summary>
internal sealed class TemporalEventTransport(
    IServiceScopeFactory scopes,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<TemporalEventTransport> logger) : IEventTransport
{
    public string Name => OutboxTransports.Temporal;

    public async Task SendAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(envelope.TenantId);
        using var scope = scopes.CreateScope();
        var sp = scope.ServiceProvider;
        var tenantScope = sp.GetService<QFace.Sdk.Temporal.Interceptors.ITenantScopeSetter>();
        var userContext = sp.GetService<ICurrentUserService>();
        try
        {
            tenantScope?.SetTenantScope(envelope.TenantId);
            userContext?.SetContext(envelope.TenantId, "outbox-relay@system", "Outbox Relay", "system");
            await DispatchAsync(envelope, sp, cancellationToken);
        }
        finally
        {
            userContext?.ClearContext();
            tenantScope?.ClearTenantScope();
        }
    }

    private async Task DispatchAsync(EventEnvelope envelope, IServiceProvider sp, CancellationToken cancellationToken)
    {
        var handlers = sp.GetServices<IOutboxTemporalHandler>().ToList();
        foreach (var handler in handlers)
        {
            if (!handler.CanHandle(envelope.EventType))
                continue;

            await handler.SendAsync(envelope, cancellationToken);
            return;
        }

        if (!outboxOptions.Value.Temporal.TryGetValue(envelope.EventType, out var route)
            || string.IsNullOrWhiteSpace(route.WorkflowType)
            || string.IsNullOrWhiteSpace(route.TaskQueue))
        {
            throw new InvalidOperationException(
                $"No Temporal route for EventType '{envelope.EventType}'. " +
                "Add Outbox:Temporal:{EventType}:WorkflowType/TaskQueue or register IOutboxTemporalHandler.");
        }

        var client = sp.GetService<ITemporalClient>()
                     ?? throw new InvalidOperationException(
                         "ITemporalClient is not registered. Call AddTemporalWorkflow / AddTemporalClient.");

        var taskQueue = ResolveTaskQueue(sp, route.TaskQueue);
        var tenantKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(envelope.TenantId)));
        var eventId = envelope.EventId != Guid.Empty ? envelope.EventId : envelope.OutboxId;
        if (eventId == Guid.Empty)
            throw new ArgumentException("A persisted event or outbox identity is required.", nameof(envelope));
        var workflowId =
            $"{tenantKey}-{envelope.EventType}-{envelope.EntityId}-v{envelope.EntityVersion}-{eventId:N}";

        IReadOnlyCollection<object> args = [envelope.Payload];

        try
        {
            await client.StartWorkflowAsync(
                route.WorkflowType,
                args,
                new WorkflowOptions(workflowId, taskQueue)
                {
                    IdReusePolicy = WorkflowIdReusePolicy.RejectDuplicate,
                    IdConflictPolicy = WorkflowIdConflictPolicy.UseExisting,
                });

            logger.LogInformation(
                "Temporal outbox start accepted. WorkflowId={WorkflowId}, EventType={EventType}, OutboxId={OutboxId}",
                workflowId, envelope.EventType, envelope.OutboxId);
        }
        catch (WorkflowAlreadyStartedException)
        {
            logger.LogDebug(
                "Temporal workflow already started (idempotent). WorkflowId={WorkflowId}", workflowId);
        }
    }

    private static string ResolveTaskQueue(IServiceProvider sp, string baseQueue)
    {
        var opts = sp.GetService<IOptions<TemporalOptions>>()?.Value;
        if (opts is not null)
            return opts.WithTaskQueueSuffix(baseQueue);

        var suffix = sp.GetService<IConfiguration>()?["Temporal:TaskQueueSuffix"] ?? string.Empty;
        if (string.IsNullOrEmpty(suffix)) return baseQueue;
        return baseQueue.EndsWith(suffix, StringComparison.Ordinal) ? baseQueue : baseQueue + suffix;
    }
}

/// <summary>
/// Optional specialized Temporal dispatch (approval workflows, module-specific starters).
/// </summary>
public interface IOutboxTemporalHandler
{
    bool CanHandle(string eventType);

    Task SendAsync(EventEnvelope envelope, CancellationToken cancellationToken = default);
}

/// <summary>
/// Routes <c>WorkflowApprovalRequired</c> outbox rows through <see cref="IWorkflowTriggerBridge"/>.
/// </summary>
internal sealed class WorkflowApprovalOutboxTemporalHandler(
    IServiceScopeFactory scopes,
    ILogger<WorkflowApprovalOutboxTemporalHandler> logger) : IOutboxTemporalHandler
{
    public const string EventTypeName = "WorkflowApprovalRequired";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public bool CanHandle(string eventType) =>
        string.Equals(eventType, EventTypeName, StringComparison.OrdinalIgnoreCase)
        || string.Equals(eventType, nameof(WorkflowEventMessage), StringComparison.OrdinalIgnoreCase);

    public async Task SendAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        using var scope = scopes.CreateScope();
        var bridge = scope.ServiceProvider.GetService<IWorkflowTriggerBridge>()
                     ?? throw new InvalidOperationException(
                         "IWorkflowTriggerBridge is not registered for WorkflowApprovalRequired outbox delivery.");

        var message = JsonSerializer.Deserialize<WorkflowEventMessage>(envelope.Payload, JsonOptions)
                      ?? throw new InvalidOperationException("WorkflowEventMessage payload was null.");

        var ok = await bridge.TryTriggerTemporalWorkflowAsync(message, cancellationToken);
        if (!ok)
        {
            throw new InvalidOperationException(
                $"IWorkflowTriggerBridge returned false for {message.EntityType}/{message.EntityId}.");
        }

        logger.LogDebug(
            "WorkflowApprovalRequired delivered via Temporal bridge. OutboxId={OutboxId}",
            envelope.OutboxId);
    }
}
