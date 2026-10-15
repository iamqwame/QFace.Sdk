using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace QimErp.Shared.Common.Diagnostics.IntegrityChecks;

/// <summary>
/// Opt-in DI wiring for DATA-06 integrity checks. Neither helper is called by
/// <c>AddDbContextWithOutbox</c> — a module must call both explicitly to get checks running.
/// </summary>
public static class IntegrityCheckServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TCheck"/> as an <see cref="IIntegrityCheck"/>. Call once per
    /// check; <see cref="IntegrityCheckRunnerBackgroundService"/> picks up every registration.
    /// </summary>
    public static IServiceCollection AddIntegrityCheck<TCheck>(this IServiceCollection services)
        where TCheck : class, IIntegrityCheck
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIntegrityCheck, TCheck>());
        return services;
    }

    /// <summary>
    /// Registers the <see cref="IntegrityCheckRunnerBackgroundService"/> hosted service that runs
    /// every <see cref="IIntegrityCheck"/> registered via <see cref="AddIntegrityCheck{TCheck}"/>.
    /// </summary>
    /// <param name="configuration">Optional. When provided, options bind from the "IntegrityCheckRunner" section; when null, default values are used.</param>
    public static IServiceCollection AddIntegrityCheckRunner(
        this IServiceCollection services, IConfiguration? configuration = null)
    {
        if (configuration != null)
            services.AddOptions<IntegrityCheckRunnerOptions>()
                .Bind(configuration.GetSection(IntegrityCheckRunnerOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        else
            services.AddOptions<IntegrityCheckRunnerOptions>()
                .Configure(_ => { })
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, IntegrityCheckRunnerBackgroundService>());

        return services;
    }
}
