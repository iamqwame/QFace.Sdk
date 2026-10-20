using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace QimErp.Shared.Common.Outbox.Transports;

/// <summary>
/// Monolith / default transport: deserializes the payload to a known <see cref="IDomainEvent"/>
/// (or MediatR <see cref="INotification"/>) and publishes in-process. Uses Ask-equivalent
/// semantics — exceptions fail the dispatch so the outbox retries.
/// </summary>
internal sealed class InProcessEventTransport(
    IServiceScopeFactory scopes,
    ILogger<InProcessEventTransport> logger) : IEventTransport
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string Name => OutboxTransports.InProcess;

    public async Task SendAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(envelope.TenantId);
        using var scope = scopes.CreateScope();
        var publisher = scope.ServiceProvider.GetService<IPublisher>();
        if (publisher is null)
        {
            throw new InvalidOperationException(
                "IPublisher is not registered. Register MediatR (RegisterMediatR / AddMediatR) " +
                "before using the in-process outbox transport.");
        }

        var tenantScope = scope.ServiceProvider.GetService<QFace.Sdk.Temporal.Interceptors.ITenantScopeSetter>();
        var userContext = scope.ServiceProvider.GetService<ICurrentUserService>();
        tenantScope?.SetTenantScope(envelope.TenantId);
        userContext?.SetContext(envelope.TenantId, "outbox-relay@system", "Outbox Relay", "system");
        try
        {
            var notification = ResolveNotification(scope.ServiceProvider, envelope);
            await publisher.Publish(notification, cancellationToken);
        }
        finally
        {
            userContext?.ClearContext();
            tenantScope?.ClearTenantScope();
        }

        logger.LogDebug(
            "In-process outbox delivery. EventType={EventType}, OutboxId={OutboxId}, EntityId={EntityId}",
            envelope.EventType, envelope.OutboxId, envelope.EntityId);
    }

    private static INotification ResolveNotification(IServiceProvider sp, EventEnvelope envelope)
    {
        var resolver = sp.GetService<IOutboxEventTypeResolver>();
        var clrType = resolver?.Resolve(envelope.EventType) ?? FindNotificationType(envelope.EventType);
        if (clrType is null)
        {
            throw new InvalidOperationException(
                $"No CLR type registered for outbox EventType '{envelope.EventType}'. " +
                "Register types with AddOutboxEventType<T>() or IOutboxEventTypeResolver.");
        }

        var instance = JsonSerializer.Deserialize(envelope.Payload, clrType, JsonOptions)
                       ?? throw new InvalidOperationException(
                           $"Deserialized null for outbox EventType '{envelope.EventType}'.");

        if (instance is not INotification notification)
        {
            throw new InvalidOperationException(
                $"Outbox EventType '{envelope.EventType}' deserialized to {clrType.Name}, " +
                "which is not an INotification / IDomainEvent.");
        }

        return notification;
    }

    private static Type? FindNotificationType(string eventType)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
            }
            catch
            {
                continue;
            }

            var match = types.FirstOrDefault(t =>
                t is { IsClass: true, IsAbstract: false }
                && typeof(INotification).IsAssignableFrom(t)
                && string.Equals(t.Name, eventType, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
                return match;
        }

        return null;
    }
}

/// <summary>Maps outbox EventType names to CLR types for in-process deserialization.</summary>
public interface IOutboxEventTypeResolver
{
    Type? Resolve(string eventType);
}

/// <summary>Simple name→type map populated via DI registration helpers.</summary>
public sealed class OutboxEventTypeResolver : IOutboxEventTypeResolver
{
    private readonly Dictionary<string, Type> _map;

    public OutboxEventTypeResolver(IEnumerable<OutboxEventTypeRegistration> registrations)
    {
        _map = registrations
            .GroupBy(r => r.EventType, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last().ClrType, StringComparer.OrdinalIgnoreCase);
    }

    public Type? Resolve(string eventType) =>
        _map.TryGetValue(eventType, out var type) ? type : null;
}

public sealed record OutboxEventTypeRegistration(string EventType, Type ClrType);
