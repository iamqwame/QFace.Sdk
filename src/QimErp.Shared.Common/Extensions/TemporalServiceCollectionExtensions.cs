using Microsoft.Extensions.DependencyInjection.Extensions;
using QimErp.Shared.Common.Services.Workflow;
using QimErp.Shared.Common.Services.Workflow.Temporal;

namespace QimErp.Shared.Common.Extensions;

public static class TemporalServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Temporal client, the IWorkflowTriggerBridge implementation,
    /// and the module activity registry as singletons.
    ///
    /// Call explicitly in any module WebApi that opts into Temporal:
    ///   services.AddTemporalWorkflow(configuration);
    ///
    /// Or use the AddDbContextWithOutboxAndTemporal convenience wrapper.
    /// Note: AddDbContextWithOutbox does NOT call this automatically — you must opt in.
    ///
    /// Safe to call multiple times — uses TryAdd semantics.
    /// No-op when "Temporal:Address" is absent.
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

        // Registry — populated on startup by ModuleApprovalActivityRegistryStartup (IHostedService)
        services.TryAddSingleton<IModuleApprovalActivityRegistry, ModuleApprovalActivityRegistry>();

        return services;
    }

    /// <summary>
    /// Registers a module's IModuleApprovalActivity implementation and maps it to
    /// one or more entity type names.
    ///
    /// Call once per module in the module's Consumer/Worker Program.cs:
    ///
    ///   services.AddModuleApprovalActivity&lt;HrApprovalActivity&gt;(
    ///       "Employee", "Department", "Rank");
    /// </summary>
    public static IServiceCollection AddModuleApprovalActivity<TActivity>(
        this IServiceCollection services,
        params string[] entityTypes)
        where TActivity : class, IModuleApprovalActivity
    {
        services.AddScoped<TActivity>();
        services.AddSingleton<ModuleApprovalActivityRegistration>(
            _ => new ModuleApprovalActivityRegistration(entityTypes, typeof(TActivity)));
        return services;
    }
}
