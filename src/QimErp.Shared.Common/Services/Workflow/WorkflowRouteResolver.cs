using QimErp.Shared.Common.Workflow.Entities;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Resolves the first matching update/delete workflow route from tenant configuration.
/// No match means no workflow — there is no fallback code.
/// </summary>
public static class WorkflowRouteResolver
{
    public static WorkflowOperationRoute? ResolveUpdateRoute(
        EntityWorkflowConfig config,
        IReadOnlySet<string> changedFields,
        IWorkflowEnabled entity)
    {
        if (!config.EnableWorkflowForUpdate || config.UpdateWorkflowRoutes.Count == 0)
            return null;

        return ResolveRoute(config.UpdateWorkflowRoutes, changedFields, entity);
    }

    public static WorkflowOperationRoute? ResolveDeleteRoute(
        EntityWorkflowConfig config,
        IReadOnlySet<string> changedFields,
        IWorkflowEnabled entity)
    {
        if (!config.EnableWorkflowForDelete || config.DeleteWorkflowRoutes.Count == 0)
            return null;

        return ResolveRoute(config.DeleteWorkflowRoutes, changedFields, entity);
    }

    private static WorkflowOperationRoute? ResolveRoute(
        IEnumerable<WorkflowOperationRoute> routes,
        IReadOnlySet<string> changedFields,
        IWorkflowEnabled entity)
    {
        foreach (var route in routes.OrderBy(r => r.Priority))
        {
            if (!RouteMatches(route, changedFields, entity))
                continue;

            return route;
        }

        return null;
    }

    private static bool RouteMatches(
        WorkflowOperationRoute route,
        IReadOnlySet<string> changedFields,
        IWorkflowEnabled entity)
    {
        if (route.SignificantFields.Count > 0)
        {
            var hasFieldOverlap = route.SignificantFields.Any(field =>
                changedFields.Contains(field));

            if (!hasFieldOverlap)
                return false;
        }

        return WorkflowTriggerConditionEvaluator.EvaluateAll(entity, route.Conditions);
    }
}
