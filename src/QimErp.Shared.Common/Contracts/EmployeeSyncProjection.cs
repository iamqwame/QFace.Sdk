namespace QimErp.Shared.Common.Contracts;

/// <summary>
/// Owner read contract for employee sync (DATA-11): returned by People's internal API
/// so Temporal payloads stay Ids-only. Must not be placed on workflow/event envelopes.
/// </summary>
public sealed class EmployeeSyncProjection
{
    public Guid EmployeeId { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string CompanyId { get; init; } = string.Empty;
    public bool IsVisibleAcrossCompanies { get; init; }
    public long Version { get; init; }

    public string Code { get; init; } = string.Empty;
    public string? EmployeeNo { get; init; }
    public string? ReferenceCode { get; init; }

    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public string? PreferredName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? ProfilePicture { get; init; }
    public string? Gender { get; init; }

    public bool IsActive { get; init; }
    public string? EmploymentStatus { get; init; }
    public string? WorkWeekDays { get; init; }
    public DateOnly? DateOfEmployment { get; init; }

    public Guid? JobTitleId { get; init; }
    public string? JobTitleName { get; init; }
    public string? JobTitleCode { get; init; }

    public Guid? OrganizationalUnitId { get; init; }
    public string? OrganizationalUnitName { get; init; }
    public string? OrganizationalUnitCode { get; init; }

    public Guid? LocationId { get; init; }
    public string? LocationName { get; init; }
    public string? LocationCode { get; init; }
    public string? LocationCountry { get; init; }
    public string? LocationRegion { get; init; }
    public bool LocationGeographyResolved { get; init; }

    public Guid? SupervisorId { get; init; }
    public string? SupervisorName { get; init; }
    public string? SupervisorCode { get; init; }
    public string? SupervisorTitle { get; init; }
    public string? SupervisorEmail { get; init; }
    public string? SupervisorPhone { get; init; }

    public Guid? JobStatusId { get; init; }
    public string? JobStatusName { get; init; }
    public string? JobStatusCode { get; init; }
}
