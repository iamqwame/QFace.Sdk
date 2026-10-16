using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Outbox.Transports;

namespace QimErp.Shared.Common.Outbox;

public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// Registers durable outbox (<see cref="IOutbox"/>), store, transports, and Akka relay.
    /// Called from <c>AddDbContextWithOutbox*</c>.
    /// </summary>
    public static IServiceCollection AddTransactionalOutbox<TContext>(
        this IServiceCollection services,
        IConfiguration? configuration = null)
        where TContext : DbContext
    {
        if (configuration is not null)
            services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        else
            services.Configure<OutboxOptions>(_ => { });

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ITransportRouting, TransportRouting>();

        services.TryAddScoped<OutboxStore>(sp =>
            new OutboxStore(sp.GetRequiredService<TContext>(), sp.GetRequiredService<ILogger<OutboxStore>>()));

        services.TryAddScoped<IOutbox>(sp =>
            new EfOutbox(
                sp.GetRequiredService<TContext>(),
                sp.GetRequiredService<ITransportRouting>(),
                sp.GetRequiredService<TimeProvider>()));

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IEventTransport, InProcessEventTransport>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IEventTransport, TemporalEventTransport>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IEventTransport, UnconfiguredRabbitMqTransport>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IEventTransport, UnconfiguredKafkaTransport>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IOutboxTemporalHandler, WorkflowApprovalOutboxTemporalHandler>());

        services.TryAddSingleton<IOutboxEventTypeResolver>(sp =>
            new OutboxEventTypeResolver(sp.GetServices<OutboxEventTypeRegistration>()));

        if (!services.Any(d => d.ImplementationType == typeof(OutboxRelayHostedService)))
            services.AddHostedService<OutboxRelayHostedService>();

        return services;
    }

    /// <summary>
    /// Registers a CLR type so the in-process transport can deserialize outbox payloads.
    /// </summary>
    public static IServiceCollection AddOutboxEventType<TEvent>(this IServiceCollection services)
        where TEvent : class
    {
        services.AddSingleton(new OutboxEventTypeRegistration(typeof(TEvent).Name, typeof(TEvent)));
        return services;
    }

    /// <summary>
    /// Registers a CLR type under an explicit EventType name.
    /// </summary>
    public static IServiceCollection AddOutboxEventType<TEvent>(this IServiceCollection services, string eventType)
        where TEvent : class
    {
        services.AddSingleton(new OutboxEventTypeRegistration(eventType, typeof(TEvent)));
        return services;
    }
}
