using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QFace.Sdk.Temporal.Extensions;
using QimErp.Shared.Common.Services.Notifications;
using QimErp.Shared.Common.Services.Workflow;
using QimErp.Shared.Common.Services.Workflow.Temporal;
using QimErp.Shared.Common.Services.Workflow.Temporal.Approval;

namespace QimErp.Shared.Common.Extensions;

public static class TemporalServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Temporal client, all SDK generic abstractions, and all
    /// QimErp approval-specific domain wrappers.
    ///
    /// Called automatically from AddDbContextWithOutboxAndTemporal when
    /// "Temporal:Address" is present. Safe to call explicitly when finer control
    /// is needed (e.g. WebApi that signals but does not host a worker).
    ///
    /// Registers (all TryAdd — safe to call multiple times):
    ///   SDK layer (QFace.Sdk.Temporal):
    ///     ITemporalClient, IWorkflowStarter, IWorkflowSignaller,
    ///     IWorkflowQueryClient, IWorkflowTerminator
    ///   Domain layer (QimErp.Shared.Common):
    ///     IApprovalWorkflowStarter, IApprovalWorkflowSignaller,
    ///     IApprovalWorkflowQueryClient, IApprovalWorkflowTerminator
    ///     INotificationWorkflowStarter
    /// </summary>
    public static IServiceCollection AddTemporalWorkflow(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var address = configuration["Temporal:Address"];
        if (string.IsNullOrWhiteSpace(address))
            return services; // Temporal not configured — skip entirely, actor fallback remains

        // SDK layer — client + generic abstractions
        services.AddTemporalClient(configuration);

        // Domain layer — typed approval wrappers
        services.TryAddSingleton<IApprovalWorkflowStarter,     ApprovalWorkflowStarter>();
        services.TryAddSingleton<IApprovalWorkflowSignaller,   ApprovalWorkflowSignaller>();
        services.TryAddSingleton<IApprovalWorkflowQueryClient, ApprovalWorkflowQueryClient>();
        services.TryAddSingleton<IApprovalWorkflowTerminator,  ApprovalWorkflowTerminator>();

        // Bridge — replaces WorkflowEventPublisherActor in the interceptor
        services.TryAddSingleton<IWorkflowTriggerBridge, TemporalWorkflowTriggerBridge>();

        // Notification starter — any service injects INotificationWorkflowStarter to fire emails/SMS
        services.TryAddSingleton<INotificationWorkflowStarter, NotificationWorkflowStarter>();

        return services;
    }

    /// <summary>
    /// Registers a module's IModuleApprovalActivity implementation for the given entity types.
    /// Call once per module from that module's Program.cs:
    ///
    ///   services.AddModuleApprovalActivity&lt;HrApprovalActivity&gt;(
    ///       "Employee", "Department", "Rank", "Station");
    ///
    /// Registers the activity as Scoped (Temporal executes each activity invocation
    /// in a new scope). Also registers a ModuleApprovalActivityRegistration singleton
    /// so the module approval activity registry can be populated at startup.
    /// </summary>
    public static IServiceCollection AddModuleApprovalActivity<TActivity>(
        this IServiceCollection services,
        params string[] entityTypes)
        where TActivity : class, IModuleApprovalActivity
    {
        // Scoped — Temporal worker creates a new scope per activity execution
        services.AddScoped<TActivity>();

        // Registration marker for IModuleApprovalActivityRegistry
        services.AddSingleton<ModuleApprovalActivityRegistration>(
            _ => new ModuleApprovalActivityRegistration(entityTypes, typeof(TActivity)));

        return services;
    }
}
