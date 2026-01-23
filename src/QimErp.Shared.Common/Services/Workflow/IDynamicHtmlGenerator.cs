using QimErp.Shared.Common.Workflow.Entities;

namespace QimErp.Shared.Common.Services.Workflow;

public interface IDynamicHtmlGenerator
{
    /// <summary>
    /// Generates HTML representing the progress of a workflow.
    /// </summary>
    /// <param name="workflowDefinition"></param>
    /// <param name="currentStepCode"></param>
    /// <param name="initiatedAt"></param>
    /// <param name="isRequester"></param>
    /// <param name="isCompleted"></param>
    /// <returns></returns>
    string GenerateWorkflowProgressHtml(
        WorkflowDefinition workflowDefinition,
        string currentStepCode,
        DateTime initiatedAt,
        bool isRequester = false,
        bool isCompleted = false);

    /// <summary>
    /// Generates HTML for an empty workflow progress state.
    /// </summary>
    /// <param name="initiatedAt"></param>
    /// <returns></returns>
    string GenerateEmptyProgressHtml(DateTime initiatedAt);
}
