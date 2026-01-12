namespace QimErp.Shared.Common.Services.Workflow;

public class WorkflowService(
    IWorkflowConfigCacheService configCacheService,
    ILogger<WorkflowService> logger,
    ICurrentUserService? currentUserService = null)
    : IWorkflowService
{
    public async Task<bool> ShouldTriggerWorkflow(IWorkflowEnabled entity, string operation, string? module = null)
    {
        try
        {
            string entityType = entity.EntityType;
            
            // If module is not provided, cannot check workflow configuration
            if (string.IsNullOrWhiteSpace(module))
            {
                logger.LogDebug("Module not provided for ShouldTriggerWorkflow. Skipping workflow configuration check for {EntityType}", entityType);
                return false;
            }

            if (!await configCacheService.IsWorkflowEnabledAsync(module, entityType, operation))
                return false;

            List<WorkflowTriggerCondition> conditions = await configCacheService.GetTriggerConditionsAsync(module, entityType, operation);
            if (!conditions.Any())
                return true;

            return await EvaluateConditions(entity, conditions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error evaluating workflow trigger for Module: {Module}, EntityType: {EntityType}, Operation: {Operation}", module, entity.EntityType, operation);
            return false;
        }
    }

    public async Task InitiateWorkflowAsync(IWorkflowEnabled entity, string operation, WorkflowDefinition? workflowDefinition = null)
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
                return;
            }

            string currentUser = currentUserService?.GetUserId() ?? "system";
            string currentUserEmail = currentUserService?.GetUserEmail() ?? "system@qimerp.com";
            string currentUserName = currentUserService?.GetUserName() ?? "System";
            Guid workflowHistoryId = Guid.NewGuid();

            logger.LogDebug("Setting CurrentWorkflowHistoryId={WorkflowHistoryId} for {EntityType} before initiating workflow", 
                workflowHistoryId, entity.EntityType);
            
            entity.CurrentWorkflowHistoryId = workflowHistoryId;
            
            // Set workflow properties if workflowDefinition is provided
            if (workflowDefinition != null)
            {
                logger.LogDebug("Setting workflow properties on {EntityType}. WorkflowCode={WorkflowCode}, StepsCount={StepsCount}", 
                    entity.EntityType, workflowCode, workflowDefinition.Steps.Count);
                
                entity.WorkflowCode = workflowCode;
                entity.WorkflowDefinition = workflowDefinition;
                
                var firstStep = workflowDefinition.Steps.MinBy(s => s.Order);
                
                if (firstStep != null)
                {
                    entity.CurrentWorkflowState = firstStep.StepCode;
                    logger.LogDebug("Set CurrentWorkflowState to first step: {StepCode} for {EntityType}", 
                        firstStep.StepCode, entity.EntityType);
                }
                else
                {
                    logger.LogWarning("WorkflowDefinition has no steps for {EntityType} WorkflowCode={WorkflowCode}", 
                        entity.EntityType, workflowCode);
                }
            }
            else
            {
                // Even without workflowDefinition, set the WorkflowCode if we have it
                if (!string.IsNullOrWhiteSpace(workflowCode))
                {
                    entity.WorkflowCode = workflowCode;
                    logger.LogDebug("Set WorkflowCode={WorkflowCode} on {EntityType} without WorkflowDefinition", 
                        workflowCode, entity.EntityType);
                }
            }
            
            logger.LogDebug("Calling InitiateWorkflow extension method for {EntityType}. User={UserEmail}, Operation={Operation}", 
                entity.EntityType, currentUserEmail, operation);
            
            entity.InitiateWorkflow(currentUserEmail, currentUser, currentUserName, $"Workflow initiated for {operation}");

            logger.LogInformation("Successfully initiated workflow {WorkflowCode} for {EntityType} {EntityId}. WorkflowStatus={WorkflowStatus}, CurrentWorkflowState={CurrentWorkflowState}, CurrentWorkflowHistoryId={WorkflowHistoryId}",
                workflowCode, entity.EntityType, entity.GetType().GetProperty("Id")?.GetValue(entity), entity.WorkflowStatus, entity.CurrentWorkflowState, entity.CurrentWorkflowHistoryId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initiating workflow for {EntityType} {Operation}. WorkflowCode={WorkflowCode}", 
                entity.EntityType, operation, entity.WorkflowCode);
            throw;
        }
    }
    

    public async Task UpdateWorkflowStateAsync(Guid workflowHistoryId, string newState, string? comments = null)
    {
        try
        {
            string currentUser = currentUserService?.GetUserId() ?? "system";

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
    }

    public async Task CompleteWorkflowAsync(Guid workflowHistoryId, WorkflowStatus finalStatus, string completedByEmail, string? completedByEmployeeId = null, string? completedByName = null, string? comments = null)
    {
        try
        {
            // TODO: Update workflow history in database
            // TODO: Log completion activity
            
            logger.LogInformation("Completed workflow {WorkflowHistoryId} with status {FinalStatus} by {CompletedByEmail}", 
                workflowHistoryId, finalStatus, completedByEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing workflow {WorkflowHistoryId}", workflowHistoryId);
            throw;
        }
    }

    private async Task<bool> EvaluateConditions(IWorkflowEnabled entity, List<WorkflowTriggerCondition> conditions)
    {
        foreach (WorkflowTriggerCondition condition in conditions)
        {
            if (!EvaluateCondition(entity, condition))
                return false;
        }
        return true;
    }

    private bool EvaluateCondition(IWorkflowEnabled entity, WorkflowTriggerCondition condition)
    {
        Type entityType = entity.GetType();
        PropertyInfo? property = entityType.GetProperty(condition.Field);
        
        if (property == null)
        {
            logger.LogWarning("Property {PropertyName} not found on {EntityType}", condition.Field, entityType.Name);
            return false;
        }

        object? actualValue = property.GetValue(entity);
        return EvaluateValue(actualValue, condition.Operator, condition.Value);
    }

    private bool EvaluateValue(object? actualValue, WorkflowOperators @operator, object expectedValue)
    {
        return @operator switch
        {
            WorkflowOperators.Equals => Equals(actualValue, expectedValue),
            WorkflowOperators.NotEquals => !Equals(actualValue, expectedValue),
            WorkflowOperators.GreaterThan => CompareValues(actualValue, expectedValue) > 0,
            WorkflowOperators.LessThan => CompareValues(actualValue, expectedValue) < 0,
            WorkflowOperators.GreaterThanOrEqual => CompareValues(actualValue, expectedValue) >= 0,
            WorkflowOperators.LessThanOrEqual => CompareValues(actualValue, expectedValue) <= 0,
            WorkflowOperators.Contains => actualValue?.ToString()?.Contains(expectedValue?.ToString() ?? "") == true,
            WorkflowOperators.StartsWith => actualValue?.ToString()?.StartsWith(expectedValue?.ToString() ?? "") == true,
            WorkflowOperators.EndsWith => actualValue?.ToString()?.EndsWith(expectedValue?.ToString() ?? "") == true,
            _ => false
        };
    }




    private int CompareValues(object? actualValue, object expectedValue)
    {
        if (actualValue is IComparable comparable && expectedValue is IComparable)
        {
            return comparable.CompareTo(expectedValue);
        }
        return 0;
    }
}

// Extension methods for IWorkflowEnabled interface

