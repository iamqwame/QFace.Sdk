using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Temporalio.Client;
using Temporalio.Extensions.Hosting;
using QFace.Sdk.Temporal.Abstractions;
using QFace.Sdk.Temporal.HealthChecks;
using QFace.Sdk.Temporal.Helpers;
using QFace.Sdk.Temporal.Implementations;
using QFace.Sdk.Temporal.Options;

namespace QFace.Sdk.Temporal.Extensions;

public static class TemporalClientServiceCollectionExtensions
{
    /// <summary>
    /// Registers ITemporalClient as a singleton using the "Temporal" config section.
    /// Applies TLS automatically when EnableTls=true or when Address is non-localhost.
    /// Validates TemporalOptions on startup — host refuses to start if Address is empty.
    /// Registers IWorkflowStarter, IWorkflowSignaller, IWorkflowQueryClient, IWorkflowTerminator.
    /// Configures HostOptions.BackgroundServiceExceptionBehavior from WorkerFailureBehavior.
    /// Safe to call multiple times — TryAdd semantics throughout.
    /// </summary>
    public static IServiceCollection AddTemporalClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<TemporalOptions>()
            .Bind(configuration.GetSection(TemporalOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services.RegisterTemporalClient(sp =>
        {
            var opts = configuration
                .GetSection(TemporalOptions.SectionName)
                .Get<TemporalOptions>() ?? new TemporalOptions();
            return opts;
        });
    }

    /// <summary>
    /// Registers ITemporalClient using an explicit configure action.
    /// Use for tests or apps without IConfiguration.
    /// </summary>
    public static IServiceCollection AddTemporalClient(
        this IServiceCollection services,
        Action<TemporalOptions> configure)
    {
        var opts = new TemporalOptions();
        configure(opts);

        services.AddOptions<TemporalOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services.RegisterTemporalClient(_ => opts);
    }

    // ── private ───────────────────────────────────────────────────────────────

    private static IServiceCollection RegisterTemporalClient(
        this IServiceCollection services,
        Func<IServiceProvider, TemporalOptions> optionsFactory)
    {
        // ITemporalClient — one connection per process
        services.TryAddSingleton<ITemporalClient>(sp =>
        {
            var opts   = optionsFactory(sp);
            var logger = sp.GetRequiredService<ILogger<ITemporalClient>>();

            // Prefer config; fall back to TEMPORAL_* env vars (Temporal Cloud convention).
            var address = (opts.Address ?? Environment.GetEnvironmentVariable("TEMPORAL_ADDRESS") ?? "localhost:7233").Trim();
            var @namespace = (opts.Namespace ?? Environment.GetEnvironmentVariable("TEMPORAL_NAMESPACE") ?? "default").Trim();
            var apiKey = !string.IsNullOrWhiteSpace(opts.ApiKey)
                ? opts.ApiKey.Trim()
                : Environment.GetEnvironmentVariable("TEMPORAL_API_KEY")?.Trim();

            // Auto-enable TLS for non-local addresses or when API key is set (Temporal Cloud).
            var useTls = opts.EnableTls || !string.IsNullOrEmpty(apiKey) || !TemporalNaming.IsLocalAddress(address);

            logger.LogInformation(
                "[QFace.Sdk.Temporal] Connecting. Address={Address}, Namespace={Namespace}, TLS={UseTls}, ApiKey={HasApiKey}",
                address, @namespace, useTls, !string.IsNullOrEmpty(apiKey));

            var connectOptions = new TemporalClientConnectOptions
            {
                TargetHost = address,
                Namespace  = @namespace
            };

            if (!string.IsNullOrEmpty(apiKey))
                connectOptions.ApiKey = apiKey;

            if (useTls)
            {
                var tlsOptions = new TlsOptions();

                if (opts.Tls != null)
                {
                    // Custom CA certificate
                    if (!string.IsNullOrWhiteSpace(opts.Tls.ServerRootCaCert))
                        tlsOptions.ServerRootCACert =
                            System.Text.Encoding.UTF8.GetBytes(opts.Tls.ServerRootCaCert);

                    // SNI domain override — default to host from address
                    tlsOptions.Domain = !string.IsNullOrWhiteSpace(opts.Tls.Domain)
                        ? opts.Tls.Domain
                        : TemporalNaming.ExtractHost(address);

                    // mTLS client certificate
                    if (!string.IsNullOrWhiteSpace(opts.Tls.ClientCert) &&
                        !string.IsNullOrWhiteSpace(opts.Tls.ClientPrivateKey))
                    {
                        tlsOptions.ClientCert =
                            System.Text.Encoding.UTF8.GetBytes(opts.Tls.ClientCert);
                        tlsOptions.ClientPrivateKey =
                            System.Text.Encoding.UTF8.GetBytes(opts.Tls.ClientPrivateKey);
                    }
                }
                else
                {
                    // Default TLS — system CA, SNI from address host
                    tlsOptions.Domain = TemporalNaming.ExtractHost(address);
                }

                connectOptions.Tls = tlsOptions;
            }

            return TemporalClient.ConnectAsync(connectOptions).GetAwaiter().GetResult();
        });

        // Generic workflow operation abstractions
        services.TryAddSingleton<IWorkflowStarter,     WorkflowStarter>();
        services.TryAddSingleton<IWorkflowSignaller,   WorkflowSignaller>();
        services.TryAddSingleton<IWorkflowQueryClient, WorkflowQueryClient>();
        services.TryAddSingleton<IWorkflowTerminator,  WorkflowTerminator>();

        // WorkerFailureBehavior — configure host behaviour when worker background service fails
        services.Configure<HostOptions>(hostOpts =>
        {
            // We read TemporalOptions again from the service provider at configure time.
            // We can't inject IOptions<TemporalOptions> here directly because this runs
            // during service collection build — use a post-configure hook instead.
        });

        // Post-configure HostOptions after TemporalOptions is bound
        services.AddOptions<HostOptions>()
            .Configure<IOptions<TemporalOptions>>((hostOpts, temporalOpts) =>
            {
                var behavior = temporalOpts.Value.WorkerFailureBehavior;
                hostOpts.BackgroundServiceExceptionBehavior = behavior == WorkerFailureBehavior.ContinueWithUnhealthy
                    ? BackgroundServiceExceptionBehavior.Ignore
                    : BackgroundServiceExceptionBehavior.StopHost;
            });

        return services;
    }
}
