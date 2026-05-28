using QimErp.Shared.Common.Services.Workflow.Temporal;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Resolves workflow notification recipient tokens to concrete email addresses.
/// Modules extend via approver resolver activities.
/// </summary>
public static class WorkflowNotificationRecipientResolver
{
    public const string InitiatorToken = "Initiator";
    public const string RequesterToken = "Requester";

    public static List<string> ExtractLiteralEmails(IEnumerable<string> tokens) =>
        tokens.Where(IsValidEmail).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    public static List<string> ExtractRoleTokens(IEnumerable<string> tokens) =>
        tokens.Where(t => !string.IsNullOrWhiteSpace(t) && !IsValidEmail(t)).ToList();

    public static ResolvedApprover? ResolveInitiator(ApprovalWorkflowInput input)
    {
        if (string.IsNullOrWhiteSpace(input.InitiatedBy) || !IsValidEmail(input.InitiatedBy))
            return null;

        return new ResolvedApprover
        {
            Email = input.InitiatedBy,
            Name = input.InitiatedByName ?? input.InitiatedBy,
            Id = Guid.TryParse(input.InitiatedByEmployeeId, out var id) ? id : Guid.Empty
        };
    }

    public static bool IsInitiatorToken(string token) =>
        string.Equals(token, InitiatorToken, StringComparison.OrdinalIgnoreCase)
        || string.Equals(token, RequesterToken, StringComparison.OrdinalIgnoreCase);

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try { _ = new System.Net.Mail.MailAddress(email); return true; }
        catch { return false; }
    }
}
