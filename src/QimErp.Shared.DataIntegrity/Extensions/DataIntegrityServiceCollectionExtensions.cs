using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using QFace.Sdk.Temporal.Extensions;
using QFace.Sdk.Temporal.Options;
using QimErp.Shared.DataIntegrity.Discovery;
using QimErp.Shared.DataIntegrity.Results;
using QimErp.Shared.DataIntegrity.Temporal;
using Temporalio.Extensions.Hosting;

namespace QimErp.Shared.DataIntegrity;

public static class DataIntegrityServiceCollectionExtensions
{
    private const string OutboxMeterName = "QimErp.Shared.Common.Outbox";

    /// <summary>moduleKey: ^[a-z][a-z0-9]*(-[a-z0-9]+)*$, max 40 chars. Throws InvalidOperationException if called twice.</summary>
    public static IServiceCollection AddDataIntegrityChecks(
        this IServiceCollection services,
        IConfiguration configuration,
        string moduleKey,
        Action<DataIntegrityBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configure);
        ArgumentNullException.ThrowIfNull(moduleKey);

        if (!IntegrityNaming.IsValidModuleKey(moduleKey))
        {
            throw new ArgumentException(
                $"Module key '{moduleKey}' must match ^[a-z][a-z0-9]*(-[a-z0-9]+)*$ and be at most {IntegrityNaming.MaxModuleKeyLength} characters.",
                nameof(moduleKey));
        }

        if (services.Any(d => d.ServiceType == typeof(DataIntegrityRegistration)))
            throw new InvalidOperationException("AddDataIntegrityChecks has already been called for this host.");

        var builder = new DataIntegrityBuilder();
        configure(builder);
        if (builder.Contexts.Count == 0 && builder.Checks.Count == 0)
            throw new InvalidOperationException("AddDataIntegrityChecks needs at least one AddContext or AddCheck registration.");

        var contextSet = new DataIntegrityContextSet(moduleKey, builder.Contexts.ToList(), builder.Checks.ToList());
        services.AddSingleton(new DataIntegrityRegistration(moduleKey, contextSet.Contexts.Select(c => c.ContextType).ToList()));
        services.AddSingleton(contextSet);

        services.AddOptions<DataIntegrityOptions>()
            .Bind(configuration.GetSection(DataIntegrityOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        foreach (var check in contextSet.Checks)
            services.TryAddScoped(check);

        services.TryAddSingleton<IntegrityLinkCatalog>();
        services.TryAddSingleton<IntegrityMetrics>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IIntegrityResultSink, LoggingIntegrityResultSink>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IIntegrityResultSink, MetricsIntegrityResultSink>());
        services.ConfigureOpenTelemetryMeterProvider(b => b.AddMeter(IntegrityMetrics.MeterName, OutboxMeterName));

        if (string.IsNullOrWhiteSpace(configuration[$"{TemporalOptions.SectionName}:{nameof(TemporalOptions.Address)}"]))
            return services;

        services.AddHostedService<DataIntegrityScheduleRegistrar>();

        var enabled = !bool.TryParse(configuration[$"{DataIntegrityOptions.SectionName}:{nameof(DataIntegrityOptions.Enabled)}"], out var configured) || configured;
        if (!enabled)
            return services;

        services.TryAddScoped(sp => new DataIntegrityActivities(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<DataIntegrityContextSet>(),
            sp.GetRequiredService<IntegrityLinkCatalog>(),
            sp.GetRequiredService<IOptions<DataIntegrityOptions>>(),
            sp.GetServices<IIntegrityResultSink>(),
            sp.GetRequiredService<IntegrityMetrics>(),
            sp.GetRequiredService<ILogger<DataIntegrityActivities>>()));

        services.AddTemporalWorker(configuration, IntegrityNaming.TaskQueue(moduleKey))
            .AddWorkflow<DataIntegrityWorkflow>()
            .AddScopedActivities<DataIntegrityActivities>()
            .ConfigureOptions(o =>
            {
                o.MaxConcurrentActivities = 1;
                o.MaxConcurrentWorkflowTasks = 2;
            });

        return services;
    }
}
