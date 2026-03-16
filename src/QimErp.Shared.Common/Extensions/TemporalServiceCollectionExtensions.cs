using Microsoft.Extensions.DependencyInjection.Extensions;
using QimErp.Shared.Common.Services.Workflow;
using QimErp.Shared.Common.Services.Workflow.Temporal;
using Temporalio.Extensions.Hosting;

namespace QimErp.Shared.Common.Extensions;

public static class TemporalServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Temporal client and the IWorkflowTriggerBridge implementation.
    ///
    /// Call explicitly in any module WebApi that opts into Temporal:
    ///   services.AddTemporalWorkflow(configuration);
    ///
    /// Safe to call multiple times — uses TryAdd semantics.
    /// No-op when "Temporal:Address" is absent (falls back to actor path).
    /// </summary>
    public static IServiceCollection AddTemporalWorkflow(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var address = configuration["Temporal:Address"];
        var ns      = configuration["Temporal:Namespace"] ?? TemporalConstants.DefaultNamespace;

        if (string.IsNullOrWhiteSpace(address))
            return services; // Temporal not configured — fall through to actor path

        // Lazy connection via AddTemporalClient — does not block the startup thread
        services.AddTemporalClient(opts =>
        {
            opts.TargetHost = address;
            opts.Namespace  = ns;
        });

        // Bridge — replaces WorkflowEventPublisherActor path in the interceptor
        services.TryAddSingleton<IWorkflowTriggerBridge, TemporalWorkflowTriggerBridge>();

        return services;
    }

    /// <summary>
    /// Registers a module's IModuleApprovalActivity implementation AND starts a
    /// Temporal worker that polls the module's dedicated task queue.
    ///
    /// Each module Consumer must call this once in Program.cs:
    ///
    ///   services.AddModuleApprovalActivity&lt;PayrollApprovalActivity&gt;(
    ///       context.Configuration, "Payroll",
    ///       "PayrollRun", "SalaryAdjustment", "BonusRequest");
    ///
    /// The <paramref name="module"/> name MUST exactly match the value of
    /// ApprovalWorkflowInput.Module for the entity types handled by this consumer.
    /// The Temporal worker polls "qimerp-{module}-approvals".
    ///
    /// When "Temporal:Address" is not configured, this is a safe no-op —
    /// the activity class is still registered in DI for any direct usage,
    /// but no Temporal worker is started.
    /// </summary>
    public static IServiceCollection AddModuleApprovalActivity<TActivity>(
        this IServiceCollection services,
        IConfiguration configuration,
        string module,
        params string[] entityTypes)
        where TActivity : class, IModuleApprovalActivity
    {
        // Always register the activity class in DI for direct resolution if needed
        services.TryAddScoped<TActivity>();

        var address   = configuration["Temporal:Address"];
        var ns        = configuration["Temporal:Namespace"] ?? TemporalConstants.DefaultNamespace;
        var taskQueue = TemporalConstants.ModuleTaskQueue(module);

        if (string.IsNullOrWhiteSpace(address))
            return services; // Temporal not configured — no worker needed

        // Start a dedicated Temporal worker for this module.
        // Polls qimerp-{module}-approvals, so activity type name collisions between
        // modules (all implement the same IModuleApprovalActivity methods) are impossible.
        services.AddHostedTemporalWorker(address, ns, taskQueue)
            .AddScopedActivities<TActivity>();

        return services;
    }
}
