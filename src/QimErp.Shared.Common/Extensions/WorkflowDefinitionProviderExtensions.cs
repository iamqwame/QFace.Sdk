using QimErp.Shared.Common.Services.Workflow;

namespace QimErp.Shared.Common.Extensions;

public static class WorkflowDefinitionProviderExtensions
{
    /// <summary>
    /// Registers cache-only workflow definition read path plus cache writer for publish operations.
    /// </summary>
    public static IServiceCollection AddWorkflowDefinitionProvider(this IServiceCollection services)
    {
        services.AddScoped<IWorkflowDefinitionCacheWriter, WorkflowDefinitionCacheWriter>();
        services.AddScoped<IWorkflowDefinitionProvider, WorkflowDefinitionProvider>();
        return services;
    }
}
