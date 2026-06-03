using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Temporalio.Common;
using Temporalio.Extensions.Hosting;
using Temporalio.Worker;
using QFace.Sdk.Temporal.Options;

namespace QFace.Sdk.Temporal.Extensions;

public static class TemporalWorkerServiceCollectionExtensions
{
    /// <summary>
    /// Registers a hosted Temporal worker using the "Temporal" config section.
    /// Task queue comes from TemporalOptions.TaskQueue.
    /// Uses the already-registered ITemporalClient — no second connection opened.
    /// Applies WorkerDeploymentOptions from UseWorkerVersioning + BuildId in options.
    /// Applies MaxConcurrentActivityExecutions / MaxConcurrentWorkflowTaskExecutions if set.
    ///
    /// Returns ITemporalWorkerServiceOptionsBuilder for chaining:
    ///   .AddWorkflow&lt;TWorkflow&gt;()
    ///   .AddScopedActivities&lt;TActivity&gt;()
    ///   .AddSingletonActivities&lt;TActivity&gt;()
    /// </summary>
    public static ITemporalWorkerServiceOptionsBuilder AddTemporalWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var opts = ReadOptions(configuration);
        if (string.IsNullOrWhiteSpace(opts.TaskQueue))
            throw new InvalidOperationException(
                "Temporal:TaskQueue must be set in configuration when calling " +
                "AddTemporalWorker(configuration) without an explicit taskQueue parameter.");

        // Suffix applied here so callers see the convention enforced once, in the SDK,
        // rather than at every call site.
        return services.BuildWorker(opts, opts.WithTaskQueueSuffix(opts.TaskQueue));
    }

    /// <summary>
    /// Registers a hosted Temporal worker using the "Temporal" config section
    /// with the task queue overridden at call site.
    /// Use when the app hosts multiple workers on different queues,
    /// or when the task queue is known at code-time rather than config-time.
    /// </summary>
    public static ITemporalWorkerServiceOptionsBuilder AddTemporalWorker(
        this IServiceCollection services,
        IConfiguration configuration,
        string taskQueue)
    {
        if (string.IsNullOrWhiteSpace(taskQueue))
            throw new ArgumentException("taskQueue must not be empty.", nameof(taskQueue));

        var opts = ReadOptions(configuration);
        // Pass the BASE queue name; the SDK appends Temporal:TaskQueueSuffix uniformly
        // so every deployment routes to its own workers.
        return services.BuildWorker(opts, opts.WithTaskQueueSuffix(taskQueue));
    }

    /// <summary>
    /// Registers a hosted Temporal worker using an explicit configure action.
    /// Task queue must be supplied — it cannot come from configuration here.
    /// Use for apps without IConfiguration or for fine-grained control.
    /// </summary>
    public static ITemporalWorkerServiceOptionsBuilder AddTemporalWorker(
        this IServiceCollection services,
        Action<TemporalOptions> configure,
        string taskQueue)
    {
        if (string.IsNullOrWhiteSpace(taskQueue))
            throw new ArgumentException("taskQueue must not be empty.", nameof(taskQueue));

        var opts = new TemporalOptions();
        configure(opts);
        return services.BuildWorker(opts, opts.WithTaskQueueSuffix(taskQueue));
    }

    // ── private ───────────────────────────────────────────────────────────────

    private static ITemporalWorkerServiceOptionsBuilder BuildWorker(
        this IServiceCollection services,
        TemporalOptions opts,
        string taskQueue)
    {
        // WorkerDeploymentOptions — required by Temporalio 1.11+ even when versioning is off.
        // When UseWorkerVersioning=false, tasks are delivered to this worker regardless of version.
        var buildId           = opts.BuildId ?? "unversioned";
        var deploymentVersion = new WorkerDeploymentVersion(taskQueue, buildId);
        var deploymentOptions = new WorkerDeploymentOptions(
            deploymentVersion,
            useWorkerVersioning: opts.UseWorkerVersioning);

        // Build the hosted worker using the registered ITemporalClient (no second connection).
        // AddHostedTemporalWorker(taskQueue, deploymentOptions) — uses injected ITemporalClient.
        var builder = services.AddHostedTemporalWorker(taskQueue, deploymentOptions);

        // Apply concurrency caps and auto-seed ambient tenant context for all activities.
        builder.ConfigureOptions(workerOpts =>
        {
            if (opts.MaxConcurrentActivityExecutions.HasValue)
                workerOpts.MaxConcurrentActivities =
                    opts.MaxConcurrentActivityExecutions.Value;

            if (opts.MaxConcurrentWorkflowTaskExecutions.HasValue)
                workerOpts.MaxConcurrentWorkflowTasks =
                    opts.MaxConcurrentWorkflowTaskExecutions.Value;

            // Automatically seeds ICurrentUserService.SetContext(tenantId) before every
            // activity executes, so AuditEntitySaveChangesInterceptor stamps TenantId on
            // all saved entities without per-activity or per-entity WithTenantId() calls.
            // IServiceScopeFactory is resolved from the worker host — it is always available.
            var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
            workerOpts.Interceptors = [new Interceptors.TenantContextActivityInterceptor(scopeFactory)];
        });

        return builder;
    }

    private static TemporalOptions ReadOptions(IConfiguration configuration)
        => configuration
               .GetSection(TemporalOptions.SectionName)
               .Get<TemporalOptions>()
           ?? new TemporalOptions();
}
