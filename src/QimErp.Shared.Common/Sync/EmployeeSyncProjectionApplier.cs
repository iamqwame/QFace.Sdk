namespace QimErp.Shared.Common.Sync;

/// <summary>
/// Applies a People-owned <see cref="Contracts.EmployeeSyncProjection"/> onto a local
/// <see cref="Entities.EmployeeBase"/> mirror. Used by module sync activities after
/// fetching from the owner (DATA-11) — never from a Temporal event payload.
/// </summary>
public static class EmployeeSyncProjectionApplier
{
    public static void Apply(EmployeeBase target, EmployeeSyncProjection projection)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(projection);

        target.UpdateBasicInfo(
            projection.FirstName,
            projection.LastName,
            projection.MiddleName,
            projection.Email);

        if (!string.IsNullOrWhiteSpace(projection.ProfilePicture))
            target.WithProfilePicture(projection.ProfilePicture);

        target.UpdateCurrentSupervisor(
            projection.SupervisorId,
            projection.SupervisorName,
            projection.SupervisorCode,
            projection.SupervisorTitle,
            projection.SupervisorEmail,
            projection.SupervisorPhone);

        target.UpdateCurrentOrganizationalUnit(
            projection.OrganizationalUnitId,
            projection.OrganizationalUnitName,
            projection.OrganizationalUnitCode);

        target.UpdateCurrentJobTitle(
            projection.JobTitleId,
            projection.JobTitleName,
            projection.JobTitleCode);

        target.UpdateCurrentStation(
            projection.LocationId,
            projection.LocationName,
            projection.LocationCode);

        target.UpdateCurrentJobStatus(
            projection.JobStatusId,
            projection.JobStatusName,
            projection.JobStatusCode);

        target.WithEmploymentStatus(projection.EmploymentStatus ?? target.EmploymentStatus);
        target.WithGender(projection.Gender);
        target.WithVisibilityAcrossCompanies(projection.IsVisibleAcrossCompanies);

        if (!string.IsNullOrWhiteSpace(projection.CompanyId))
            target.WithCompanyId(projection.CompanyId);

        if (projection.IsActive)
            target.Activate();
        else
            target.Deactivate();
    }
}
