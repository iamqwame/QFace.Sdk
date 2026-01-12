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
    public string? UpdateWorkflowCode { get; set; }
    public string? DeleteWorkflowCode { get; set; }
    
    public List<WorkflowTriggerCondition> CreateTriggerConditions { get; set; } = [];
    public List<WorkflowTriggerCondition> UpdateTriggerConditions { get; set; } = [];
    public List<WorkflowTriggerCondition> DeleteTriggerConditions { get; set; } = [];
    
    public List<string> SignificantFieldsForUpdate { get; set; } = [];
    public bool AutoSubmitOnCreate { get; set; } = true;
    public bool PreventDirectSaveOnCreate { get; set; } = true;
    public decimal? AmountThreshold { get; set; }
    public List<string> ExcludeRoles { get; set; } = [];
    public List<string> ExcludeUsers { get; set; } = [];
}

public class WorkflowTriggerCondition
{
    public string Field { get; set; } = "";
    public WorkflowOperators Operator { get; set; } = WorkflowOperators.Equals;
    public string Value { get; set; } = "";
    public string? Description { get; set; }
}

