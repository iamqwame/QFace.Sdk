namespace QimErp.Shared.Common.Workflow.Entities;

public class WorkflowConfiguration: GuidAuditableEntity
{
    /// <summary>
    /// Module/Microservice identifier (e.g., "HR", "Recruitment", "Payroll")
    /// Required to prevent EntityType conflicts across different modules
    /// </summary>
    public string Module { get; set; } = "";
    
    /// <summary>
    /// Entity type this configuration applies to (e.g., "Employee", "JobRequisition", "PurchaseOrder")
    /// </summary>
    public string EntityType { get; set; } = ""; // Employee, PurchaseOrder, etc.
    public EntityWorkflowConfig Configuration { get; set; } = new();
    public int Version { get; set; } = 1;
    public string? Description { get; set; }
}

public class EntityWorkflowConfig
{
    public bool EnableWorkflowForCreate { get; set; } = false;
    public bool EnableWorkflowForUpdate { get; set; } = false;
    public bool EnableWorkflowForDelete { get; set; } = false;
    
    public string? CreateWorkflowCode { get; set; }
    public List<WorkflowOperationRoute> UpdateWorkflowRoutes { get; set; } = [];
    public List<WorkflowOperationRoute> DeleteWorkflowRoutes { get; set; } = [];
    
    public List<WorkflowTriggerCondition> CreateTriggerConditions { get; set; } = [];
    
    public bool AutoSubmitOnCreate { get; set; } = true;
    public bool PreventDirectSaveOnCreate { get; set; } = true;
    public decimal? AmountThreshold { get; set; }
    public List<string> ExcludeRoles { get; set; } = [];
    public List<string> ExcludeUsers { get; set; } = [];

    public IEnumerable<string> GetAllWorkflowCodes()
    {
        if (!string.IsNullOrWhiteSpace(CreateWorkflowCode))
            yield return CreateWorkflowCode;

        foreach (var route in UpdateWorkflowRoutes)
        {
            if (!string.IsNullOrWhiteSpace(route.WorkflowCode))
                yield return route.WorkflowCode;
        }

        foreach (var route in DeleteWorkflowRoutes)
        {
            if (!string.IsNullOrWhiteSpace(route.WorkflowCode))
                yield return route.WorkflowCode;
        }
    }
}

public class WorkflowOperationRoute
{
    public string WorkflowCode { get; set; } = "";
    public string? Name { get; set; }
    public int Priority { get; set; }
    public List<string> SignificantFields { get; set; } = [];
    public List<WorkflowTriggerCondition> Conditions { get; set; } = [];
}

public class WorkflowTriggerCondition
{
    public string Field { get; set; } = "";
    public WorkflowOperators Operator { get; set; } = WorkflowOperators.Equals;
    public string Value { get; set; } = "";
    public string? Description { get; set; }
}
