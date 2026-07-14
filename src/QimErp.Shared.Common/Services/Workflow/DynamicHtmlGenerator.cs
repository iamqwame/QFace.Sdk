namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Generates dynamic HTML for workflow progress visualization.
/// Uses email-safe inline styles and Unicode symbols for compatibility with email clients.
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
            html.AppendLine("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse:collapse;\">");

            html.Append(RenderRequestSubmittedStep(orderedSteps.Count > 0, initiatedAt));

            for (int i = 0; i < orderedSteps.Count; i++)
            {
                var step = orderedSteps[i];
                var isLast = i == orderedSteps.Count - 1;
                html.Append(RenderCompletedStep(step, isLast));
            }

            html.AppendLine("</table>");
            return html.ToString();
        }

        var currentStep = orderedSteps.FirstOrDefault(s => s.StepCode == currentStepCode);

        if (currentStep == null && !string.IsNullOrWhiteSpace(currentStepCode))
        {
            logger.LogWarning("[DynamicHtmlGenerator] Current step code {StepCode} not found in workflow definition. Using first step as current.",
                currentStepCode);
            currentStep = orderedSteps.FirstOrDefault();
        }

        if (currentStep == null)
        {
            return GenerateEmptyProgressHtml(initiatedAt);
        }

        var progressHtml = new StringBuilder();
        progressHtml.AppendLine("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse:collapse;\">");

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

        progressHtml.AppendLine("</table>");
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
        html.AppendLine("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse:collapse;\">");
        html.Append(RenderRequestSubmittedStep(false, initiatedAt));
        html.AppendLine("</table>");
        return html.ToString();
    }

    private string RenderRequestSubmittedStep(bool hasSteps, DateTime initiatedAt)
    {
        var html = new StringBuilder();
        html.AppendLine("<tr>");
        html.AppendLine("  <td style=\"width:32px;vertical-align:top;padding-top:2px;\">");
        html.AppendLine("    <span style=\"display:inline-block;width:20px;height:20px;background-color:#f8fafc;color:#059669;font-size:16px;text-align:center;line-height:20px;\">&#10003;</span>");
        html.AppendLine("  </td>");
        html.AppendLine("  <td style=\"vertical-align:top;padding-bottom:24px;\">");
        html.AppendLine("    <p style=\"margin:0;font-size:14px;font-weight:600;color:#111318;line-height:1.2;\">Request Submitted</p>");
        html.AppendLine($"    <p style=\"margin:4px 0 0;font-size:12px;color:#616f89;\">{initiatedAt:MMMM dd, yyyy}</p>");
        html.AppendLine("  </td>");
        html.AppendLine("</tr>");
        if (hasSteps)
        {
            html.AppendLine("<tr>");
            html.AppendLine("  <td style=\"width:32px;vertical-align:top;padding:0;\"><div style=\"width:2px;height:24px;background-color:#059669;margin-left:9px;\"></div></td>");
            html.AppendLine("  <td style=\"padding:0;\"></td>");
            html.AppendLine("</tr>");
        }
        return html.ToString();
    }

    private string RenderCompletedStep(WorkflowStep step, bool isLast)
    {
        var html = new StringBuilder();
        html.AppendLine("<tr>");
        html.AppendLine("  <td style=\"width:32px;vertical-align:top;padding-top:2px;\">");
        html.AppendLine("    <span style=\"display:inline-block;width:20px;height:20px;background-color:#f8fafc;color:#059669;font-size:16px;text-align:center;line-height:20px;\">&#10003;</span>");
        html.AppendLine("  </td>");
        html.AppendLine("  <td style=\"vertical-align:top;padding-bottom:24px;\">");
        html.AppendLine($"  <p style=\"margin:0;font-size:14px;font-weight:600;color:#111318;line-height:1.2;\">{EscapeHtml(step.Name)}</p>");
        html.AppendLine("  </td>");
        html.AppendLine("</tr>");
        if (!isLast)
        {
            html.AppendLine("<tr>");
            html.AppendLine("  <td style=\"width:32px;vertical-align:top;padding:0;\"><div style=\"width:2px;height:24px;background-color:#059669;margin-left:9px;\"></div></td>");
            html.AppendLine("  <td style=\"padding:0;\"></td>");
            html.AppendLine("</tr>");
        }
        return html.ToString();
    }

    private string RenderCurrentStep(WorkflowStep step, bool isLast, bool isRequester = false)
    {
        var html = new StringBuilder();
        var iconColor = isRequester ? "#d97706" : "#2b6cee";
        var badgeBg = isRequester ? "#fffbeb" : "#eff6ff";
        var badgeColor = isRequester ? "#b45309" : "#2563eb";
        var badgeBorder = isRequester ? "#fde68a" : "#bfdbfe";
        var badgeText = isRequester ? "Awaiting Approval" : "Pending Your Review";

        html.AppendLine("<tr>");
        html.AppendLine("  <td style=\"width:32px;vertical-align:top;padding-top:2px;\">");
        html.AppendLine($"    <span style=\"display:inline-block;width:20px;height:20px;background-color:#f8fafc;color:{iconColor};font-size:16px;text-align:center;line-height:20px;\">&#9679;</span>");
        html.AppendLine("  </td>");
        html.AppendLine("  <td style=\"vertical-align:top;padding-bottom:24px;\">");
        html.AppendLine($"  <p style=\"margin:0;font-size:14px;font-weight:600;color:#111318;line-height:1.2;\">{EscapeHtml(step.Name)}</p>");
        html.AppendLine($"  <span style=\"display:inline-block;margin-top:8px;padding:4px 8px;border-radius:4px;font-size:12px;font-weight:500;background-color:{badgeBg};color:{badgeColor};border:1px solid {badgeBorder};\">&#9679; {badgeText}</span>");
        html.AppendLine("  </td>");
        html.AppendLine("</tr>");
        if (!isLast)
        {
            html.AppendLine("<tr>");
            html.AppendLine("  <td style=\"width:32px;vertical-align:top;padding:0;\"><div style=\"width:2px;height:24px;background-color:#e2e8f0;margin-left:9px;\"></div></td>");
            html.AppendLine("  <td style=\"padding:0;\"></td>");
            html.AppendLine("</tr>");
        }
        return html.ToString();
    }

    private string RenderPendingStep(WorkflowStep step, bool isLast)
    {
        var html = new StringBuilder();
        html.AppendLine("<tr>");
        html.AppendLine("  <td style=\"width:32px;vertical-align:top;padding-top:2px;\">");
        html.AppendLine("    <span style=\"display:inline-block;width:20px;height:20px;background-color:#f8fafc;color:#9ca3af;font-size:14px;text-align:center;line-height:20px;\">&#9675;</span>");
        html.AppendLine("  </td>");
        html.AppendLine("  <td style=\"vertical-align:top;padding-bottom:24px;\">");
        html.AppendLine($"  <p style=\"margin:0;font-size:14px;font-weight:500;color:#111318;opacity:0.7;line-height:1.2;\">{EscapeHtml(step.Name)}</p>");
        html.AppendLine("  <p style=\"margin:4px 0 0;font-size:12px;color:#616f89;font-style:italic;\">Waiting for approval</p>");
        html.AppendLine("  </td>");
        html.AppendLine("</tr>");
        if (!isLast)
        {
            html.AppendLine("<tr>");
            html.AppendLine("  <td style=\"width:32px;vertical-align:top;padding:0;\"><div style=\"width:2px;height:24px;background-color:#e2e8f0;margin-left:9px;\"></div></td>");
            html.AppendLine("  <td style=\"padding:0;\"></td>");
            html.AppendLine("</tr>");
        }
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
