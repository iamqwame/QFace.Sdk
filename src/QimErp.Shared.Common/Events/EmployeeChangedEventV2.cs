namespace QimErp.Shared.Common.Events;

/// <summary>
/// DATA-11 / DATA-25 compliant employee change contract: envelope + Ids only.
/// No names, emails, phones, national Ids, or other personal/display data.
/// Consumers fetch current state from People (owner) via activity → internal API.
/// </summary>
/// <remarks>
/// Breaking replacement for <see cref="EmployeeChangedEvent"/>. Publish alongside V1
/// until all consumers migrate (DATA-11 additive/V2 rule at level S).
/// </remarks>
public sealed class EmployeeChangedEventV2
{
    public const string EventTypeName = "EmployeeChanged";

    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public string EventType => EventTypeName;
    public string TenantId { get; init; } = string.Empty;
    public Guid EmployeeId { get; init; }
    public long Version { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public string CorrelationId { get; init; } = string.Empty;

    public string CompanyId { get; init; } = string.Empty;
    public bool IsVisibleAcrossCompanies { get; init; }
    public string? GrantCompanyId { get; init; }
    public string? RevokeCompanyId { get; init; }

    /// <summary>Staff code — Internal classification (DATA-25), not PII.</summary>
    public string? Code { get; init; }
    public string? EmployeeNo { get; init; }
    public string? ReferenceCode { get; init; }

    public bool IsActive { get; init; }
    public string? EmploymentStatus { get; init; }
    public string? WorkWeekDays { get; init; }
    public DateOnly? DateOfEmployment { get; init; }

    public Guid? JobTitleId { get; init; }
    public Guid? OrganizationalUnitId { get; init; }
    public Guid? LocationId { get; init; }
    public bool LocationGeographyResolved { get; init; }
    public Guid? SupervisorId { get; init; }
    public Guid? JobStatusId { get; init; }

    public bool SendInvitation { get; init; }
    public bool SetDefaultPassword { get; init; }
    public bool MustChangePassword { get; init; } = true;
    public bool IsSeed { get; init; }

    /// <summary>Tenant installed modules — set by EmployeeSyncWorkflow before fan-out.</summary>
    public IReadOnlyList<string>? SelectedModules { get; set; }

    public static EmployeeChangedEventV2 Create(
        Guid employeeId,
        string tenantId,
        long version,
        string? correlationId = null)
    {
        return new EmployeeChangedEventV2
        {
            EventId = Guid.CreateVersion7(),
            EmployeeId = employeeId,
            TenantId = tenantId ?? string.Empty,
            Version = version,
            OccurredAt = DateTimeOffset.UtcNow,
            CorrelationId = correlationId ?? string.Empty,
            IsActive = true
        };
    }
}
