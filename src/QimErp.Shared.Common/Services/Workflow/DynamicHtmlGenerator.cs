using System.Text;
using QimErp.Shared.Common.Workflow.Entities;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Generates dynamic HTML for workflow progress visualization.
/// </summary>
/// <param name="logger"></param>
public class DynamicHtmlGenerator(ILogger<DynamicHtmlGenerator> logger) : IDynamicHtmlGenerator
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
    public string GenerateWorkflowProgressHtml(
        WorkflowDefinition workflowDefinition,
        string currentStepCode,
        DateTime initiatedAt,
        bool isRequester = false,
        bool isCompleted = false)
    {
        if (workflowDefinition?.Steps == null || workflowDefinition.Steps.Count == 0)
        {
            return GenerateEmptyProgressHtml(initiatedAt);
        }

        var orderedSteps = workflowDefinition.Steps.OrderBy(s => s.Order).ToList();

        if (isCompleted)
        {
            var html = new StringBuilder();
            html.AppendLine("<div class=\"grid grid-cols-[32px_1fr] gap-x-3\">");

            html.Append(RenderRequestSubmittedStep(orderedSteps.Count > 0, initiatedAt));

            for (int i = 0; i < orderedSteps.Count; i++)
            {
                var step = orderedSteps[i];
                var isLast = i == orderedSteps.Count - 1;
                html.Append(RenderCompletedStep(step, isLast));
            }

            html.AppendLine("</div>");
            return html.ToString();
        }

        var currentStep = orderedSteps.FirstOrDefault(s => s.StepCode == currentStepCode);
        
        if (currentStep == null && !string.IsNullOrWhiteSpace(currentStepCode))
        {
            logger.LogWarning("⚠️ [DynamicHtmlGenerator] Current step code {StepCode} not found in workflow definition. Using first step as current.",
                currentStepCode);
            currentStep = orderedSteps.FirstOrDefault();
        }

        if (currentStep == null)
        {
            return GenerateEmptyProgressHtml(initiatedAt);
        }

        var progressHtml = new StringBuilder();
        progressHtml.AppendLine("<div class=\"grid grid-cols-[32px_1fr] gap-x-3\">");

        progressHtml.Append(RenderRequestSubmittedStep(orderedSteps.Count > 0, initiatedAt));

        for (int i = 0; i < orderedSteps.Count; i++)
        {
            var step = orderedSteps[i];
            var isLast = i == orderedSteps.Count - 1;
            
            if (step.Order < currentStep.Order)
            {
                progressHtml.Append(RenderCompletedStep(step, isLast));
            }
            else if (step.Order == currentStep.Order)
            {
                progressHtml.Append(RenderCurrentStep(step, isLast, isRequester));
            }
            else
            {
                progressHtml.Append(RenderPendingStep(step, isLast));
            }
        }

        progressHtml.AppendLine("</div>");
        return progressHtml.ToString();
    }

    /// <summary>
    /// Generates HTML for an empty workflow progress (only request submitted).
    /// </summary>
    /// <param name="initiatedAt"></param>
    /// <returns></returns>
    public string GenerateEmptyProgressHtml(DateTime initiatedAt)
    {
        var html = new StringBuilder();
        html.AppendLine("<div class=\"grid grid-cols-[32px_1fr] gap-x-3\">");
        html.Append(RenderRequestSubmittedStep(false, initiatedAt));
        html.AppendLine("</div>");
        return html.ToString();
    }

    private string RenderRequestSubmittedStep(bool hasSteps, DateTime initiatedAt)
    {
        var html = new StringBuilder();
        html.AppendLine("<div class=\"flex flex-col items-center\">");
        html.AppendLine("  <div class=\"text-emerald-600 dark:text-emerald-500 z-10 bg-slate-50 dark:bg-slate-800\" style=\"font-variation-settings: 'FILL' 1, 'wght' 600, 'opsz' 20\">");
        html.AppendLine("    <span class=\"material-symbols-outlined text-[20px]\">check_circle</span>");
        html.AppendLine("  </div>");
        
        if (hasSteps)
        {
            html.AppendLine("  <div class=\"w-[2px] bg-emerald-600 dark:text-emerald-500 h-full min-h-[2.5rem] -mt-1\"></div>");
        }
        
        html.AppendLine("</div>");
        html.AppendLine("<div class=\"flex flex-col pb-6 pt-0.5\">");
        html.AppendLine("  <p class=\"text-[#111318] dark:text-white text-sm font-semibold leading-none\">Request Submitted</p>");
        html.AppendLine($"  <p class=\"text-[#616f89] dark:text-slate-400 text-xs font-normal mt-1\">{initiatedAt:MMMM dd, yyyy}</p>");
        html.AppendLine("</div>");
        
        return html.ToString();
    }

    private string RenderCompletedStep(WorkflowStep step, bool isLast)
    {
        var html = new StringBuilder();
        html.AppendLine("<div class=\"flex flex-col items-center\">");
        html.AppendLine("  <div class=\"text-emerald-600 dark:text-emerald-500 z-10 bg-slate-50 dark:bg-slate-800\" style=\"font-variation-settings: 'FILL' 1, 'wght' 600, 'opsz' 20\">");
        html.AppendLine("    <span class=\"material-symbols-outlined text-[20px]\">check_circle</span>");
        html.AppendLine("  </div>");
        
        if (!isLast)
        {
            html.AppendLine("  <div class=\"w-[2px] bg-emerald-600 dark:text-emerald-500 h-full min-h-[2.5rem] -mt-1\"></div>");
        }
        
        html.AppendLine("</div>");
        html.AppendLine("<div class=\"flex flex-col pb-6 pt-0.5\">");
        html.AppendLine($"  <p class=\"text-[#111318] dark:text-white text-sm font-semibold leading-none\">{EscapeHtml(step.Name)}</p>");
        html.AppendLine("</div>");
        
        return html.ToString();
    }

    private string RenderCurrentStep(WorkflowStep step, bool isLast, bool isRequester = false)
    {
        var html = new StringBuilder();
        html.AppendLine("<div class=\"flex flex-col items-center\">");
        
        if (isRequester)
        {
            html.AppendLine("  <div class=\"text-amber-600 dark:text-amber-500 z-10 bg-slate-50 dark:bg-slate-800\" style=\"font-variation-settings: 'FILL' 1, 'wght' 600, 'opsz' 20\">");
            html.AppendLine("    <span class=\"material-symbols-outlined text-[20px]\">schedule</span>");
        }
        else
        {
            html.AppendLine("  <div class=\"text-primary z-10 bg-slate-50 dark:bg-slate-800\" style=\"font-variation-settings: 'FILL' 1, 'wght' 600, 'opsz' 20\">");
            html.AppendLine("    <span class=\"material-symbols-outlined text-[20px]\">pending</span>");
        }
        
        html.AppendLine("  </div>");
        
        if (!isLast)
        {
            html.AppendLine("  <div class=\"w-[2px] bg-[#dbdfe6] dark:bg-slate-600 h-full min-h-[2.5rem] -mt-1\"></div>");
        }
        
        html.AppendLine("</div>");
        html.AppendLine("<div class=\"flex flex-col pb-6 pt-0.5\">");
        html.AppendLine($"  <p class=\"text-[#111318] dark:text-white text-sm font-semibold leading-none\">{EscapeHtml(step.Name)}</p>");
        html.AppendLine("  <div class=\"flex items-center gap-2 mt-2\">");
        
        if (isRequester)
        {
            html.AppendLine("    <div class=\"flex items-center gap-1.5 px-2 py-0.5 rounded bg-amber-50 dark:bg-amber-900/30 border border-amber-100 dark:border-amber-800/50\">");
            html.AppendLine("      <span class=\"w-1.5 h-1.5 rounded-full bg-amber-500 animate-pulse\"></span>");
            html.AppendLine("      <p class=\"text-amber-700 dark:text-amber-300 text-xs font-medium\">Awaiting Approval</p>");
        }
        else
        {
            html.AppendLine("    <div class=\"flex items-center gap-1.5 px-2 py-0.5 rounded bg-blue-50 dark:bg-blue-900/30 border border-blue-100 dark:border-blue-800/50\">");
            html.AppendLine("      <span class=\"w-1.5 h-1.5 rounded-full bg-primary animate-pulse\"></span>");
            html.AppendLine("      <p class=\"text-primary dark:text-blue-300 text-xs font-medium\">Pending Your Review</p>");
        }
        
        html.AppendLine("    </div>");
        html.AppendLine("  </div>");
        html.AppendLine("</div>");
        
        return html.ToString();
    }

    private string RenderPendingStep(WorkflowStep step, bool isLast)
    {
        var html = new StringBuilder();
        html.AppendLine("<div class=\"flex flex-col items-center\">");
        html.AppendLine("  <div class=\"text-[#9ca3af] dark:text-slate-500 z-10 bg-slate-50 dark:bg-slate-800\" style=\"font-variation-settings: 'FILL' 0, 'wght' 400, 'opsz' 20\">");
        html.AppendLine("    <span class=\"material-symbols-outlined text-[20px]\">radio_button_unchecked</span>");
        html.AppendLine("  </div>");
        html.AppendLine("</div>");
        html.AppendLine("<div class=\"flex flex-col pt-0.5\">");
        html.AppendLine($"  <p class=\"text-[#111318] dark:text-white text-sm font-medium leading-none opacity-60\">{EscapeHtml(step.Name)}</p>");
        html.AppendLine("  <p class=\"text-[#616f89] dark:text-slate-400 text-xs font-normal mt-1 italic\">Waiting for approval</p>");
        html.AppendLine("</div>");
        
        return html.ToString();
    }

    private static string EscapeHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}

