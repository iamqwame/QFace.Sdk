namespace QimErp.Shared.Common.Services.Workflow;

public interface IWorkflowValidationService
{
    Task<Result> ValidateCanEditAsync(IWorkflowEnabled entity);
    Task<Result> ValidateCanDeleteAsync(IWorkflowEnabled entity);
    Task<Result> ValidateCanCreateAsync(string entityType);
    Task<bool> RequiresApprovalAsync(IWorkflowEnabled entity, string operation);
}

public class WorkflowValidationService(
    IWorkflowConfigCacheService configCacheService,
    IWorkflowService workflowService,
    ICurrentUserService currentUserService,
    ILogger<WorkflowValidationService> logger) : IWorkflowValidationService
{
    public async Task<Result> ValidateCanEditAsync(IWorkflowEnabled entity)
    {
        // Check workflow status
        if (!entity.CanBeEdited())
        {
            return Result.WithFailure(new Error(
                "WorkflowValidation.CannotEdit",
                $"Entity cannot be edited in current workflow status: {entity.WorkflowStatus}"));
        }

        // Check if user has permission to edit
        // Note: Module should be passed from caller, but for backward compatibility we'll skip if not available
        // This method may need to be updated to accept module parameter in the future
        // For now, we'll skip config check if module is not available
        // TODO: Update this method to accept module parameter
        // var config = await configCacheService.GetEntityConfigAsync(module, entity.EntityType);
        // if (config != null)
        // {
        //     var currentUser = currentUserService.GetUserId();
        //     var userRoles = currentUserService.GetUserRoles();
        //
        //     // Check if user is excluded
        //     if (config.ExcludeUsers.Contains(currentUser))
        //     {
        //         return Result.WithFailure(new Error(
        //             "WorkflowValidation.UserExcluded",
        //             "Current user is excluded from editing this entity"));
        //     }
        //
        //     // Check if user role is excluded
        //     if (userRoles.Any(role => config.ExcludeRoles.Contains(role)))
        //     {
        //         return Result.WithFailure(new Error(
        //             "WorkflowValidation.RoleExcluded",
        //             "Current user role is excluded from editing this entity"));
        //     }
        // }

        return Result.WithSuccess();
    }

    public async Task<Result> ValidateCanDeleteAsync(IWorkflowEnabled entity)
    {
        if (!entity.CanBeDeleted())
        {
            return Result.WithFailure(new Error(
                "WorkflowValidation.CannotDelete",
                $"Entity cannot be deleted in current workflow status: {entity.WorkflowStatus}"));
        }

        return Result.WithSuccess();
    }

    public async Task<Result> ValidateCanCreateAsync(string entityType)
    {
        // Note: Module should be passed from caller, but for backward compatibility we'll skip if not available
        // This method may need to be updated to accept module parameter in the future
        // TODO: Update this method to accept module parameter
        // var config = await configCacheService.GetEntityConfigAsync(module, entityType);
        // if (config != null && config.PreventDirectSaveOnCreate)
        // {
        //     return Result.WithFailure(new Error(
        //         "WorkflowValidation.DirectCreatePrevented",
        //         "Direct creation is prevented for this entity type. Use workflow submission."));
        // }

        return Result.WithSuccess();
    }

    public async Task<bool> RequiresApprovalAsync(IWorkflowEnabled entity, string operation)
    {
        // Note: Module should be passed from caller, but for backward compatibility we'll pass null
        // This method may need to be updated to accept module parameter in the future
        return await workflowService.ShouldTriggerWorkflow(entity, operation, null);
    }
}