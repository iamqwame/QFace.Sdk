using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services.MultiTenancy;

using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;

namespace QimErp.Shared.Common.Services.Workflow;

public class WorkflowService(
    IWorkflowConfigCacheService configCacheService,
    ITenantModuleAccessService moduleAccess,
    ITenantContext tenantContext,
    IOptions<SystemOptions> systemOptions,
    ILogger<WorkflowService> logger,
    ICurrentUserService? currentUserService = null)
    : IWorkflowService
{
    private readonly SystemOptions _systemOptions = systemOptions.Value;

    public async Task<bool> ShouldTriggerWorkflow(IWorkflowEnabled entity, string operation, string? module = null)
    {
        try
        {
            string entityType = entity.EntityType;
            var tenantId = ResolveTenantId(entity);

            if (!await moduleAccess.IsModuleEnabledAsync(tenantId, ModuleKeys.Workflow))
                return false;

            if (string.IsNullOrWhiteSpace(module))
            {
                logger.LogDebug("Module not provided for ShouldTriggerWorkflow. Skipping workflow configuration check for {EntityType}", entityType);
                return false;
            }

            if (!await configCacheService.IsWorkflowEnabledAsync(module, entityType, operation, tenantId))
                return false;

            List<WorkflowTriggerCondition> conditions =
                await configCacheService.GetTriggerConditionsAsync(module, entityType, operation, tenantId);
            if (!conditions.Any())
                return true;

            return WorkflowTriggerConditionEvaluator.EvaluateAll(entity, conditions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error evaluating workflow trigger for Module: {Module}, EntityType: {EntityType}, Operation: {Operation}", module, entity.EntityType, operation);
            return false;
        }
    }

    public Task InitiateWorkflowAsync(IWorkflowEnabled entity, string operation, WorkflowDefinition? workflowDefinition = null)
    {
        try
        {
            logger.LogDebug("InitiateWorkflowAsync called for {EntityType} {Operation}. WorkflowDefinition provided: {HasDefinition}", 
                entity.EntityType, operation, workflowDefinition != null);
            
            string? workflowCode = null;
            
            // First, try to get workflow code from entity's WorkflowCode property if it's already set
            var entityType = entity.GetType();
            var workflowCodeProperty = entityType.GetProperty("WorkflowCode");
            if (workflowCodeProperty != null)
            {
                workflowCode = workflowCodeProperty.GetValue(entity)?.ToString();
                if (!string.IsNullOrWhiteSpace(workflowCode))
                {
                    logger.LogDebug("Found WorkflowCode={WorkflowCode} on entity {EntityType}", workflowCode, entity.EntityType);
                }
            }
            
            // If workflowDefinition is provided (from EntityWorkflowStep), we should have a workflowCode
            // If not found on entity, we need to get it from config cache (requires module)
            if (workflowCode.IsEmpty() && workflowDefinition != null)
            {
                logger.LogWarning("WorkflowDefinition provided but WorkflowCode not found on entity {EntityType}. Cannot determine WorkflowCode without module.", 
                    entity.EntityType);
            }
            
            // If still no workflowCode, try to get from config cache
            // Note: This requires module which may not be available here
            // The interceptor should set WorkflowCode on entity before calling this method
            if (workflowCode.IsEmpty())
            {
                logger.LogWarning("WorkflowCode not found on entity {EntityType} for {Operation}. Workflow properties may not be set correctly.", 
                    entity.EntityType, operation);
            }
            
            // Validate workflowCode exists before proceeding
            if (workflowCode.IsEmpty())
            {
                logger.LogWarning("No workflow code found for {EntityType} {Operation}. Cannot initiate workflow.", entity.EntityType, operation);
                return Task.CompletedTask;
            }

            string currentUser = currentUserService?.GetUserId() ?? _systemOptions.DefaultUserId;
            string currentUserEmail = currentUserService?.GetUserEmail() ?? _systemOptions.DefaultSystemEmail;
            string currentUserName = currentUserService?.GetUserName() ?? _systemOptions.DefaultUserName;
            Guid workflowHistoryId = Guid.NewGuid();

            logger.LogDebug("Setting CurrentWorkflowHistoryId={WorkflowHistoryId} for {EntityType} before initiating workflow", 
                workflowHistoryId, entity.EntityType);
            
            entity.CurrentWorkflowHistoryId = workflowHistoryId;
            
            if (!string.IsNullOrWhiteSpace(workflowCode))
            {
                entity.WorkflowCode = workflowCode;
            }
            
            logger.LogDebug("Calling InitiateWorkflow extension method for {EntityType}. UserId={UserId}, Operation={Operation}",
                entity.EntityType, currentUser, operation);
            
            entity.InitiateWorkflow(currentUserEmail, currentUser, currentUserName, $"Workflow initiated for {operation}");

            logger.LogInformation("Successfully initiated workflow {WorkflowCode} for {EntityType} {EntityId}. WorkflowStatus={WorkflowStatus}, CurrentWorkflowHistoryId={WorkflowHistoryId}",
                workflowCode, entity.EntityType, entity.GetType().GetProperty("Id")?.GetValue(entity), entity.WorkflowStatus, entity.CurrentWorkflowHistoryId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initiating workflow for {EntityType} {Operation}. WorkflowCode={WorkflowCode}",
                entity.EntityType, operation, entity.WorkflowCode);
            throw;
        }

        return Task.CompletedTask;
    }


    public Task UpdateWorkflowStateAsync(Guid workflowHistoryId, string newState, string? comments = null)
    {
        try
        {
            string currentUser = currentUserService?.GetUserId() ?? _systemOptions.DefaultUserId;

            // TODO: Update workflow history in database
            // TODO: Log activity

            logger.LogInformation("Updated workflow {WorkflowHistoryId} to state {NewState}",
                workflowHistoryId, newState);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating workflow state for {WorkflowHistoryId}", workflowHistoryId);
            throw;
        }

        return Task.CompletedTask;
    }

    public Task CompleteWorkflowAsync(Guid workflowHistoryId, WorkflowStatus finalStatus, string completedByEmail, string? completedByEmployeeId = null, string? completedByName = null, string? comments = null)
    {
        try
        {
            // TODO: Update workflow history in database
            // TODO: Log completion activity

            logger.LogInformation("Completed workflow {WorkflowHistoryId} with status {FinalStatus} by {CompletedByEmployeeId}",
                workflowHistoryId, finalStatus, completedByEmployeeId ?? "unknown");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing workflow {WorkflowHistoryId}", workflowHistoryId);
            throw;
        }

        return Task.CompletedTask;
    }

    private string ResolveTenantId(IWorkflowEnabled entity)
    {
        var tenantId = currentUserService?.GetTenantId();
        if (!string.IsNullOrWhiteSpace(tenantId))
            return tenantId;

        tenantId = tenantContext.TenantId;
        if (!string.IsNullOrWhiteSpace(tenantId))
            return tenantId;

        if (entity is AuditableEntity auditableEntity && !string.IsNullOrWhiteSpace(auditableEntity.TenantId))
            return auditableEntity.TenantId;

        return string.Empty;
    }
}

// Extension methods for IWorkflowEnabled interface

