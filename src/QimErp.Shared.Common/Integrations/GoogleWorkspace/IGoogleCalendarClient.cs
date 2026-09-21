namespace QimErp.Shared.Common.Integrations.GoogleWorkspace;

public sealed class TenantGoogleWorkspaceResolvedOptions
{
    public string ClientEmail { get; set; } = string.Empty;
    public string PrivateKeyPem { get; set; } = string.Empty;
    public string? DelegatedAdminEmail { get; set; }
    public bool EnableCalendarBlockOnLeave { get; set; }
    public bool HideLeaveTypeOnCalendar { get; set; }
}

public sealed class GoogleCalendarResult
{
    public bool Succeeded { get; init; }
    public bool Skipped { get; init; }
    public string? Error { get; init; }
}

public interface IGoogleCalendarClient
{
    Task<TenantGoogleWorkspaceResolvedOptions?> GetTenantOptionsAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<GoogleCalendarResult> TryUpsertLeaveCalendarEventAsync(
        string tenantId,
        string employeeEmail,
        string subject,
        DateTime startUtc,
        DateTime endUtcExclusive,
        string? transactionId,
        bool clear,
        CancellationToken cancellationToken = default);
}
