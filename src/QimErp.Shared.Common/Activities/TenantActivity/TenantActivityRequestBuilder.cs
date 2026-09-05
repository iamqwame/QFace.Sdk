using QimErp.Shared.Common.Services.Auth;

namespace QimErp.Shared.Common.Activities.TenantActivity;

/// <summary>
/// Factory helpers for <see cref="RecordTenantActivityRequest"/>.
/// Call sites pass actor id, subject, and optional business <c>metadataJson</c> only — IAM enriches
/// actor name/email/phone/picture at persist time and <see cref="Services.TenantActivity.TenantActivityRecorder"/>
/// merges request context (IP, user agent, session id) from the current HTTP request.
/// </summary>
public static class TenantActivityRequestBuilder
{
    public static RecordTenantActivityRequest ForEmployeeCreated(
        string tenantId,
        Guid employeeId,
        string employeeName,
        string? organizationalUnitName,
        ICurrentUserService currentUser) =>
        Build(
            tenantId,
            TenantActivityModules.Hr,
            HrActivityTypes.EmployeeCreated,
            BuildEmployeeJoinedSummary(employeeName, organizationalUnitName),
            "employee",
            employeeId,
            employeeName,
            currentUser,
            correlationSuffix: $"employee-created:{employeeId:N}");

    public static RecordTenantActivityRequest ForEmployeeUpdated(
        string tenantId,
        Guid employeeId,
        string employeeName,
        ICurrentUserService currentUser) =>
        Build(
            tenantId,
            TenantActivityModules.Hr,
            HrActivityTypes.EmployeeUpdated,
            $"Employee {employeeName} was updated",
            "employee",
            employeeId,
            employeeName,
            currentUser,
            correlationSuffix: $"employee-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeProfilePictureUpdated(
        string tenantId,
        Guid employeeId,
        string employeeName,
        string photoUrl,
        ICurrentUserService currentUser) =>
        Build(
            tenantId,
            TenantActivityModules.Hr,
            HrActivityTypes.EmployeeProfilePictureUpdated,
            $"Employee {employeeName}'s profile picture was updated",
            "employee",
            employeeId,
            employeeName,
            currentUser,
            correlationSuffix: $"employee-profile-picture-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            pictureUrl: photoUrl);

    // ── Department / Org Unit ──────────────────────────────────────────────
    public static RecordTenantActivityRequest ForDepartmentCreated(
        string tenantId, Guid unitId, string unitName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.DepartmentCreated,
            $"New department \"{unitName}\" was created",
            "department", unitId, unitName, currentUser,
            correlationSuffix: $"department-created:{unitId:N}");

    public static RecordTenantActivityRequest ForDepartmentUpdated(
        string tenantId, Guid unitId, string unitName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.DepartmentUpdated,
            $"Department \"{unitName}\" was updated",
            "department", unitId, unitName, currentUser,
            correlationSuffix: $"department-updated:{unitId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDepartmentDeleted(
        string tenantId, Guid unitId, string unitName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.DepartmentDeleted,
            $"Department \"{unitName}\" was deleted",
            "department", unitId, unitName, currentUser,
            correlationSuffix: $"department-deleted:{unitId:N}");

    // ── Job Titles ─────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForJobTitleCreated(
        string tenantId, Guid jobTitleId, string jobTitleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobTitleCreated,
            $"Job title \"{jobTitleName}\" was created",
            "job-title", jobTitleId, jobTitleName, currentUser,
            correlationSuffix: $"job-title-created:{jobTitleId:N}");

    public static RecordTenantActivityRequest ForJobTitleUpdated(
        string tenantId, Guid jobTitleId, string jobTitleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobTitleUpdated,
            $"Job title \"{jobTitleName}\" was updated",
            "job-title", jobTitleId, jobTitleName, currentUser,
            correlationSuffix: $"job-title-updated:{jobTitleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForJobTitleDeleted(
        string tenantId, Guid jobTitleId, string jobTitleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobTitleDeleted,
            $"Job title \"{jobTitleName}\" was deleted",
            "job-title", jobTitleId, jobTitleName, currentUser,
            correlationSuffix: $"job-title-deleted:{jobTitleId:N}");

    // ── Leave ──────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForLeaveRequestSubmitted(
        string tenantId, Guid leaveRequestId, string employeeName, string leaveTypeName,
        Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.LeaveRequestSubmitted,
            $"{employeeName} submitted a {leaveTypeName} request",
            "leave-request", leaveRequestId, employeeName, actorUserId,
            correlationSuffix: $"leave-submitted:{leaveRequestId:N}");

    public static RecordTenantActivityRequest ForLeaveRequestApproved(
        string tenantId, Guid leaveRequestId, string employeeName, string leaveTypeName,
        Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.LeaveRequestApproved,
            $"{employeeName}'s {leaveTypeName} request was approved",
            "leave-request", leaveRequestId, employeeName, actorUserId,
            correlationSuffix: $"leave-approved:{leaveRequestId:N}");

    public static RecordTenantActivityRequest ForLeaveRequestRejected(
        string tenantId, Guid leaveRequestId, string employeeName, string leaveTypeName,
        Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.LeaveRequestRejected,
            $"{employeeName}'s {leaveTypeName} request was rejected",
            "leave-request", leaveRequestId, employeeName, actorUserId,
            correlationSuffix: $"leave-rejected:{leaveRequestId:N}");

    public static RecordTenantActivityRequest ForLeaveRequestCancelled(
        string tenantId, Guid leaveRequestId, string employeeName, string leaveTypeName,
        Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.LeaveRequestCancelled,
            $"{employeeName}'s {leaveTypeName} request was cancelled",
            "leave-request", leaveRequestId, employeeName, actorUserId,
            correlationSuffix: $"leave-cancelled:{leaveRequestId:N}");

    // ── Offboarding ────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForOffboardingInitiated(
        string tenantId, Guid instanceId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.OffboardingInitiated,
            $"Offboarding initiated for {employeeName}",
            "offboarding", instanceId, employeeName, currentUser,
            correlationSuffix: $"offboarding-initiated:{instanceId:N}");

    public static RecordTenantActivityRequest ForOffboardingApproved(
        string tenantId, Guid instanceId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.OffboardingApproved,
            $"Offboarding approved for {employeeName}",
            "offboarding", instanceId, employeeName, currentUser,
            correlationSuffix: $"offboarding-approved:{instanceId:N}");

    public static RecordTenantActivityRequest ForOffboardingCancelled(
        string tenantId, Guid instanceId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.OffboardingCancelled,
            $"Offboarding cancelled for {employeeName}",
            "offboarding", instanceId, employeeName, currentUser,
            correlationSuffix: $"offboarding-cancelled:{instanceId:N}");

    public static RecordTenantActivityRequest ForOffboardingCompleted(
        string tenantId, Guid instanceId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.OffboardingCompleted,
            $"Offboarding completed for {employeeName}",
            "offboarding", instanceId, employeeName, currentUser,
            correlationSuffix: $"offboarding-completed:{instanceId:N}");

    // ── Employee assignments ───────────────────────────────────────────────
    public static RecordTenantActivityRequest ForEmployeeJobTitleChanged(
        string tenantId, Guid employeeId, string employeeName, string jobTitleName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeJobTitleChanged,
            $"{employeeName} was assigned job title \"{jobTitleName}\"",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"job-title-changed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeOrgUnitChanged(
        string tenantId, Guid employeeId, string employeeName, string orgUnitName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeOrgUnitChanged,
            $"{employeeName} was moved to \"{orgUnitName}\"",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"org-unit-changed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeJobTitleAssignmentDeleted(
        string tenantId, Guid employeeId, string employeeName, string jobTitleName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeJobTitleAssignmentDeleted,
            $"{employeeName}'s job title assignment \"{jobTitleName}\" was deleted",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"job-title-assignment-deleted:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeOrgUnitAssignmentDeleted(
        string tenantId, Guid employeeId, string employeeName, string orgUnitName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeOrgUnitAssignmentDeleted,
            $"{employeeName}'s organizational unit assignment \"{orgUnitName}\" was deleted",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"org-unit-assignment-deleted:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeJobStatusChanged(
        string tenantId, Guid employeeId, string employeeName, string jobStatusName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeJobStatusChanged,
            $"{employeeName}'s job status was changed to \"{jobStatusName}\"",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"job-status-changed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeStationChanged(
        string tenantId, Guid employeeId, string employeeName, string stationName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeStationChanged,
            $"{employeeName}'s station was changed to \"{stationName}\"",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"station-changed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeRankChanged(
        string tenantId, Guid employeeId, string employeeName, string rankName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeRankChanged,
            $"{employeeName}'s rank was changed to \"{rankName}\"",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"rank-changed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeSupervisorChanged(
        string tenantId, Guid employeeId, string employeeName, string supervisorName,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeSupervisorChanged,
            $"{employeeName}'s supervisor was changed to {supervisorName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"supervisor-changed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeDeleted(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeDeleted,
            $"Employee {employeeName} was deleted",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-deleted:{employeeId:N}");

    // ── Bulk operations (generic) ───────────────────────────────────────────
    public static RecordTenantActivityRequest ForBulkOperation(
        string tenantId, string operationLabel, int affectedCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.BulkOperation,
            $"{operationLabel} affecting {affectedCount} employee{(affectedCount == 1 ? "" : "s")}",
            "bulk-operation", Guid.Empty, operationLabel, currentUser,
            correlationSuffix: $"bulk-operation:{operationLabel.ToLowerInvariant().Replace(' ', '-')}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeBulkImportQueued(
        string tenantId, string importType, string fileName, string jobId, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeBulkImportQueued,
            $"Bulk {importType} import started (file: {fileName})",
            "bulk-operation", Guid.Empty, importType, currentUser,
            correlationSuffix: $"employee-bulk-import:{jobId}");

    public static RecordTenantActivityRequest ForPayrollBulkImportQueued(
        string tenantId, string importType, string fileName, string jobId, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollBulkImportQueued,
            $"Bulk {importType} import started (file: {fileName})",
            "bulk-operation", Guid.Empty, importType, currentUser,
            correlationSuffix: $"payroll-bulk-import:{jobId}");

    public static RecordTenantActivityRequest ForPerformanceLibraryImported(
        string tenantId, string fileName, string jobId, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.PerformanceLibraryImported,
            $"Performance library import started (file: {fileName})",
            "bulk-operation", Guid.Empty, "performance-library", currentUser,
            correlationSuffix: $"performance-library-import:{jobId}");

    public static RecordTenantActivityRequest ForAppraisalTargetsImported(
        string tenantId, Guid appraisalPeriodId, int createdCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.AppraisalTargetsImported,
            $"Appraisal target import committed: {createdCount} draft plan{(createdCount == 1 ? "" : "s")} created",
            "bulk-operation", Guid.Empty, "appraisal-targets", currentUser,
            correlationSuffix: $"appraisal-targets-imported:{appraisalPeriodId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Employee records (certifications, dependants, documents, etc.) ─────
    public static RecordTenantActivityRequest ForEmployeeCertificationAdded(
        string tenantId, Guid employeeId, string employeeName, string certificationName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeCertificationAdded,
            $"Certification \"{certificationName}\" was added for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-certification-added:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeCertificationDeleted(
        string tenantId, Guid employeeId, string employeeName, string certificationName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeCertificationDeleted,
            $"Certification \"{certificationName}\" was deleted for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-certification-deleted:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeCertificationUpdated(
        string tenantId, Guid employeeId, string employeeName, string certificationName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeCertificationUpdated,
            $"Certification \"{certificationName}\" was updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-certification-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeDependantAdded(
        string tenantId, Guid employeeId, string employeeName, string dependantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeDependantAdded,
            $"Dependant \"{dependantName}\" was added for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-dependant-added:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeDependantRemoved(
        string tenantId, Guid employeeId, string employeeName, string dependantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeDependantRemoved,
            $"Dependant \"{dependantName}\" was removed for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-dependant-removed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeDependantUpdated(
        string tenantId, Guid employeeId, string employeeName, string dependantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeDependantUpdated,
            $"Dependant \"{dependantName}\" was updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-dependant-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeDocumentUploaded(
        string tenantId, Guid employeeId, string employeeName, string documentName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeDocumentUploaded,
            $"Document \"{documentName}\" was uploaded for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-document-uploaded:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeDocumentDeleted(
        string tenantId, Guid employeeId, string employeeName, string documentName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeDocumentDeleted,
            $"Document \"{documentName}\" was deleted for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-document-deleted:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeEducationalQualificationAdded(
        string tenantId, Guid employeeId, string employeeName, string qualificationName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeEducationalQualificationAdded,
            $"Educational qualification \"{qualificationName}\" was added for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-educational-qualification-added:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeEducationalQualificationDeleted(
        string tenantId, Guid employeeId, string employeeName, string qualificationName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeEducationalQualificationDeleted,
            $"Educational qualification \"{qualificationName}\" was deleted for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-educational-qualification-deleted:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeEducationalQualificationUpdated(
        string tenantId, Guid employeeId, string employeeName, string qualificationName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeEducationalQualificationUpdated,
            $"Educational qualification \"{qualificationName}\" was updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-educational-qualification-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeEmergencyContactRemoved(
        string tenantId, Guid employeeId, string employeeName, string contactName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeEmergencyContactRemoved,
            $"Emergency contact \"{contactName}\" was removed for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-emergency-contact-removed:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeEmergencyContactUpdated(
        string tenantId, Guid employeeId, string employeeName, string contactName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeEmergencyContactUpdated,
            $"Emergency contact \"{contactName}\" was updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-emergency-contact-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeNextOfKinAdded(
        string tenantId, Guid employeeId, string employeeName, string nextOfKinName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeNextOfKinAdded,
            $"Next of kin \"{nextOfKinName}\" was added for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-next-of-kin-added:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeNextOfKinDeleted(
        string tenantId, Guid employeeId, string employeeName, string nextOfKinName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeNextOfKinDeleted,
            $"Next of kin \"{nextOfKinName}\" was deleted for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-next-of-kin-deleted:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeNextOfKinUpdated(
        string tenantId, Guid employeeId, string employeeName, string nextOfKinName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeNextOfKinUpdated,
            $"Next of kin \"{nextOfKinName}\" was updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-next-of-kin-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeTrainingAdded(
        string tenantId, Guid employeeId, string employeeName, string trainingName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeTrainingAdded,
            $"Training \"{trainingName}\" was added for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-training-added:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeTrainingDeleted(
        string tenantId, Guid employeeId, string employeeName, string trainingName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeTrainingDeleted,
            $"Training \"{trainingName}\" was deleted for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-training-deleted:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeTrainingUpdated(
        string tenantId, Guid employeeId, string employeeName, string trainingName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.EmployeeTrainingUpdated,
            $"Training \"{trainingName}\" was updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-training-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOrgChartRootSet(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEmployeeRecordActivityTypes.OrgChartRootSet,
            $"{employeeName} was set as the org chart root",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"org-chart-root-set:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Payroll (grade bulk) ───────────────────────────────────────────────
    public static RecordTenantActivityRequest ForGradeBulkAssigned(
        string tenantId, string gradeCode, int count, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, PayrollActivityTypes.GradeBulkAssigned,
            $"Grade {gradeCode} assigned to {count} employee{(count == 1 ? "" : "s")}",
            "grade", Guid.Empty, gradeCode, actorUserId,
            correlationSuffix: $"grade-bulk-assigned:{gradeCode}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeDeactivated(
        string tenantId,
        Guid employeeId,
        string employeeName,
        ICurrentUserService currentUser) =>
        Build(
            tenantId,
            TenantActivityModules.Hr,
            HrActivityTypes.EmployeeDeactivated,
            $"Employee {employeeName} was deactivated",
            "employee",
            employeeId,
            employeeName,
            currentUser,
            correlationSuffix: $"employee-deactivated:{employeeId:N}");

    // ── Employee Lifecycle ─────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForEmployeeActivated(
        string tenantId, Guid employeeId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.EmployeeActivated,
            $"Employee {name} was activated",
            "employee", employeeId, name, currentUser,
            correlationSuffix: $"employee-activated:{employeeId:N}");

    // ── Rank ───────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForRankCreated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.RankCreated,
            $"Rank \"{name}\" was created",
            "rank", id, name, currentUser,
            correlationSuffix: $"rank-created:{id:N}");

    public static RecordTenantActivityRequest ForRankUpdated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.RankUpdated,
            $"Rank \"{name}\" was updated",
            "rank", id, name, currentUser,
            correlationSuffix: $"rank-updated:{id:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForRankDeleted(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.RankDeleted,
            $"Rank \"{name}\" was deleted",
            "rank", id, name, currentUser,
            correlationSuffix: $"rank-deleted:{id:N}");

    // ── Station ────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForStationCreated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.StationCreated,
            $"Station \"{name}\" was created",
            "station", id, name, currentUser,
            correlationSuffix: $"station-created:{id:N}");

    public static RecordTenantActivityRequest ForStationUpdated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.StationUpdated,
            $"Station \"{name}\" was updated",
            "station", id, name, currentUser,
            correlationSuffix: $"station-updated:{id:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForStationDeleted(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.StationDeleted,
            $"Station \"{name}\" was deleted",
            "station", id, name, currentUser,
            correlationSuffix: $"station-deleted:{id:N}");

    // ── Job Status ─────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForJobStatusCreated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobStatusCreated,
            $"Job status \"{name}\" was created",
            "job-status", id, name, currentUser,
            correlationSuffix: $"job-status-created:{id:N}");

    public static RecordTenantActivityRequest ForJobStatusUpdated(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobStatusUpdated,
            $"Job status \"{name}\" was updated",
            "job-status", id, name, currentUser,
            correlationSuffix: $"job-status-updated:{id:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForJobStatusDeleted(
        string tenantId, Guid id, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrActivityTypes.JobStatusDeleted,
            $"Job status \"{name}\" was deleted",
            "job-status", id, name, currentUser,
            correlationSuffix: $"job-status-deleted:{id:N}");

    // ── Onboarding ─────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForOnboardingStarted(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingStarted,
            $"Onboarding started for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"onboarding-started:{employeeId:N}");

    public static RecordTenantActivityRequest ForOnboardingCancelled(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingCancelled,
            $"Onboarding cancelled for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"onboarding-cancelled:{employeeId:N}");

    public static RecordTenantActivityRequest ForOnboardingMetaUpdated(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingMetaUpdated,
            $"Onboarding details updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"onboarding-meta-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingTaskCompleted(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskCompleted,
            $"Onboarding task \"{taskTitle}\" was completed",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-completed:{taskId:N}");

    public static RecordTenantActivityRequest ForOnboardingTaskReopened(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskReopened,
            $"Onboarding task \"{taskTitle}\" was reopened",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-reopened:{taskId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingTaskSkipped(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskSkipped,
            $"Onboarding task \"{taskTitle}\" was skipped",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-skipped:{taskId:N}");

    public static RecordTenantActivityRequest ForOnboardingTaskReassigned(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskReassigned,
            $"Onboarding task \"{taskTitle}\" was reassigned",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-reassigned:{taskId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingTaskEscalated(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTaskEscalated,
            $"Onboarding task \"{taskTitle}\" was escalated",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-task-escalated:{taskId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingAdhocTaskAdded(
        string tenantId, Guid taskId, string taskTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingAdhocTaskAdded,
            $"Ad-hoc onboarding task \"{taskTitle}\" was added",
            "onboarding-task", taskId, taskTitle, currentUser,
            correlationSuffix: $"onboarding-adhoc-task-added:{taskId:N}");

    public static RecordTenantActivityRequest ForOnboardingTemplateCreated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTemplateCreated,
            $"Onboarding template \"{templateName}\" was created",
            "onboarding-template", templateId, templateName, currentUser,
            correlationSuffix: $"onboarding-template-created:{templateId:N}");

    public static RecordTenantActivityRequest ForOnboardingTemplateUpdated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTemplateUpdated,
            $"Onboarding template \"{templateName}\" was updated",
            "onboarding-template", templateId, templateName, currentUser,
            correlationSuffix: $"onboarding-template-updated:{templateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOnboardingTemplateDeleted(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTemplateDeleted,
            $"Onboarding template \"{templateName}\" was deleted",
            "onboarding-template", templateId, templateName, currentUser,
            correlationSuffix: $"onboarding-template-deleted:{templateId:N}");

    public static RecordTenantActivityRequest ForOnboardingTemplateDefaultSet(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrOnboardingActivityTypes.OnboardingTemplateDefaultSet,
            $"Onboarding template \"{templateName}\" was set as default",
            "onboarding-template", templateId, templateName, currentUser,
            correlationSuffix: $"onboarding-template-default-set:{templateId:N}");

    // ── Performance ────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForPerformanceReviewCreated(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ReviewCreated,
            summary, "performance-review", reviewId, summary, currentUser,
            correlationSuffix: $"review-created:{reviewId:N}");

    public static RecordTenantActivityRequest ForSelfReviewSubmitted(
        string tenantId, Guid reviewId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.SelfReviewSubmitted,
            summary, "performance-review", reviewId, summary, actorUserId,
            correlationSuffix: $"self-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForManagerReviewSubmitted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ManagerReviewSubmitted,
            summary, "performance-review", reviewId, summary, currentUser,
            correlationSuffix: $"manager-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForHRReviewSubmitted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.HRReviewSubmitted,
            summary, "performance-review", reviewId, summary, currentUser,
            correlationSuffix: $"hr-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForPerformanceReviewFinalized(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ReviewFinalized,
            summary, "performance-review", reviewId, summary, currentUser,
            correlationSuffix: $"review-finalized:{reviewId:N}");

    public static RecordTenantActivityRequest ForPerformanceReviewAcknowledged(
        string tenantId, Guid reviewId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ReviewAcknowledged,
            summary, "performance-review", reviewId, summary, actorUserId,
            correlationSuffix: $"review-acknowledged:{reviewId:N}");

    public static RecordTenantActivityRequest ForGoalCreated(
        string tenantId, Guid goalId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalCreated,
            $"Goal \"{title}\" was created",
            "goal", goalId, title, currentUser,
            correlationSuffix: $"goal-created:{goalId:N}");

    public static RecordTenantActivityRequest ForGoalUpdated(
        string tenantId, Guid goalId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalUpdated,
            $"Goal \"{title}\" was updated",
            "goal", goalId, title, currentUser,
            correlationSuffix: $"goal-updated:{goalId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForGoalProgressUpdated(
        string tenantId, Guid goalId, string title, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalProgressUpdated,
            $"Progress updated on goal \"{title}\"",
            "goal", goalId, title, actorUserId,
            correlationSuffix: $"goal-progress-updated:{goalId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForGoalApproved(
        string tenantId, Guid goalId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalApproved,
            $"Goal \"{title}\" was approved",
            "goal", goalId, title, currentUser,
            correlationSuffix: $"goal-approved:{goalId:N}");

    public static RecordTenantActivityRequest ForGoalCompleted(
        string tenantId, Guid goalId, string title, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalCompleted,
            $"Goal \"{title}\" was completed",
            "goal", goalId, title, actorUserId,
            correlationSuffix: $"goal-completed:{goalId:N}");

    public static RecordTenantActivityRequest ForGoalAligned(
        string tenantId, Guid goalId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalAligned,
            $"Goal \"{title}\" was aligned",
            "goal", goalId, title, currentUser,
            correlationSuffix: $"goal-aligned:{goalId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForGoalEvidenceUploaded(
        string tenantId, Guid goalId, string title, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.GoalEvidenceUploaded,
            $"Evidence uploaded for goal \"{title}\"",
            "goal", goalId, title, actorUserId,
            correlationSuffix: $"goal-evidence-uploaded:{goalId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCheckInCreated(
        string tenantId, Guid checkInId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CheckInCreated,
            summary, "check-in", checkInId, summary, currentUser,
            correlationSuffix: $"check-in-created:{checkInId:N}");

    public static RecordTenantActivityRequest ForCheckInUpdated(
        string tenantId, Guid checkInId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CheckInUpdated,
            summary, "check-in", checkInId, summary, currentUser,
            correlationSuffix: $"check-in-updated:{checkInId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCheckInManagerFeedbackAdded(
        string tenantId, Guid checkInId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CheckInManagerFeedbackAdded,
            summary, "check-in", checkInId, summary, currentUser,
            correlationSuffix: $"check-in-manager-feedback-added:{checkInId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCalibrationCreated(
        string tenantId, Guid calibrationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CalibrationCreated,
            summary, "calibration", calibrationId, summary, currentUser,
            correlationSuffix: $"calibration-created:{calibrationId:N}");

    public static RecordTenantActivityRequest ForCalibrationRatingAdjusted(
        string tenantId, Guid calibrationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CalibrationRatingAdjusted,
            summary, "calibration", calibrationId, summary, currentUser,
            correlationSuffix: $"calibration-rating-adjusted:{calibrationId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCalibrationCompleted(
        string tenantId, Guid calibrationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CalibrationCompleted,
            summary, "calibration", calibrationId, summary, currentUser,
            correlationSuffix: $"calibration-completed:{calibrationId:N}");

    public static RecordTenantActivityRequest ForCalibrationAnalyzed(
        string tenantId, Guid calibrationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CalibrationAnalyzed,
            summary, "calibration", calibrationId, summary, currentUser,
            correlationSuffix: $"calibration-analyzed:{calibrationId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForModerationCommitteeEnabled(
        string tenantId, Guid calibrationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ModerationCommitteeEnabled,
            summary, "calibration", calibrationId, summary, currentUser,
            correlationSuffix: $"moderation-committee-enabled:{calibrationId:N}");

    public static RecordTenantActivityRequest ForFeedback360Created(
        string tenantId, Guid feedbackId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.Feedback360Created,
            summary, "feedback360", feedbackId, summary, currentUser,
            correlationSuffix: $"feedback360-created:{feedbackId:N}");

    public static RecordTenantActivityRequest ForFeedback360ProviderAdded(
        string tenantId, Guid feedbackId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.Feedback360ProviderAdded,
            summary, "feedback360", feedbackId, summary, currentUser,
            correlationSuffix: $"feedback360-provider-added:{feedbackId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForFeedback360Completed(
        string tenantId, Guid feedbackId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.Feedback360Completed,
            summary, "feedback360", feedbackId, summary, currentUser,
            correlationSuffix: $"feedback360-completed:{feedbackId:N}");

    public static RecordTenantActivityRequest ForDevelopmentPlanCreated(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanCreated,
            $"Development plan \"{title}\" was created",
            "development-plan", planId, title, currentUser,
            correlationSuffix: $"development-plan-created:{planId:N}");

    public static RecordTenantActivityRequest ForDevelopmentPlanActivityAdded(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanActivityAdded,
            $"Activity added to development plan \"{title}\"",
            "development-plan", planId, title, currentUser,
            correlationSuffix: $"development-plan-activity-added:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDevelopmentPlanActivityCompleted(
        string tenantId, Guid planId, string title, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanActivityCompleted,
            $"Activity completed in development plan \"{title}\"",
            "development-plan", planId, title, actorUserId,
            correlationSuffix: $"development-plan-activity-completed:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDevelopmentPlanApproved(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanApproved,
            $"Development plan \"{title}\" was approved",
            "development-plan", planId, title, currentUser,
            correlationSuffix: $"development-plan-approved:{planId:N}");

    public static RecordTenantActivityRequest ForDevelopmentPlanCompleted(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.DevelopmentPlanCompleted,
            $"Development plan \"{title}\" was completed",
            "development-plan", planId, title, currentUser,
            correlationSuffix: $"development-plan-completed:{planId:N}");

    // ── Performance: Competencies / Conversations / Strategic Framework / Templates ──
    public static RecordTenantActivityRequest ForCompetencyCreated(
        string tenantId, Guid competencyId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CompetencyCreated,
            $"Competency \"{name}\" was created",
            "competency", competencyId, name, currentUser,
            correlationSuffix: $"competency-created:{competencyId:N}");

    public static RecordTenantActivityRequest ForCompetencyUpdated(
        string tenantId, Guid competencyId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CompetencyUpdated,
            $"Competency \"{name}\" was updated",
            "competency", competencyId, name, currentUser,
            correlationSuffix: $"competency-updated:{competencyId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCompetencyDeleted(
        string tenantId, Guid competencyId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.CompetencyDeleted,
            $"Competency \"{name}\" was deleted",
            "competency", competencyId, name, currentUser,
            correlationSuffix: $"competency-deleted:{competencyId:N}");

    public static RecordTenantActivityRequest ForConversationCreated(
        string tenantId, Guid conversationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ConversationCreated,
            summary, "conversation", conversationId, summary, currentUser,
            correlationSuffix: $"conversation-created:{conversationId:N}");

    public static RecordTenantActivityRequest ForConversationNoteAdded(
        string tenantId, Guid conversationId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ConversationNoteAdded,
            summary, "conversation", conversationId, summary, actorUserId,
            correlationSuffix: $"conversation-note-added:{conversationId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForConversationActionItemAdded(
        string tenantId, Guid conversationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ConversationActionItemAdded,
            summary, "conversation", conversationId, summary, currentUser,
            correlationSuffix: $"conversation-action-item-added:{conversationId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForConversationCompleted(
        string tenantId, Guid conversationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.ConversationCompleted,
            summary, "conversation", conversationId, summary, currentUser,
            correlationSuffix: $"conversation-completed:{conversationId:N}");

    public static RecordTenantActivityRequest ForStrategicPerspectiveCreated(
        string tenantId, Guid perspectiveId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.StrategicPerspectiveCreated,
            $"Strategic perspective \"{name}\" was created",
            "strategic-perspective", perspectiveId, name, currentUser,
            correlationSuffix: $"strategic-perspective-created:{perspectiveId:N}");

    public static RecordTenantActivityRequest ForStrategicPerspectiveUpdated(
        string tenantId, Guid perspectiveId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.StrategicPerspectiveUpdated,
            $"Strategic perspective \"{name}\" was updated",
            "strategic-perspective", perspectiveId, name, currentUser,
            correlationSuffix: $"strategic-perspective-updated:{perspectiveId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForStrategicObjectiveCreated(
        string tenantId, Guid objectiveId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.StrategicObjectiveCreated,
            $"Strategic objective \"{name}\" was created",
            "strategic-objective", objectiveId, name, currentUser,
            correlationSuffix: $"strategic-objective-created:{objectiveId:N}");

    public static RecordTenantActivityRequest ForKpiCreated(
        string tenantId, Guid kpiId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.KpiCreated,
            $"KPI \"{name}\" was created",
            "kpi", kpiId, name, currentUser,
            correlationSuffix: $"kpi-created:{kpiId:N}");

    public static RecordTenantActivityRequest ForOuPerspectiveWeightCreated(
        string tenantId, Guid weightId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.OuPerspectiveWeightCreated,
            summary, "ou-perspective-weight", weightId, summary, currentUser,
            correlationSuffix: $"ou-perspective-weight-created:{weightId:N}");

    public static RecordTenantActivityRequest ForOuPerspectiveWeightUpdated(
        string tenantId, Guid weightId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.OuPerspectiveWeightUpdated,
            summary, "ou-perspective-weight", weightId, summary, currentUser,
            correlationSuffix: $"ou-perspective-weight-updated:{weightId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPerformanceTemplateCreated(
        string tenantId, Guid templateId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.PerformanceTemplateCreated,
            $"Performance template \"{name}\" was created",
            "performance-template", templateId, name, currentUser,
            correlationSuffix: $"performance-template-created:{templateId:N}");

    public static RecordTenantActivityRequest ForPerformanceTemplateUpdated(
        string tenantId, Guid templateId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.PerformanceTemplateUpdated,
            $"Performance template \"{name}\" was updated",
            "performance-template", templateId, name, currentUser,
            correlationSuffix: $"performance-template-updated:{templateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPerformanceTemplateDeleted(
        string tenantId, Guid templateId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.PerformanceTemplateDeleted,
            $"Performance template \"{name}\" was deleted",
            "performance-template", templateId, name, currentUser,
            correlationSuffix: $"performance-template-deleted:{templateId:N}");

    public static RecordTenantActivityRequest ForPerformanceRatingScaleCreated(
        string tenantId, Guid ratingScaleId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.PerformanceRatingScaleCreated,
            $"Performance rating scale \"{name}\" was created",
            "performance-rating-scale", ratingScaleId, name, currentUser,
            correlationSuffix: $"performance-rating-scale-created:{ratingScaleId:N}");

    public static RecordTenantActivityRequest ForPerformanceRatingScaleUpdated(
        string tenantId, Guid ratingScaleId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.PerformanceRatingScaleUpdated,
            $"Performance rating scale \"{name}\" was updated",
            "performance-rating-scale", ratingScaleId, name, currentUser,
            correlationSuffix: $"performance-rating-scale-updated:{ratingScaleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPerformanceRatingScaleDeleted(
        string tenantId, Guid ratingScaleId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrPerformanceActivityTypes.PerformanceRatingScaleDeleted,
            $"Performance rating scale \"{name}\" was deleted",
            "performance-rating-scale", ratingScaleId, name, currentUser,
            correlationSuffix: $"performance-rating-scale-deleted:{ratingScaleId:N}");

    // ── Talent ─────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForTalentReviewCreated(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewCreated,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-review-created:{reviewId:N}");

    public static RecordTenantActivityRequest ForTalentManagerReviewSubmitted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentManagerReviewSubmitted,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-manager-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForTalentHRReviewSubmitted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentHRReviewSubmitted,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-hr-review-submitted:{reviewId:N}");

    public static RecordTenantActivityRequest ForTalentReviewLinkedToPerformance(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewLinkedToPerformance,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-review-linked-to-performance:{reviewId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentReviewUpdated(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewUpdated,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-review-updated:{reviewId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentReviewCompleted(
        string tenantId, Guid reviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewCompleted,
            summary, "talent-review", reviewId, summary, currentUser,
            correlationSuffix: $"talent-review-completed:{reviewId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanCreated(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanCreated,
            $"Succession plan \"{title}\" was created",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-created:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanUpdated(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanUpdated,
            $"Succession plan \"{title}\" was updated",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-updated:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSuccessionPlanDeleted(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanDeleted,
            $"Succession plan \"{title}\" was deleted",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-deleted:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanActivated(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanActivated,
            $"Succession plan \"{title}\" was activated",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-activated:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanCompleted(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanCompleted,
            $"Succession plan \"{title}\" was completed",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-completed:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionPlanCancelled(
        string tenantId, Guid planId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionPlanCancelled,
            $"Succession plan \"{title}\" was cancelled",
            "succession-plan", planId, title, currentUser,
            correlationSuffix: $"succession-plan-cancelled:{planId:N}");

    public static RecordTenantActivityRequest ForSuccessionCandidateAdded(
        string tenantId, Guid planId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionCandidateAdded,
            $"{employeeName} added as succession candidate",
            "succession-plan", planId, employeeName, currentUser,
            correlationSuffix: $"succession-candidate-added:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSuccessionCandidateReadinessUpdated(
        string tenantId, Guid planId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionCandidateReadinessUpdated,
            $"Readiness updated for succession candidate {employeeName}",
            "succession-plan", planId, employeeName, currentUser,
            correlationSuffix: $"succession-candidate-readiness-updated:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSuccessionCandidateRemoved(
        string tenantId, Guid planId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.SuccessionCandidateRemoved,
            $"{employeeName} removed from succession plan",
            "succession-plan", planId, employeeName, currentUser,
            correlationSuffix: $"succession-candidate-removed:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentPipelineCreated(
        string tenantId, Guid pipelineId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentPipelineCreated,
            $"Talent pipeline \"{title}\" was created",
            "talent-pipeline", pipelineId, title, currentUser,
            correlationSuffix: $"talent-pipeline-created:{pipelineId:N}");

    public static RecordTenantActivityRequest ForTalentPipelineUpdated(
        string tenantId, Guid pipelineId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentPipelineUpdated,
            $"Talent pipeline \"{title}\" was updated",
            "talent-pipeline", pipelineId, title, currentUser,
            correlationSuffix: $"talent-pipeline-updated:{pipelineId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentPipelineStageUpdated(
        string tenantId, Guid pipelineId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentPipelineStageUpdated,
            $"Stage updated in talent pipeline \"{title}\"",
            "talent-pipeline", pipelineId, title, currentUser,
            correlationSuffix: $"talent-pipeline-stage-updated:{pipelineId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentNineBoxPositionUpdated(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentNineBoxPositionUpdated,
            $"Nine-box position updated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"talent-nine-box-position-updated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentEmployeeMarkedHighPotential(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentEmployeeMarkedHighPotential,
            $"{employeeName} was marked as high potential",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"talent-employee-marked-high-potential:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateCreated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateCreated,
            $"Talent review template \"{templateName}\" was created",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-created:{templateId:N}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateUpdated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateUpdated,
            $"Talent review template \"{templateName}\" was updated",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-updated:{templateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateDeleted(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateDeleted,
            $"Talent review template \"{templateName}\" was deleted",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-deleted:{templateId:N}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateActivated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateActivated,
            $"Talent review template \"{templateName}\" was activated",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-activated:{templateId:N}");

    public static RecordTenantActivityRequest ForTalentReviewTemplateDeactivated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrTalentActivityTypes.TalentReviewTemplateDeactivated,
            $"Talent review template \"{templateName}\" was deactivated",
            "talent-review-template", templateId, templateName, currentUser,
            correlationSuffix: $"talent-review-template-deactivated:{templateId:N}");

    // ── Leave extras ───────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForTravelPermissionCreated(
        string tenantId, Guid permissionId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.TravelPermissionCreated,
            summary, "travel-permission", permissionId, summary, actorUserId,
            correlationSuffix: $"travel-permission-created:{permissionId:N}");

    public static RecordTenantActivityRequest ForTravelPermissionApproved(
        string tenantId, Guid permissionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.TravelPermissionApproved,
            summary, "travel-permission", permissionId, summary, currentUser,
            correlationSuffix: $"travel-permission-approved:{permissionId:N}");

    public static RecordTenantActivityRequest ForTravelPermissionRejected(
        string tenantId, Guid permissionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.TravelPermissionRejected,
            summary, "travel-permission", permissionId, summary, currentUser,
            correlationSuffix: $"travel-permission-rejected:{permissionId:N}");

    public static RecordTenantActivityRequest ForEmployeeLeaveConfigured(
        string tenantId, Guid employeeId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.EmployeeLeaveConfigured,
            $"Leave configured for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"employee-leave-configured:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeLeaveBackfillCompleted(
        string tenantId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.EmployeeLeaveBackfillCompleted,
            summary, "leave-backfill", Guid.Empty, summary, currentUser,
            correlationSuffix: $"employee-leave-backfill-completed:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForLeaveRecallCreated(
        string tenantId, Guid recallId, string employeeName, string leaveTypeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.LeaveRecallCreated,
            $"Leave recall created for {employeeName} ({leaveTypeName})",
            "leave-recall", recallId, employeeName, currentUser,
            correlationSuffix: $"leave-recall-created:{recallId:N}");

    public static RecordTenantActivityRequest ForLeaveRequestAttachmentDeleted(
        string tenantId, Guid attachmentId, string fileName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.LeaveRequestAttachmentDeleted,
            $"Leave request attachment \"{fileName}\" was deleted",
            "leave-request-attachment", attachmentId, fileName, currentUser,
            correlationSuffix: $"leave-request-attachment-deleted:{attachmentId:N}");

    public static RecordTenantActivityRequest ForLeaveRequestAttachmentUploaded(
        string tenantId, Guid attachmentId, string fileName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.LeaveRequestAttachmentUploaded,
            $"Leave request attachment \"{fileName}\" was uploaded",
            "leave-request-attachment", attachmentId, fileName, currentUser,
            correlationSuffix: $"leave-request-attachment-uploaded:{attachmentId:N}");

    public static RecordTenantActivityRequest ForLeaveTypeUpdated(
        string tenantId, Guid leaveTypeId, string leaveTypeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLeaveActivityTypes.LeaveTypeUpdated,
            $"Leave type \"{leaveTypeName}\" was updated",
            "leave-type", leaveTypeId, leaveTypeName, currentUser,
            correlationSuffix: $"leave-type-updated:{leaveTypeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Recruitment ────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForApplicationSubmitted(
        string tenantId, Guid applicationId, string applicantName, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ApplicationSubmitted,
            $"{applicantName} submitted an application",
            "application", applicationId, applicantName, actorUserId,
            correlationSuffix: $"application-submitted:{applicationId:N}");

    public static RecordTenantActivityRequest ForInternalApplicationSubmitted(
        string tenantId, Guid applicationId, string applicantName, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.InternalApplicationSubmitted,
            $"{applicantName} submitted an internal application",
            "application", applicationId, applicantName, actorUserId,
            correlationSuffix: $"internal-application-submitted:{applicationId:N}");

    public static RecordTenantActivityRequest ForInterviewCompleted(
        string tenantId, Guid interviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.InterviewCompleted,
            summary, "interview", interviewId, summary, currentUser,
            correlationSuffix: $"interview-completed:{interviewId:N}");

    public static RecordTenantActivityRequest ForApplicationRejectedAfterInterview(
        string tenantId, Guid applicationId, string applicantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ApplicationRejectedAfterInterview,
            $"{applicantName}'s application was rejected after interview",
            "application", applicationId, applicantName, currentUser,
            correlationSuffix: $"application-rejected-after-interview:{applicationId:N}");

    public static RecordTenantActivityRequest ForOfferApprovalStepApproved(
        string tenantId, Guid offerId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.OfferApprovalStepApproved,
            summary, "offer", offerId, summary, currentUser,
            correlationSuffix: $"offer-approval-step-approved:{offerId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOfferFullyApproved(
        string tenantId, Guid offerId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.OfferFullyApproved,
            summary, "offer", offerId, summary, currentUser,
            correlationSuffix: $"offer-fully-approved:{offerId:N}");

    public static RecordTenantActivityRequest ForCandidateHired(
        string tenantId, Guid hireId, string candidateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.CandidateHired,
            $"{candidateName} was hired",
            "hire", hireId, candidateName, currentUser,
            correlationSuffix: $"candidate-hired:{hireId:N}");

    // ── Recruitment: Applications / Candidates / Interviews (2026-07-14) ────
    public static RecordTenantActivityRequest ForApplicationStageMoved(
        string tenantId, Guid applicationId, string applicantName, string stage, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ApplicationStageMoved,
            $"{applicantName}'s application was moved to stage \"{stage}\"",
            "application", applicationId, applicantName, currentUser,
            correlationSuffix: $"application-stage-moved:{applicationId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCandidateProfileCreated(
        string tenantId, Guid candidateId, string candidateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.CandidateProfileCreated,
            $"Candidate profile \"{candidateName}\" was created",
            "candidate", candidateId, candidateName, currentUser,
            correlationSuffix: $"candidate-profile-created:{candidateId:N}");

    public static RecordTenantActivityRequest ForCandidateExperienceAdded(
        string tenantId, Guid candidateId, string candidateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.CandidateExperienceAdded,
            $"Experience was added to candidate \"{candidateName}\"",
            "candidate", candidateId, candidateName, currentUser,
            correlationSuffix: $"candidate-experience-added:{candidateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForInterviewScheduled(
        string tenantId, Guid interviewId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.InterviewScheduled,
            summary, "interview", interviewId, summary, currentUser,
            correlationSuffix: $"interview-scheduled:{interviewId:N}");

    // ── Recruitment: Job Requisitions ────────────────────────────────────────
    public static RecordTenantActivityRequest ForJobRequisitionCreated(
        string tenantId, Guid requisitionId, string requisitionTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobRequisitionCreated,
            $"Job requisition \"{requisitionTitle}\" was created",
            "job-requisition", requisitionId, requisitionTitle, currentUser,
            correlationSuffix: $"job-requisition-created:{requisitionId:N}");

    public static RecordTenantActivityRequest ForJobRequisitionUpdated(
        string tenantId, Guid requisitionId, string requisitionTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobRequisitionUpdated,
            $"Job requisition \"{requisitionTitle}\" was updated",
            "job-requisition", requisitionId, requisitionTitle, currentUser,
            correlationSuffix: $"job-requisition-updated:{requisitionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForJobRequisitionApprovalStepApproved(
        string tenantId, Guid requisitionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobRequisitionApprovalStepApproved,
            summary, "job-requisition", requisitionId, summary, currentUser,
            correlationSuffix: $"job-requisition-approval-step-approved:{requisitionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForJobRequisitionApprovalStepAdded(
        string tenantId, Guid requisitionId, string requisitionTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobRequisitionApprovalStepAdded,
            $"An approval step was added to job requisition \"{requisitionTitle}\"",
            "job-requisition", requisitionId, requisitionTitle, currentUser,
            correlationSuffix: $"job-requisition-approval-step-added:{requisitionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForJobRequisitionRejected(
        string tenantId, Guid requisitionId, string requisitionTitle, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobRequisitionRejected,
            $"Job requisition \"{requisitionTitle}\" was rejected",
            "job-requisition", requisitionId, requisitionTitle, actorUserId,
            correlationSuffix: $"job-requisition-rejected:{requisitionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Recruitment: Jobs ─────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForJobCreated(
        string tenantId, Guid jobId, string jobTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobCreated,
            $"Job \"{jobTitle}\" was created",
            "job", jobId, jobTitle, currentUser,
            correlationSuffix: $"job-created:{jobId:N}");

    public static RecordTenantActivityRequest ForJobUpdated(
        string tenantId, Guid jobId, string jobTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobUpdated,
            $"Job \"{jobTitle}\" was updated",
            "job", jobId, jobTitle, currentUser,
            correlationSuffix: $"job-updated:{jobId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForJobPublished(
        string tenantId, Guid jobId, string jobTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobPublished,
            $"Job \"{jobTitle}\" was published",
            "job", jobId, jobTitle, currentUser,
            correlationSuffix: $"job-published:{jobId:N}");

    public static RecordTenantActivityRequest ForJobDuplicated(
        string tenantId, Guid jobId, string jobTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobDuplicated,
            $"Job \"{jobTitle}\" was duplicated",
            "job", jobId, jobTitle, currentUser,
            correlationSuffix: $"job-duplicated:{jobId:N}");

    public static RecordTenantActivityRequest ForJobPostingChannelAdded(
        string tenantId, Guid jobId, string jobTitle, string channel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.JobPostingChannelAdded,
            $"Posting channel \"{channel}\" was added to job \"{jobTitle}\"",
            "job", jobId, jobTitle, currentUser,
            correlationSuffix: $"job-posting-channel-added:{jobId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Recruitment: Offers / Scorecards ──────────────────────────────────────
    public static RecordTenantActivityRequest ForOfferCreated(
        string tenantId, Guid offerId, string applicantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.OfferCreated,
            $"An offer was created for {applicantName}",
            "offer", offerId, applicantName, currentUser,
            correlationSuffix: $"offer-created:{offerId:N}");

    public static RecordTenantActivityRequest ForScorecardCreated(
        string tenantId, Guid scorecardId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ScorecardCreated,
            summary, "scorecard", scorecardId, summary, actorUserId,
            correlationSuffix: $"scorecard-created:{scorecardId:N}");

    // ── Recruitment: Screening Rules ───────────────────────────────────────────
    public static RecordTenantActivityRequest ForScreeningRuleCreated(
        string tenantId, Guid ruleId, string ruleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ScreeningRuleCreated,
            $"Screening rule \"{ruleName}\" was created",
            "screening-rule", ruleId, ruleName, currentUser,
            correlationSuffix: $"screening-rule-created:{ruleId:N}");

    public static RecordTenantActivityRequest ForScreeningRuleUpdated(
        string tenantId, Guid ruleId, string ruleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ScreeningRuleUpdated,
            $"Screening rule \"{ruleName}\" was updated",
            "screening-rule", ruleId, ruleName, currentUser,
            correlationSuffix: $"screening-rule-updated:{ruleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForScreeningRuleDeleted(
        string tenantId, Guid ruleId, string ruleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ScreeningRuleDeleted,
            $"Screening rule \"{ruleName}\" was deleted",
            "screening-rule", ruleId, ruleName, currentUser,
            correlationSuffix: $"screening-rule-deleted:{ruleId:N}");

    public static RecordTenantActivityRequest ForScreeningRuleToggled(
        string tenantId, Guid ruleId, string ruleName, bool isActive, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ScreeningRuleToggled,
            $"Screening rule \"{ruleName}\" was {(isActive ? "activated" : "deactivated")}",
            "screening-rule", ruleId, ruleName, currentUser,
            correlationSuffix: $"screening-rule-toggled:{ruleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadataJson: $$"""{"isActive":{{(isActive ? "true" : "false")}}}""");

    public static RecordTenantActivityRequest ForScreeningRuleDuplicated(
        string tenantId, Guid ruleId, string ruleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrRecruitmentActivityTypes.ScreeningRuleDuplicated,
            $"Screening rule \"{ruleName}\" was duplicated",
            "screening-rule", ruleId, ruleName, currentUser,
            correlationSuffix: $"screening-rule-duplicated:{ruleId:N}");

    // ── Employee Engagement: Disciplinary cases ─────────────────────────────
    public static RecordTenantActivityRequest ForDisciplinaryCaseCreated(
        string tenantId, Guid caseId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseCreated,
            $"Disciplinary case {caseCode} was created for {employeeName}",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-created:{caseId:N}");

    public static RecordTenantActivityRequest ForDisciplinaryCaseInterdicted(
        string tenantId, Guid caseId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseInterdicted,
            $"{employeeName} was interdicted for disciplinary case {caseCode}",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-interdicted:{caseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDisciplinaryCaseDecisionMade(
        string tenantId, Guid caseId, string caseCode, string employeeName, string actionTaken, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseDecisionMade,
            $"Decision \"{actionTaken}\" was made for disciplinary case {caseCode} ({employeeName})",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-decision-made:{caseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDisciplinaryCaseBonusWithheld(
        string tenantId, Guid caseId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseBonusWithheld,
            $"Bonus was withheld for {employeeName} (case {caseCode})",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-bonus-withheld:{caseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDisciplinaryCaseBonusReleased(
        string tenantId, Guid caseId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseBonusReleased,
            $"Bonus was released for {employeeName} (case {caseCode})",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-bonus-released:{caseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDisciplinaryCaseHearingInvitationGenerated(
        string tenantId, Guid caseId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseHearingInvitationGenerated,
            $"Hearing invitation was generated for {employeeName} (case {caseCode})",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-hearing-invitation-generated:{caseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDisciplinaryCaseInterdictionLetterGenerated(
        string tenantId, Guid caseId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseInterdictionLetterGenerated,
            $"Interdiction letter was generated for {employeeName} (case {caseCode})",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-interdiction-letter-generated:{caseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDisciplinaryCaseAuditReportRedacted(
        string tenantId, Guid caseId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseAuditReportRedacted,
            $"Audit report was redacted for disciplinary case {caseCode} ({employeeName})",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-audit-report-redacted:{caseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDisciplinaryCaseAuditReportUploaded(
        string tenantId, Guid caseId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseAuditReportUploaded,
            $"Audit report was uploaded for disciplinary case {caseCode} ({employeeName})",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-audit-report-uploaded:{caseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDisciplinaryCaseHearingAudioUploaded(
        string tenantId, Guid caseId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.DisciplinaryCaseHearingAudioUploaded,
            $"Hearing audio was uploaded for disciplinary case {caseCode} ({employeeName})",
            "disciplinary-case", caseId, employeeName, currentUser,
            correlationSuffix: $"disciplinary-case-hearing-audio-uploaded:{caseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Employee Engagement: Wellbeing ──────────────────────────────────────
    public static RecordTenantActivityRequest ForRecognitionCreated(
        string tenantId, Guid recognitionId, string toEmployeeName, string fromEmployeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.RecognitionCreated,
            $"{fromEmployeeName} recognized {toEmployeeName}",
            "recognition", recognitionId, toEmployeeName, currentUser,
            correlationSuffix: $"recognition-created:{recognitionId:N}");

    // ── Employee Engagement: Health ─────────────────────────────────────────
    public static RecordTenantActivityRequest ForHealthIssueCreated(
        string tenantId, Guid healthIssueId, string caseCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrEngagementActivityTypes.HealthIssueCreated,
            $"Health issue {caseCode} was created for {employeeName}",
            "health-issue", healthIssueId, employeeName, currentUser,
            correlationSuffix: $"health-issue-created:{healthIssueId:N}");

    // ── Surveys ──────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForSurveyCreated(
        string tenantId, Guid surveyId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyCreated,
            $"Survey \"{title}\" was created",
            "survey", surveyId, title, currentUser,
            correlationSuffix: $"survey-created:{surveyId:N}");

    public static RecordTenantActivityRequest ForSurveyUpdated(
        string tenantId, Guid surveyId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyUpdated,
            $"Survey \"{title}\" was updated",
            "survey", surveyId, title, currentUser,
            correlationSuffix: $"survey-updated:{surveyId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSurveyDeleted(
        string tenantId, Guid surveyId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyDeleted,
            $"Survey \"{title}\" was deleted",
            "survey", surveyId, title, currentUser,
            correlationSuffix: $"survey-deleted:{surveyId:N}");

    public static RecordTenantActivityRequest ForSurveyPublished(
        string tenantId, Guid surveyId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyPublished,
            $"Survey \"{title}\" was published",
            "survey", surveyId, title, currentUser,
            correlationSuffix: $"survey-published:{surveyId:N}");

    public static RecordTenantActivityRequest ForSurveyClosed(
        string tenantId, Guid surveyId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyClosed,
            $"Survey \"{title}\" was closed",
            "survey", surveyId, title, currentUser,
            correlationSuffix: $"survey-closed:{surveyId:N}");

    public static RecordTenantActivityRequest ForSurveyArchived(
        string tenantId, Guid surveyId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyArchived,
            $"Survey \"{title}\" was archived",
            "survey", surveyId, title, currentUser,
            correlationSuffix: $"survey-archived:{surveyId:N}");

    public static RecordTenantActivityRequest ForSurveyCloned(
        string tenantId, Guid newSurveyId, string newTitle, string sourceTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyCloned,
            $"Survey \"{sourceTitle}\" was cloned to \"{newTitle}\"",
            "survey", newSurveyId, newTitle, currentUser,
            correlationSuffix: $"survey-cloned:{newSurveyId:N}");

    public static RecordTenantActivityRequest ForSurveyDistributed(
        string tenantId, Guid surveyId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyDistributed,
            $"Survey \"{title}\" was distributed",
            "survey", surveyId, title, currentUser,
            correlationSuffix: $"survey-distributed:{surveyId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSurveyReminderSent(
        string tenantId, Guid surveyId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyReminderSent,
            $"Reminder sent for survey \"{title}\"",
            "survey", surveyId, title, currentUser,
            correlationSuffix: $"survey-reminder-sent:{surveyId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSurveyQuestionAdded(
        string tenantId, Guid questionId, string questionText, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyQuestionAdded,
            $"Question \"{questionText}\" was added",
            "survey-question", questionId, questionText, currentUser,
            correlationSuffix: $"survey-question-added:{questionId:N}");

    public static RecordTenantActivityRequest ForSurveyQuestionRemoved(
        string tenantId, Guid questionId, string questionText, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyQuestionRemoved,
            $"Question \"{questionText}\" was removed",
            "survey-question", questionId, questionText, currentUser,
            correlationSuffix: $"survey-question-removed:{questionId:N}");

    public static RecordTenantActivityRequest ForSurveyQuestionsReordered(
        string tenantId, Guid surveyId, string surveyTitle, int count, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyQuestionsReordered,
            $"{count} question{(count == 1 ? "" : "s")} reordered on survey \"{surveyTitle}\"",
            "survey", surveyId, surveyTitle, currentUser,
            correlationSuffix: $"survey-questions-reordered:{surveyId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSurveyQuestionUpdated(
        string tenantId, Guid questionId, string questionText, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyQuestionUpdated,
            $"Question \"{questionText}\" was updated",
            "survey-question", questionId, questionText, currentUser,
            correlationSuffix: $"survey-question-updated:{questionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForShareLinkGenerated(
        string tenantId, Guid surveyId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.ShareLinkGenerated,
            $"Share link generated for survey \"{title}\"",
            "survey", surveyId, title, currentUser,
            correlationSuffix: $"share-link-generated:{surveyId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSurveyLookupCreated(
        string tenantId, Guid lookupId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyLookupCreated,
            $"Lookup \"{name}\" was created",
            "survey-lookup", lookupId, name, currentUser,
            correlationSuffix: $"survey-lookup-created:{lookupId:N}");

    public static RecordTenantActivityRequest ForSurveyLookupUpdated(
        string tenantId, Guid lookupId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyLookupUpdated,
            $"Lookup \"{name}\" was updated",
            "survey-lookup", lookupId, name, currentUser,
            correlationSuffix: $"survey-lookup-updated:{lookupId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSurveyLookupDeleted(
        string tenantId, Guid lookupId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyLookupDeleted,
            $"Lookup \"{name}\" was deleted",
            "survey-lookup", lookupId, name, currentUser,
            correlationSuffix: $"survey-lookup-deleted:{lookupId:N}");

    public static RecordTenantActivityRequest ForSurveyResponseSubmitted(
        string tenantId, Guid responseId, string surveyTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveyResponseSubmitted,
            $"A response was submitted for survey \"{surveyTitle}\"",
            "survey-response", responseId, surveyTitle, currentUser,
            correlationSuffix: $"survey-response-submitted:{responseId:N}");

    public static RecordTenantActivityRequest ForSurveysBulkOperated(
        string tenantId, string operationLabel, int affectedCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, SurveyActivityTypes.SurveysBulkOperated,
            $"{operationLabel} applied to {affectedCount} survey{(affectedCount == 1 ? "" : "s")}",
            "surveys-bulk-operation", Guid.Empty, operationLabel, currentUser,
            correlationSuffix: $"surveys-bulk-operated:{operationLabel.ToLowerInvariant()}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Benefit: Accommodations ─────────────────────────────────────────────
    public static RecordTenantActivityRequest ForAccommodationCreated(
        string tenantId, Guid accommodationId, string accommodationCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.AccommodationCreated,
            $"Accommodation \"{accommodationCode}\" was created",
            "accommodation", accommodationId, accommodationCode, currentUser,
            correlationSuffix: $"accommodation-created:{accommodationId:N}");

    public static RecordTenantActivityRequest ForAccommodationAllocated(
        string tenantId, Guid accommodationId, string accommodationCode, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.AccommodationAllocated,
            $"Accommodation \"{accommodationCode}\" was allocated to {employeeName}",
            "accommodation", accommodationId, accommodationCode, currentUser,
            correlationSuffix: $"accommodation-allocated:{accommodationId:N}");

    public static RecordTenantActivityRequest ForAccommodationVacated(
        string tenantId, Guid accommodationId, string accommodationCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.AccommodationVacated,
            $"Accommodation \"{accommodationCode}\" was vacated",
            "accommodation", accommodationId, accommodationCode, currentUser,
            correlationSuffix: $"accommodation-vacated:{accommodationId:N}");

    public static RecordTenantActivityRequest ForBenefitPlanActivated(
        string tenantId, Guid planId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitPlanActivated,
            $"Benefit plan \"{planName}\" was activated",
            "benefit-plan", planId, planName, currentUser,
            correlationSuffix: $"benefit-plan-activated:{planId:N}");

    public static RecordTenantActivityRequest ForBenefitPlanCreated(
        string tenantId, Guid planId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitPlanCreated,
            $"Benefit plan \"{planName}\" was created",
            "benefit-plan", planId, planName, currentUser,
            correlationSuffix: $"benefit-plan-created:{planId:N}");

    public static RecordTenantActivityRequest ForBenefitPlanDeactivated(
        string tenantId, Guid planId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitPlanDeactivated,
            $"Benefit plan \"{planName}\" was deactivated",
            "benefit-plan", planId, planName, currentUser,
            correlationSuffix: $"benefit-plan-deactivated:{planId:N}");

    public static RecordTenantActivityRequest ForBenefitPlanDeleted(
        string tenantId, Guid planId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitPlanDeleted,
            $"Benefit plan \"{planName}\" was deleted",
            "benefit-plan", planId, planName, currentUser,
            correlationSuffix: $"benefit-plan-deleted:{planId:N}");

    public static RecordTenantActivityRequest ForBenefitPlanUpdated(
        string tenantId, Guid planId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitPlanUpdated,
            $"Benefit plan \"{planName}\" was updated",
            "benefit-plan", planId, planName, currentUser,
            correlationSuffix: $"benefit-plan-updated:{planId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForBenefitTypeCreated(
        string tenantId, Guid benefitTypeId, string benefitTypeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitTypeCreated,
            $"Benefit type \"{benefitTypeName}\" was created",
            "benefit-type", benefitTypeId, benefitTypeName, currentUser,
            correlationSuffix: $"benefit-type-created:{benefitTypeId:N}");

    public static RecordTenantActivityRequest ForBenefitTypeDeleted(
        string tenantId, Guid benefitTypeId, string benefitTypeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitTypeDeleted,
            $"Benefit type \"{benefitTypeName}\" was deleted",
            "benefit-type", benefitTypeId, benefitTypeName, currentUser,
            correlationSuffix: $"benefit-type-deleted:{benefitTypeId:N}");

    public static RecordTenantActivityRequest ForBenefitTypeUpdated(
        string tenantId, Guid benefitTypeId, string benefitTypeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitTypeUpdated,
            $"Benefit type \"{benefitTypeName}\" was updated",
            "benefit-type", benefitTypeId, benefitTypeName, currentUser,
            correlationSuffix: $"benefit-type-updated:{benefitTypeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForAutoEnrollmentConfigUpdated(
        string tenantId, Guid configId, string configLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.AutoEnrollmentConfigUpdated,
            $"{configLabel} was updated",
            "enrollment-configuration", configId, configLabel, currentUser,
            correlationSuffix: $"auto-enrollment-config-updated:{configId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForLifeEventConfigUpdated(
        string tenantId, Guid configId, string configLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.LifeEventConfigUpdated,
            $"{configLabel} was updated",
            "enrollment-configuration", configId, configLabel, currentUser,
            correlationSuffix: $"life-event-config-updated:{configId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForManualEnrollmentConfigUpdated(
        string tenantId, Guid configId, string configLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.ManualEnrollmentConfigUpdated,
            $"{configLabel} was updated",
            "enrollment-configuration", configId, configLabel, currentUser,
            correlationSuffix: $"manual-enrollment-config-updated:{configId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForOpenEnrollmentConfigUpdated(
        string tenantId, Guid configId, string configLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.OpenEnrollmentConfigUpdated,
            $"{configLabel} was updated",
            "enrollment-configuration", configId, configLabel, currentUser,
            correlationSuffix: $"open-enrollment-config-updated:{configId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDependentCreated(
        string tenantId, Guid dependentId, string dependentName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.DependentCreated,
            $"Dependent \"{dependentName}\" was added",
            "benefit-dependent", dependentId, dependentName, currentUser,
            correlationSuffix: $"dependent-created:{dependentId:N}");

    public static RecordTenantActivityRequest ForDependentDeleted(
        string tenantId, Guid dependentId, string dependentName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.DependentDeleted,
            $"Dependent \"{dependentName}\" was removed",
            "benefit-dependent", dependentId, dependentName, currentUser,
            correlationSuffix: $"dependent-deleted:{dependentId:N}");

    public static RecordTenantActivityRequest ForDependentUpdated(
        string tenantId, Guid dependentId, string dependentName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.DependentUpdated,
            $"Dependent \"{dependentName}\" was updated",
            "benefit-dependent", dependentId, dependentName, currentUser,
            correlationSuffix: $"dependent-updated:{dependentId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForBenefitEnrollmentCreated(
        string tenantId, Guid enrollmentId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitEnrollmentCreated,
            $"{employeeName} was enrolled in a benefit plan",
            "benefit-enrollment", enrollmentId, employeeName, currentUser,
            correlationSuffix: $"benefit-enrollment-created:{enrollmentId:N}");

    public static RecordTenantActivityRequest ForBenefitEnrollmentTerminated(
        string tenantId, Guid enrollmentId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitEnrollmentTerminated,
            $"{employeeName}'s benefit enrollment was terminated",
            "benefit-enrollment", enrollmentId, employeeName, currentUser,
            correlationSuffix: $"benefit-enrollment-terminated:{enrollmentId:N}");

    public static RecordTenantActivityRequest ForBenefitEnrollmentUpdated(
        string tenantId, Guid enrollmentId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitEnrollmentUpdated,
            $"{employeeName}'s benefit enrollment was updated",
            "benefit-enrollment", enrollmentId, employeeName, currentUser,
            correlationSuffix: $"benefit-enrollment-updated:{enrollmentId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForHouseCategoryCreated(
        string tenantId, Guid categoryId, string categoryName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.HouseCategoryCreated,
            $"House category \"{categoryName}\" was created",
            "house-category", categoryId, categoryName, currentUser,
            correlationSuffix: $"house-category-created:{categoryId:N}");

    public static RecordTenantActivityRequest ForHouseCategoryDeleted(
        string tenantId, Guid categoryId, string categoryName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.HouseCategoryDeleted,
            $"House category \"{categoryName}\" was deleted",
            "house-category", categoryId, categoryName, currentUser,
            correlationSuffix: $"house-category-deleted:{categoryId:N}");

    public static RecordTenantActivityRequest ForHouseCategoryUpdated(
        string tenantId, Guid categoryId, string categoryName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.HouseCategoryUpdated,
            $"House category \"{categoryName}\" was updated",
            "house-category", categoryId, categoryName, currentUser,
            correlationSuffix: $"house-category-updated:{categoryId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForBenefitLoanCreated(
        string tenantId, Guid loanId, string loanCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitLoanCreated,
            $"Loan \"{loanCode}\" was created",
            "benefit-loan", loanId, loanCode, currentUser,
            correlationSuffix: $"benefit-loan-created:{loanId:N}");

    public static RecordTenantActivityRequest ForBenefitLoanRepaymentRecorded(
        string tenantId, Guid loanId, string loanCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitLoanRepaymentRecorded,
            $"A repayment was recorded for loan \"{loanCode}\"",
            "benefit-loan", loanId, loanCode, currentUser,
            correlationSuffix: $"benefit-loan-repayment-recorded:{loanId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForBenefitLoanStatusUpdated(
        string tenantId, Guid loanId, string loanCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitLoanStatusUpdated,
            $"Loan \"{loanCode}\" status was updated",
            "benefit-loan", loanId, loanCode, currentUser,
            correlationSuffix: $"benefit-loan-status-updated:{loanId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForBenefitLookupCreated(
        string tenantId, Guid lookupId, string lookupName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitLookupCreated,
            $"Lookup value \"{lookupName}\" was created",
            "benefit-lookup", lookupId, lookupName, currentUser,
            correlationSuffix: $"benefit-lookup-created:{lookupId:N}");

    public static RecordTenantActivityRequest ForBenefitLookupDeleted(
        string tenantId, Guid lookupId, string lookupName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitLookupDeleted,
            $"Lookup value \"{lookupName}\" was deleted",
            "benefit-lookup", lookupId, lookupName, currentUser,
            correlationSuffix: $"benefit-lookup-deleted:{lookupId:N}");

    public static RecordTenantActivityRequest ForBenefitLookupUpdated(
        string tenantId, Guid lookupId, string lookupName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.BenefitLookupUpdated,
            $"Lookup value \"{lookupName}\" was updated",
            "benefit-lookup", lookupId, lookupName, currentUser,
            correlationSuffix: $"benefit-lookup-updated:{lookupId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenancyAgreementCreated(
        string tenantId, Guid agreementId, string agreementCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.TenancyAgreementCreated,
            $"Tenancy agreement \"{agreementCode}\" was created",
            "tenancy-agreement", agreementId, agreementCode, currentUser,
            correlationSuffix: $"tenancy-agreement-created:{agreementId:N}");

    public static RecordTenantActivityRequest ForTenancyAgreementDocumentAdded(
        string tenantId, Guid agreementId, string agreementCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.TenancyAgreementDocumentAdded,
            $"A document was added to tenancy agreement \"{agreementCode}\"",
            "tenancy-agreement", agreementId, agreementCode, currentUser,
            correlationSuffix: $"tenancy-agreement-document-added:{agreementId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenancyAgreementDocumentRemoved(
        string tenantId, Guid agreementId, string agreementCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.TenancyAgreementDocumentRemoved,
            $"A document was removed from tenancy agreement \"{agreementCode}\"",
            "tenancy-agreement", agreementId, agreementCode, currentUser,
            correlationSuffix: $"tenancy-agreement-document-removed:{agreementId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenancyAgreementRenewed(
        string tenantId, Guid agreementId, string agreementCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.TenancyAgreementRenewed,
            $"Tenancy agreement \"{agreementCode}\" was renewed",
            "tenancy-agreement", agreementId, agreementCode, currentUser,
            correlationSuffix: $"tenancy-agreement-renewed:{agreementId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForWaitingListEntryAdded(
        string tenantId, Guid waitingListId, string waitingListCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, BenefitActivityTypes.WaitingListEntryAdded,
            $"Waiting list entry \"{waitingListCode}\" was added",
            "waiting-list-entry", waitingListId, waitingListCode, currentUser,
            correlationSuffix: $"waiting-list-entry-added:{waitingListId:N}");

    // ── Learning ───────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForCourseCreated(
        string tenantId, Guid courseId, string courseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseCreated,
            $"Course \"{courseName}\" was created",
            "course", courseId, courseName, currentUser,
            correlationSuffix: $"course-created:{courseId:N}");

    public static RecordTenantActivityRequest ForCoursePublished(
        string tenantId, Guid courseId, string courseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CoursePublished,
            $"Course \"{courseName}\" was published",
            "course", courseId, courseName, currentUser,
            correlationSuffix: $"course-published:{courseId:N}");

    public static RecordTenantActivityRequest ForEnrollmentCreated(
        string tenantId, Guid enrollmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.EnrollmentCreated,
            summary, "enrollment", enrollmentId, summary, currentUser,
            correlationSuffix: $"enrollment-created:{enrollmentId:N}");

    public static RecordTenantActivityRequest ForEnrollmentApproved(
        string tenantId, Guid enrollmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.EnrollmentApproved,
            summary, "enrollment", enrollmentId, summary, currentUser,
            correlationSuffix: $"enrollment-approved:{enrollmentId:N}");

    public static RecordTenantActivityRequest ForEnrollmentCompleted(
        string tenantId, Guid enrollmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.EnrollmentCompleted,
            summary, "enrollment", enrollmentId, summary, currentUser,
            correlationSuffix: $"enrollment-completed:{enrollmentId:N}");

    public static RecordTenantActivityRequest ForTranscriptUploaded(
        string tenantId, Guid transcriptId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.TranscriptUploaded,
            summary, "transcript", transcriptId, summary, currentUser,
            correlationSuffix: $"transcript-uploaded:{transcriptId:N}");

    public static RecordTenantActivityRequest ForCertificationUploaded(
        string tenantId, Guid certificationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CertificationUploaded,
            summary, "certification", certificationId, summary, currentUser,
            correlationSuffix: $"certification-uploaded:{certificationId:N}");

    public static RecordTenantActivityRequest ForLearningPathEnrolled(
        string tenantId, Guid employeeId, string employeeName, int courseCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.LearningPathEnrolled,
            $"{employeeName} was enrolled in a learning path ({courseCount} course{(courseCount == 1 ? "" : "s")})",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"learning-path-enrolled:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTrainingPaymentRequestCreated(
        string tenantId, Guid paymentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.TrainingPaymentRequestCreated,
            summary, "training-payment-request", paymentId, summary, currentUser,
            correlationSuffix: $"training-payment-request-created:{paymentId:N}");

    public static RecordTenantActivityRequest ForTrainingPaymentRequestApproved(
        string tenantId, Guid paymentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.TrainingPaymentRequestApproved,
            summary, "training-payment-request", paymentId, summary, currentUser,
            correlationSuffix: $"training-payment-request-approved:{paymentId:N}");

    public static RecordTenantActivityRequest ForTrainingPaymentMarkedPaid(
        string tenantId, Guid paymentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.TrainingPaymentMarkedPaid,
            summary, "training-payment-request", paymentId, summary, currentUser,
            correlationSuffix: $"training-payment-marked-paid:{paymentId:N}");

    public static RecordTenantActivityRequest ForTrainingRefundRequestCreated(
        string tenantId, Guid refundId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.TrainingRefundRequestCreated,
            summary, "training-refund-request", refundId, summary, currentUser,
            correlationSuffix: $"training-refund-request-created:{refundId:N}");

    public static RecordTenantActivityRequest ForTrainingRefundRequestApproved(
        string tenantId, Guid refundId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.TrainingRefundRequestApproved,
            summary, "training-refund-request", refundId, summary, currentUser,
            correlationSuffix: $"training-refund-request-approved:{refundId:N}");

    public static RecordTenantActivityRequest ForTrainingRefundMarkedProcessed(
        string tenantId, Guid refundId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.TrainingRefundMarkedProcessed,
            summary, "training-refund-request", refundId, summary, currentUser,
            correlationSuffix: $"training-refund-marked-processed:{refundId:N}");

    public static RecordTenantActivityRequest ForProfessionalSubscriptionCreated(
        string tenantId, Guid subscriptionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.ProfessionalSubscriptionCreated,
            summary, "professional-subscription", subscriptionId, summary, currentUser,
            correlationSuffix: $"professional-subscription-created:{subscriptionId:N}");

    public static RecordTenantActivityRequest ForProfessionalSubscriptionApproved(
        string tenantId, Guid subscriptionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.ProfessionalSubscriptionApproved,
            summary, "professional-subscription", subscriptionId, summary, currentUser,
            correlationSuffix: $"professional-subscription-approved:{subscriptionId:N}");

    public static RecordTenantActivityRequest ForProfessionalSubscriptionMarkedPaid(
        string tenantId, Guid subscriptionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.ProfessionalSubscriptionMarkedPaid,
            summary, "professional-subscription", subscriptionId, summary, currentUser,
            correlationSuffix: $"professional-subscription-marked-paid:{subscriptionId:N}");

    // ── Learning: Courses ────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForCourseUpdated(
        string tenantId, Guid courseId, string courseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseUpdated,
            $"Course \"{courseName}\" was updated",
            "course", courseId, courseName, currentUser,
            correlationSuffix: $"course-updated:{courseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCourseDeleted(
        string tenantId, Guid courseId, string courseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseDeleted,
            $"Course \"{courseName}\" was deleted",
            "course", courseId, courseName, currentUser,
            correlationSuffix: $"course-deleted:{courseId:N}");

    public static RecordTenantActivityRequest ForCourseArchived(
        string tenantId, Guid courseId, string courseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseArchived,
            $"Course \"{courseName}\" was archived",
            "course", courseId, courseName, currentUser,
            correlationSuffix: $"course-archived:{courseId:N}");

    public static RecordTenantActivityRequest ForCourseRestored(
        string tenantId, Guid courseId, string courseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseRestored,
            $"Course \"{courseName}\" was restored from trash",
            "course", courseId, courseName, currentUser,
            correlationSuffix: $"course-restored:{courseId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCourseDuplicated(
        string tenantId, Guid newCourseId, string newCourseName, string sourceCourseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseDuplicated,
            $"Course \"{newCourseName}\" was duplicated from \"{sourceCourseName}\"",
            "course", newCourseId, newCourseName, currentUser,
            correlationSuffix: $"course-duplicated:{newCourseId:N}");

    // ── Learning: Assessments ────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForAssessmentQuestionAdded(
        string tenantId, Guid questionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.AssessmentQuestionAdded,
            summary, "assessment-question", questionId, summary, currentUser,
            correlationSuffix: $"assessment-question-added:{questionId:N}");

    public static RecordTenantActivityRequest ForAssessmentCreated(
        string tenantId, Guid assessmentId, string assessmentTitle, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.AssessmentCreated,
            $"Assessment \"{assessmentTitle}\" was created",
            "assessment", assessmentId, assessmentTitle, currentUser,
            correlationSuffix: $"assessment-created:{assessmentId:N}");

    public static RecordTenantActivityRequest ForAssessmentAttemptStarted(
        string tenantId, Guid attemptId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.AssessmentAttemptStarted,
            summary, "assessment-attempt", attemptId, summary, currentUser,
            correlationSuffix: $"assessment-attempt-started:{attemptId:N}");

    public static RecordTenantActivityRequest ForAssessmentAttemptSubmitted(
        string tenantId, Guid attemptId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.AssessmentAttemptSubmitted,
            summary, "assessment-attempt", attemptId, summary, currentUser,
            correlationSuffix: $"assessment-attempt-submitted:{attemptId:N}");

    // ── Learning: Certificates ───────────────────────────────────────────────
    public static RecordTenantActivityRequest ForCertificateRevoked(
        string tenantId, Guid certificateId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CertificateRevoked,
            summary, "certificate", certificateId, summary, currentUser,
            correlationSuffix: $"certificate-revoked:{certificateId:N}");

    // ── Learning: Content management ─────────────────────────────────────────
    public static RecordTenantActivityRequest ForCourseContentAdded(
        string tenantId, Guid contentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseContentAdded,
            summary, "course-content", contentId, summary, currentUser,
            correlationSuffix: $"course-content-added:{contentId:N}");

    public static RecordTenantActivityRequest ForCourseModuleAdded(
        string tenantId, Guid moduleId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseModuleAdded,
            summary, "course-module", moduleId, summary, currentUser,
            correlationSuffix: $"course-module-added:{moduleId:N}");

    public static RecordTenantActivityRequest ForCourseContentDeleted(
        string tenantId, Guid contentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseContentDeleted,
            summary, "course-content", contentId, summary, currentUser,
            correlationSuffix: $"course-content-deleted:{contentId:N}");

    public static RecordTenantActivityRequest ForCourseModuleDeleted(
        string tenantId, Guid moduleId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseModuleDeleted,
            summary, "course-module", moduleId, summary, currentUser,
            correlationSuffix: $"course-module-deleted:{moduleId:N}");

    public static RecordTenantActivityRequest ForCourseContentUpdated(
        string tenantId, Guid contentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseContentUpdated,
            summary, "course-content", contentId, summary, currentUser,
            correlationSuffix: $"course-content-updated:{contentId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForCourseModuleUpdated(
        string tenantId, Guid moduleId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseModuleUpdated,
            summary, "course-module", moduleId, summary, currentUser,
            correlationSuffix: $"course-module-updated:{moduleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Learning: Enrollments ────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForEnrollmentStarted(
        string tenantId, Guid enrollmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.EnrollmentStarted,
            summary, "enrollment", enrollmentId, summary, currentUser,
            correlationSuffix: $"enrollment-started:{enrollmentId:N}");

    // ── Learning: Learning paths ─────────────────────────────────────────────
    public static RecordTenantActivityRequest ForLearningPathCreated(
        string tenantId, Guid learningPathId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.LearningPathCreated,
            $"Learning path \"{title}\" was created",
            "learning-path", learningPathId, title, currentUser,
            correlationSuffix: $"learning-path-created:{learningPathId:N}");

    public static RecordTenantActivityRequest ForLearningPathDeleted(
        string tenantId, Guid learningPathId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.LearningPathDeleted,
            $"Learning path \"{title}\" was deleted",
            "learning-path", learningPathId, title, currentUser,
            correlationSuffix: $"learning-path-deleted:{learningPathId:N}");

    public static RecordTenantActivityRequest ForLearningPathUpdated(
        string tenantId, Guid learningPathId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.LearningPathUpdated,
            $"Learning path \"{title}\" was updated",
            "learning-path", learningPathId, title, currentUser,
            correlationSuffix: $"learning-path-updated:{learningPathId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Learning: Progress tracking ──────────────────────────────────────────
    public static RecordTenantActivityRequest ForCourseContentProgressCompleted(
        string tenantId, Guid progressId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseContentProgressCompleted,
            summary, "course-content-progress", progressId, summary, currentUser,
            correlationSuffix: $"course-content-progress-completed:{progressId:N}");

    public static RecordTenantActivityRequest ForCourseContentProgressStarted(
        string tenantId, Guid progressId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseContentProgressStarted,
            summary, "course-content-progress", progressId, summary, currentUser,
            correlationSuffix: $"course-content-progress-started:{progressId:N}");

    public static RecordTenantActivityRequest ForLessonCompleted(
        string tenantId, Guid lessonId, Guid progressId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.LessonCompleted,
            summary, "lesson", lessonId, summary, currentUser,
            correlationSuffix: $"lesson-completed:{progressId:N}");

    public static RecordTenantActivityRequest ForQuizCompleted(
        string tenantId, Guid quizId, Guid attemptId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.QuizCompleted,
            summary, "quiz", quizId, summary, currentUser,
            correlationSuffix: $"quiz-completed:{attemptId:N}");

    // ── Learning: Recommendations ────────────────────────────────────────────
    public static RecordTenantActivityRequest ForCourseRecommendationAccepted(
        string tenantId, Guid recommendationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseRecommendationAccepted,
            summary, "course-recommendation", recommendationId, summary, currentUser,
            correlationSuffix: $"course-recommendation-accepted:{recommendationId:N}");

    public static RecordTenantActivityRequest ForCourseRecommendationRejected(
        string tenantId, Guid recommendationId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseRecommendationRejected,
            summary, "course-recommendation", recommendationId, summary, currentUser,
            correlationSuffix: $"course-recommendation-rejected:{recommendationId:N}");

    public static RecordTenantActivityRequest ForCourseRecommendationsGenerated(
        string tenantId, Guid employeeId, string employeeName, int count, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.CourseRecommendationsGenerated,
            $"{count} course recommendation{(count == 1 ? "" : "s")} generated for {employeeName}",
            "employee", employeeId, employeeName, currentUser,
            correlationSuffix: $"course-recommendations-generated:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Learning: Skills ──────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForEmployeeSkillAdded(
        string tenantId, Guid employeeSkillId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.EmployeeSkillAdded,
            summary, "employee-skill", employeeSkillId, summary, currentUser,
            correlationSuffix: $"employee-skill-added:{employeeSkillId:N}");

    public static RecordTenantActivityRequest ForSkillCreated(
        string tenantId, Guid skillId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.SkillCreated,
            $"Skill \"{name}\" was created",
            "skill", skillId, name, currentUser,
            correlationSuffix: $"skill-created:{skillId:N}");

    public static RecordTenantActivityRequest ForSkillDeleted(
        string tenantId, Guid skillId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.SkillDeleted,
            $"Skill \"{name}\" was deleted",
            "skill", skillId, name, currentUser,
            correlationSuffix: $"skill-deleted:{skillId:N}");

    public static RecordTenantActivityRequest ForEmployeeSkillUpdated(
        string tenantId, Guid employeeSkillId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.EmployeeSkillUpdated,
            summary, "employee-skill", employeeSkillId, summary, currentUser,
            correlationSuffix: $"employee-skill-updated:{employeeSkillId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSkillUpdated(
        string tenantId, Guid skillId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrLearningActivityTypes.SkillUpdated,
            $"Skill \"{name}\" was updated",
            "skill", skillId, name, currentUser,
            correlationSuffix: $"skill-updated:{skillId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── News ───────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForNewsCreated(
        string tenantId, Guid newsId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrNewsActivityTypes.NewsCreated,
            $"News \"{title}\" was created",
            "news", newsId, title, currentUser,
            correlationSuffix: $"news-created:{newsId:N}");

    public static RecordTenantActivityRequest ForNewsPublished(
        string tenantId, Guid newsId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrNewsActivityTypes.NewsPublished,
            $"News \"{title}\" was published",
            "news", newsId, title, currentUser,
            correlationSuffix: $"news-published:{newsId:N}");

    public static RecordTenantActivityRequest ForNewsAttachmentAdded(
        string tenantId, Guid newsId, string title, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrNewsActivityTypes.NewsAttachmentAdded,
            $"An attachment was added to news \"{title}\"",
            "news", newsId, title, currentUser,
            correlationSuffix: $"news-attachment-added:{newsId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForNewsContributionCreated(
        string tenantId, Guid contributionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrNewsActivityTypes.NewsContributionCreated,
            summary, "news-contribution", contributionId, summary, currentUser,
            correlationSuffix: $"news-contribution-created:{contributionId:N}");

    public static RecordTenantActivityRequest ForNewsContributionUpdated(
        string tenantId, Guid contributionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrNewsActivityTypes.NewsContributionUpdated,
            summary, "news-contribution", contributionId, summary, currentUser,
            correlationSuffix: $"news-contribution-updated:{contributionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForNewsContributionDeleted(
        string tenantId, Guid contributionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Hr, HrNewsActivityTypes.NewsContributionDeleted,
            summary, "news-contribution", contributionId, summary, currentUser,
            correlationSuffix: $"news-contribution-deleted:{contributionId:N}");

    // ── Payroll ────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForPayRunCreated(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunCreated,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-created:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayRunApproved(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunApproved,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-approved:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayRunProcessed(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunProcessed,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-processed:{payRunId:N}");

    public static RecordTenantActivityRequest ForPaymentCreated(
        string tenantId, Guid paymentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PaymentCreated,
            summary, "payment", paymentId, summary, currentUser,
            correlationSuffix: $"payment-created:{paymentId:N}");

    public static RecordTenantActivityRequest ForPaymentProcessed(
        string tenantId, Guid paymentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PaymentProcessed,
            summary, "payment", paymentId, summary, currentUser,
            correlationSuffix: $"payment-processed:{paymentId:N}");

    public static RecordTenantActivityRequest ForPayrollRunPaymentsProcessed(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollRunPaymentsProcessed,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"payroll-run-payments-processed:{payRunId:N}");

    public static RecordTenantActivityRequest ForClaimApproved(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimApproved,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-approved:{claimId:N}");

    public static RecordTenantActivityRequest ForClaimRejected(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimRejected,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-rejected:{claimId:N}");

    public static RecordTenantActivityRequest ForClaimReadyForPayment(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimReadyForPayment,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-ready-for-payment:{claimId:N}");

    public static RecordTenantActivityRequest ForClaimPaymentProcessed(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimPaymentProcessed,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-payment-processed:{claimId:N}");

    public static RecordTenantActivityRequest ForClaimProcessed(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimProcessed,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-processed:{claimId:N}");

    public static RecordTenantActivityRequest ForAdvanceApproved(
        string tenantId, Guid advanceId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AdvanceApproved,
            summary, "advance", advanceId, summary, currentUser,
            correlationSuffix: $"advance-approved:{advanceId:N}");

    public static RecordTenantActivityRequest ForAdvanceRejected(
        string tenantId, Guid advanceId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AdvanceRejected,
            summary, "advance", advanceId, summary, currentUser,
            correlationSuffix: $"advance-rejected:{advanceId:N}");

    public static RecordTenantActivityRequest ForLoanApproved(
        string tenantId, Guid loanId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.LoanApproved,
            summary, "loan", loanId, summary, currentUser,
            correlationSuffix: $"loan-approved:{loanId:N}");

    public static RecordTenantActivityRequest ForAllowanceAssigned(
        string tenantId, Guid employeeId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AllowanceAssigned,
            summary, "employee", employeeId, summary, currentUser,
            correlationSuffix: $"allowance-assigned:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDeductionAssigned(
        string tenantId, Guid employeeId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionAssigned,
            summary, "employee", employeeId, summary, currentUser,
            correlationSuffix: $"deduction-assigned:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPayrollAdjustmentApproved(
        string tenantId, Guid adjustmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAdjustmentApproved,
            summary, "payroll-adjustment", adjustmentId, summary, currentUser,
            correlationSuffix: $"payroll-adjustment-approved:{adjustmentId:N}");

    public static RecordTenantActivityRequest ForPayrollAdjustmentRejected(
        string tenantId, Guid adjustmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAdjustmentRejected,
            summary, "payroll-adjustment", adjustmentId, summary, currentUser,
            correlationSuffix: $"payroll-adjustment-rejected:{adjustmentId:N}");

    public static RecordTenantActivityRequest ForPayrollItemApproved(
        string tenantId, Guid itemId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollItemApproved,
            summary, "payroll-item", itemId, summary, currentUser,
            correlationSuffix: $"payroll-item-approved:{itemId:N}");

    public static RecordTenantActivityRequest ForOvertimeApproved(
        string tenantId, Guid overtimeId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.OvertimeApproved,
            summary, "overtime", overtimeId, summary, currentUser,
            correlationSuffix: $"overtime-approved:{overtimeId:N}");

    public static RecordTenantActivityRequest ForOvertimeRejected(
        string tenantId, Guid overtimeId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.OvertimeRejected,
            summary, "overtime", overtimeId, summary, currentUser,
            correlationSuffix: $"overtime-rejected:{overtimeId:N}");

    public static RecordTenantActivityRequest ForProvidentFundWithdrawalApproved(
        string tenantId, Guid withdrawalId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ProvidentFundWithdrawalApproved,
            summary, "provident-fund-withdrawal", withdrawalId, summary, currentUser,
            correlationSuffix: $"provident-fund-withdrawal-approved:{withdrawalId:N}");

    public static RecordTenantActivityRequest ForProvidentFundWithdrawalRejected(
        string tenantId, Guid withdrawalId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ProvidentFundWithdrawalRejected,
            summary, "provident-fund-withdrawal", withdrawalId, summary, currentUser,
            correlationSuffix: $"provident-fund-withdrawal-rejected:{withdrawalId:N}");

    public static RecordTenantActivityRequest ForDirectDepositAccountVerified(
        string tenantId, Guid accountId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DirectDepositAccountVerified,
            summary, "direct-deposit-account", accountId, summary, currentUser,
            correlationSuffix: $"direct-deposit-account-verified:{accountId:N}");

    public static RecordTenantActivityRequest ForGradeSalaryStepsUpserted(
        string tenantId, string gradeCode, int stepCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.GradeSalaryStepsUpserted,
            $"Grade {gradeCode} salary steps upserted ({stepCount} step{(stepCount == 1 ? "" : "s")})",
            "grade", Guid.Empty, gradeCode, currentUser,
            correlationSuffix: $"grade-salary-steps-upserted:{gradeCode}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Payroll: explicit-actor (Guid) overloads for Temporal activities ────
    // These mirror existing ICurrentUserService overloads above but accept a raw actor id,
    // for call sites inside Temporal activities that have no HTTP/ICurrentUserService context.
    public static RecordTenantActivityRequest ForPayRunProcessed(
        string tenantId, Guid payRunId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunProcessed,
            summary, "pay-run", payRunId, summary, actorUserId,
            correlationSuffix: $"pay-run-processed:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayRunApproved(
        string tenantId, Guid payRunId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunApproved,
            summary, "pay-run", payRunId, summary, actorUserId,
            correlationSuffix: $"pay-run-approved:{payRunId:N}");

    public static RecordTenantActivityRequest ForClaimApproved(
        string tenantId, Guid claimId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimApproved,
            summary, "claim", claimId, summary, actorUserId,
            correlationSuffix: $"claim-approved:{claimId:N}");

    public static RecordTenantActivityRequest ForClaimRejected(
        string tenantId, Guid claimId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimRejected,
            summary, "claim", claimId, summary, actorUserId,
            correlationSuffix: $"claim-rejected:{claimId:N}");

    public static RecordTenantActivityRequest ForAdvanceApproved(
        string tenantId, Guid advanceId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AdvanceApproved,
            summary, "advance", advanceId, summary, actorUserId,
            correlationSuffix: $"advance-approved:{advanceId:N}");

    public static RecordTenantActivityRequest ForAdvanceRejected(
        string tenantId, Guid advanceId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AdvanceRejected,
            summary, "advance", advanceId, summary, actorUserId,
            correlationSuffix: $"advance-rejected:{advanceId:N}");

    public static RecordTenantActivityRequest ForLoanApproved(
        string tenantId, Guid loanId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.LoanApproved,
            summary, "loan", loanId, summary, actorUserId,
            correlationSuffix: $"loan-approved:{loanId:N}");

    public static RecordTenantActivityRequest ForOvertimeApproved(
        string tenantId, Guid overtimeId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.OvertimeApproved,
            summary, "overtime", overtimeId, summary, actorUserId,
            correlationSuffix: $"overtime-approved:{overtimeId:N}");

    public static RecordTenantActivityRequest ForProvidentFundWithdrawalApproved(
        string tenantId, Guid withdrawalId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ProvidentFundWithdrawalApproved,
            summary, "provident-fund-withdrawal", withdrawalId, summary, actorUserId,
            correlationSuffix: $"provident-fund-withdrawal-approved:{withdrawalId:N}");

    public static RecordTenantActivityRequest ForProvidentFundWithdrawalRejected(
        string tenantId, Guid withdrawalId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ProvidentFundWithdrawalRejected,
            summary, "provident-fund-withdrawal", withdrawalId, summary, actorUserId,
            correlationSuffix: $"provident-fund-withdrawal-rejected:{withdrawalId:N}");

    public static RecordTenantActivityRequest ForPayrollAdjustmentApproved(
        string tenantId, Guid adjustmentId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAdjustmentApproved,
            summary, "payroll-adjustment", adjustmentId, summary, actorUserId,
            correlationSuffix: $"payroll-adjustment-approved:{adjustmentId:N}");

    public static RecordTenantActivityRequest ForPayrollAdjustmentRejected(
        string tenantId, Guid adjustmentId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAdjustmentRejected,
            summary, "payroll-adjustment", adjustmentId, summary, actorUserId,
            correlationSuffix: $"payroll-adjustment-rejected:{adjustmentId:N}");

    public static RecordTenantActivityRequest ForAllowanceAssigned(
        string tenantId, Guid employeeId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AllowanceAssigned,
            summary, "employee", employeeId, summary, actorUserId,
            correlationSuffix: $"allowance-assigned:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDeductionAssigned(
        string tenantId, Guid employeeId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionAssigned,
            summary, "employee", employeeId, summary, actorUserId,
            correlationSuffix: $"deduction-assigned:{employeeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Payroll: Pay runs / payslips / payroll items ────────────────────────
    public static RecordTenantActivityRequest ForPayRunUpdated(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunUpdated,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-updated:{payRunId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPayRunCancelled(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunCancelled,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-cancelled:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayRunCancelled(
        string tenantId, Guid payRunId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunCancelled,
            summary, "pay-run", payRunId, summary, actorUserId,
            correlationSuffix: $"pay-run-cancelled:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayRunDeleted(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunDeleted,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-deleted:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayRunFinalized(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunFinalized,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-finalized:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayRunReversed(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunReversed,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-reversed:{payRunId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPayRunSubmittedForApproval(
        string tenantId, Guid payRunId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunSubmittedForApproval,
            summary, "pay-run", payRunId, summary, currentUser,
            correlationSuffix: $"pay-run-submitted-for-approval:{payRunId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPayRunCompleted(
        string tenantId, Guid payRunId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayRunCompleted,
            summary, "pay-run", payRunId, summary, actorUserId,
            correlationSuffix: $"pay-run-completed:{payRunId:N}");

    public static RecordTenantActivityRequest ForPayslipGenerated(
        string tenantId, Guid payslipId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayslipGenerated,
            summary, "payslip", payslipId, summary, currentUser,
            correlationSuffix: $"payslip-generated:{payslipId:N}");

    public static RecordTenantActivityRequest ForPayslipsGeneratedBulk(
        string tenantId, Guid payRunId, int count, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayslipsGeneratedBulk,
            $"Generated {count} payslip{(count == 1 ? "" : "s")} for payroll run",
            "pay-run", payRunId, payRunId.ToString(), currentUser,
            correlationSuffix: $"payslips-generated-bulk:{payRunId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPayrollRunStaleDraftsCleanedUp(
        string tenantId, int deletedCount, int olderThanDays, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollRunStaleDraftsCleanedUp,
            $"Cleaned up {deletedCount} stale draft payroll run{(deletedCount == 1 ? "" : "s")} older than {olderThanDays} days",
            "payroll-run", Guid.Empty, "stale-drafts-cleanup", currentUser,
            correlationSuffix: $"payroll-run-stale-drafts-cleaned-up:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadataJson: $$"""{"deletedCount":{{deletedCount}},"olderThanDays":{{olderThanDays}}}""");

    public static RecordTenantActivityRequest ForPayrollItemMarkedAsPaid(
        string tenantId, Guid itemId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollItemMarkedAsPaid,
            summary, "payroll-item", itemId, summary, currentUser,
            correlationSuffix: $"payroll-item-marked-as-paid:{itemId:N}");

    // ── Payroll: Accrual runs / statutory submissions ───────────────────────
    public static RecordTenantActivityRequest ForPayrollAccrualRunCreated(
        string tenantId, Guid runId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAccrualRunCreated,
            summary, "payroll-accrual-run", runId, summary, currentUser,
            correlationSuffix: $"payroll-accrual-run-created:{runId:N}");

    public static RecordTenantActivityRequest ForPayrollAccrualRunPosted(
        string tenantId, Guid runId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAccrualRunPosted,
            summary, "payroll-accrual-run", runId, summary, currentUser,
            correlationSuffix: $"payroll-accrual-run-posted:{runId:N}");

    public static RecordTenantActivityRequest ForPayrollAccrualRunSubmittedForApproval(
        string tenantId, Guid runId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAccrualRunSubmittedForApproval,
            summary, "payroll-accrual-run", runId, summary, currentUser,
            correlationSuffix: $"payroll-accrual-run-submitted-for-approval:{runId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForStatutoryFilingSubmittedForApproval(
        string tenantId, Guid submissionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.StatutoryFilingSubmittedForApproval,
            summary, "statutory-submission", submissionId, summary, currentUser,
            correlationSuffix: $"statutory-filing-submitted-for-approval:{submissionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Payroll: Claims / advances / loans ──────────────────────────────────
    public static RecordTenantActivityRequest ForClaimDocumentCreated(
        string tenantId, Guid documentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimDocumentCreated,
            summary, "claim-document", documentId, summary, currentUser,
            correlationSuffix: $"claim-document-created:{documentId:N}");

    public static RecordTenantActivityRequest ForClaimCreated(
        string tenantId, Guid claimId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ClaimCreated,
            summary, "claim", claimId, summary, currentUser,
            correlationSuffix: $"claim-created:{claimId:N}");

    public static RecordTenantActivityRequest ForAdvanceCreated(
        string tenantId, Guid advanceId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AdvanceCreated,
            summary, "advance", advanceId, summary, currentUser,
            correlationSuffix: $"advance-created:{advanceId:N}");

    public static RecordTenantActivityRequest ForAdvanceDeleted(
        string tenantId, Guid advanceId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AdvanceDeleted,
            summary, "advance", advanceId, summary, currentUser,
            correlationSuffix: $"advance-deleted:{advanceId:N}");

    public static RecordTenantActivityRequest ForAdvanceUpdated(
        string tenantId, Guid advanceId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AdvanceUpdated,
            summary, "advance", advanceId, summary, currentUser,
            correlationSuffix: $"advance-updated:{advanceId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForLoanCreated(
        string tenantId, Guid loanId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.LoanCreated,
            summary, "loan", loanId, summary, currentUser,
            correlationSuffix: $"loan-created:{loanId:N}");

    public static RecordTenantActivityRequest ForLoanUpdated(
        string tenantId, Guid loanId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.LoanUpdated,
            summary, "loan", loanId, summary, currentUser,
            correlationSuffix: $"loan-updated:{loanId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForLoanDeleted(
        string tenantId, Guid loanId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.LoanDeleted,
            summary, "loan", loanId, summary, currentUser,
            correlationSuffix: $"loan-deleted:{loanId:N}");

    public static RecordTenantActivityRequest ForLoanCancelled(
        string tenantId, Guid loanId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.LoanCancelled,
            summary, "loan", loanId, summary, actorUserId,
            correlationSuffix: $"loan-cancelled:{loanId:N}");

    public static RecordTenantActivityRequest ForLoanRepaymentCreated(
        string tenantId, Guid repaymentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.LoanRepaymentCreated,
            summary, "loan-repayment", repaymentId, summary, currentUser,
            correlationSuffix: $"loan-repayment-created:{repaymentId:N}");

    public static RecordTenantActivityRequest ForLoanRepaymentPaid(
        string tenantId, Guid repaymentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.LoanRepaymentPaid,
            summary, "loan-repayment", repaymentId, summary, currentUser,
            correlationSuffix: $"loan-repayment-paid:{repaymentId:N}");

    // ── Payroll: Overtime / adjustments ──────────────────────────────────────
    public static RecordTenantActivityRequest ForOvertimeCreated(
        string tenantId, Guid overtimeId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.OvertimeCreated,
            summary, "overtime", overtimeId, summary, currentUser,
            correlationSuffix: $"overtime-created:{overtimeId:N}");

    public static RecordTenantActivityRequest ForOvertimeRateConfigsUpserted(
        string tenantId, int count, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.OvertimeRateConfigsUpserted,
            $"Upserted {count} overtime rate config{(count == 1 ? "" : "s")}",
            "overtime-rate-config", Guid.Empty, "overtime-rate-configs", currentUser,
            correlationSuffix: $"overtime-rate-configs-upserted:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPayrollAdjustmentCreated(
        string tenantId, Guid adjustmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAdjustmentCreated,
            summary, "payroll-adjustment", adjustmentId, summary, currentUser,
            correlationSuffix: $"payroll-adjustment-created:{adjustmentId:N}");

    public static RecordTenantActivityRequest ForPayrollAdjustmentCreated(
        string tenantId, Guid adjustmentId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollAdjustmentCreated,
            summary, "payroll-adjustment", adjustmentId, summary, actorUserId,
            correlationSuffix: $"payroll-adjustment-created:{adjustmentId:N}");

    // ── Payroll: Allowances / deductions ──────────────────────────────────────
    public static RecordTenantActivityRequest ForAllowanceActivated(
        string tenantId, Guid allowanceId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AllowanceActivated,
            $"Allowance \"{name}\" was activated",
            "allowance", allowanceId, name, currentUser,
            correlationSuffix: $"allowance-activated:{allowanceId:N}");

    public static RecordTenantActivityRequest ForAllowanceCreated(
        string tenantId, Guid allowanceId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AllowanceCreated,
            $"Allowance \"{name}\" was created",
            "allowance", allowanceId, name, currentUser,
            correlationSuffix: $"allowance-created:{allowanceId:N}");

    public static RecordTenantActivityRequest ForAllowanceDeleted(
        string tenantId, Guid allowanceId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AllowanceDeleted,
            $"Allowance \"{name}\" was deleted",
            "allowance", allowanceId, name, currentUser,
            correlationSuffix: $"allowance-deleted:{allowanceId:N}");

    public static RecordTenantActivityRequest ForAllowanceUpdated(
        string tenantId, Guid allowanceId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AllowanceUpdated,
            $"Allowance \"{name}\" was updated",
            "allowance", allowanceId, name, currentUser,
            correlationSuffix: $"allowance-updated:{allowanceId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForAllowanceAssignmentUpdated(
        string tenantId, Guid assignmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AllowanceAssignmentUpdated,
            summary, "allowance-assignment", assignmentId, summary, currentUser,
            correlationSuffix: $"allowance-assignment-updated:{assignmentId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForAllowanceAssignmentDeleted(
        string tenantId, Guid assignmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.AllowanceAssignmentDeleted,
            summary, "allowance-assignment", assignmentId, summary, currentUser,
            correlationSuffix: $"allowance-assignment-deleted:{assignmentId:N}");

    public static RecordTenantActivityRequest ForDeductionActivated(
        string tenantId, Guid deductionId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionActivated,
            $"Deduction \"{name}\" was activated",
            "deduction", deductionId, name, currentUser,
            correlationSuffix: $"deduction-activated:{deductionId:N}");

    public static RecordTenantActivityRequest ForDeductionCreated(
        string tenantId, Guid deductionId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionCreated,
            $"Deduction \"{name}\" was created",
            "deduction", deductionId, name, currentUser,
            correlationSuffix: $"deduction-created:{deductionId:N}");

    public static RecordTenantActivityRequest ForDeductionDeleted(
        string tenantId, Guid deductionId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionDeleted,
            $"Deduction \"{name}\" was deleted",
            "deduction", deductionId, name, currentUser,
            correlationSuffix: $"deduction-deleted:{deductionId:N}");

    public static RecordTenantActivityRequest ForDeductionUpdated(
        string tenantId, Guid deductionId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionUpdated,
            $"Deduction \"{name}\" was updated",
            "deduction", deductionId, name, currentUser,
            correlationSuffix: $"deduction-updated:{deductionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDeductionAssignmentUpdated(
        string tenantId, Guid assignmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionAssignmentUpdated,
            summary, "deduction-assignment", assignmentId, summary, currentUser,
            correlationSuffix: $"deduction-assignment-updated:{assignmentId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDeductionAssignmentDeleted(
        string tenantId, Guid assignmentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionAssignmentDeleted,
            summary, "deduction-assignment", assignmentId, summary, currentUser,
            correlationSuffix: $"deduction-assignment-deleted:{assignmentId:N}");

    public static RecordTenantActivityRequest ForDeductionAssignmentEnded(
        string tenantId, Guid assignmentId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DeductionAssignmentEnded,
            summary, "deduction-assignment", assignmentId, summary, actorUserId,
            correlationSuffix: $"deduction-assignment-ended:{assignmentId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Payroll: Salary structures / components / grades ────────────────────
    public static RecordTenantActivityRequest ForSalaryComponentCreated(
        string tenantId, Guid componentId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SalaryComponentCreated,
            $"Salary component \"{name}\" was created",
            "salary-component", componentId, name, currentUser,
            correlationSuffix: $"salary-component-created:{componentId:N}");

    public static RecordTenantActivityRequest ForSalaryComponentUpdated(
        string tenantId, Guid componentId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SalaryComponentUpdated,
            $"Salary component \"{name}\" was updated",
            "salary-component", componentId, name, currentUser,
            correlationSuffix: $"salary-component-updated:{componentId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSalaryComponentDeleted(
        string tenantId, Guid componentId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SalaryComponentDeleted,
            $"Salary component \"{name}\" was deleted",
            "salary-component", componentId, name, currentUser,
            correlationSuffix: $"salary-component-deleted:{componentId:N}");

    public static RecordTenantActivityRequest ForSalaryStructureCreated(
        string tenantId, Guid structureId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SalaryStructureCreated,
            summary, "salary-structure", structureId, summary, currentUser,
            correlationSuffix: $"salary-structure-created:{structureId:N}");

    public static RecordTenantActivityRequest ForSalaryStructureUpdated(
        string tenantId, Guid structureId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SalaryStructureUpdated,
            summary, "salary-structure", structureId, summary, currentUser,
            correlationSuffix: $"salary-structure-updated:{structureId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSalaryStructureActivated(
        string tenantId, Guid structureId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SalaryStructureActivated,
            summary, "salary-structure", structureId, summary, actorUserId,
            correlationSuffix: $"salary-structure-activated:{structureId:N}");

    public static RecordTenantActivityRequest ForSalaryStructureDeactivated(
        string tenantId, Guid structureId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SalaryStructureDeactivated,
            summary, "salary-structure", structureId, summary, currentUser,
            correlationSuffix: $"salary-structure-deactivated:{structureId:N}");

    public static RecordTenantActivityRequest ForSalaryStructureDeactivated(
        string tenantId, Guid structureId, string summary, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SalaryStructureDeactivated,
            summary, "salary-structure", structureId, summary, actorUserId,
            correlationSuffix: $"salary-structure-deactivated:{structureId:N}");

    public static RecordTenantActivityRequest ForGradeCreated(
        string tenantId, Guid gradeId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.GradeCreated,
            $"Grade \"{name}\" was created",
            "grade", gradeId, name, currentUser,
            correlationSuffix: $"grade-created:{gradeId:N}");

    public static RecordTenantActivityRequest ForGradeUpdated(
        string tenantId, Guid gradeId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.GradeUpdated,
            $"Grade \"{name}\" was updated",
            "grade", gradeId, name, currentUser,
            correlationSuffix: $"grade-updated:{gradeId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForGradeDeleted(
        string tenantId, Guid gradeId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.GradeDeleted,
            $"Grade \"{name}\" was deleted",
            "grade", gradeId, name, currentUser,
            correlationSuffix: $"grade-deleted:{gradeId:N}");

    // ── Payroll: Bulk assignments ─────────────────────────────────────────────
    public static RecordTenantActivityRequest ForSalaryComponentsBulkAssigned(
        string tenantId, int mergedCount, int createdCount, int skippedCount, int failedCount,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SalaryComponentsBulkAssigned,
            $"Bulk-assigned salary components: {mergedCount} merged, {createdCount} created, {skippedCount} skipped, {failedCount} failed",
            "bulk-operation", Guid.Empty, "salary-components-bulk-assigned", currentUser,
            correlationSuffix: $"salary-components-bulk-assigned:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadataJson: $$"""{"merged":{{mergedCount}},"created":{{createdCount}},"skipped":{{skippedCount}},"failed":{{failedCount}}}""");

    public static RecordTenantActivityRequest ForGradeNotchBulkAssigned(
        string tenantId, string gradeCode, string notchLabel, int createdCount, int skippedCount, int failedCount,
        ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.GradeNotchBulkAssigned,
            $"Grade {gradeCode} notch {notchLabel} assigned — {createdCount} created, {skippedCount} skipped, {failedCount} failed",
            "grade", Guid.Empty, gradeCode, currentUser,
            correlationSuffix: $"grade-notch-bulk-assigned:{gradeCode}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadataJson: $$"""{"created":{{createdCount}},"skipped":{{skippedCount}},"failed":{{failedCount}}}""");

    // ── Payroll: Payroll groups ───────────────────────────────────────────────
    public static RecordTenantActivityRequest ForPayrollGroupCreated(
        string tenantId, Guid groupId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollGroupCreated,
            $"Payroll group \"{name}\" was created",
            "payroll-group", groupId, name, currentUser,
            correlationSuffix: $"payroll-group-created:{groupId:N}");

    public static RecordTenantActivityRequest ForPayrollGroupUpdated(
        string tenantId, Guid groupId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollGroupUpdated,
            $"Payroll group \"{name}\" was updated",
            "payroll-group", groupId, name, currentUser,
            correlationSuffix: $"payroll-group-updated:{groupId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPayrollGroupActivated(
        string tenantId, Guid groupId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollGroupActivated,
            $"Payroll group \"{name}\" was activated",
            "payroll-group", groupId, name, currentUser,
            correlationSuffix: $"payroll-group-activated:{groupId:N}");

    public static RecordTenantActivityRequest ForPayrollGroupDeactivated(
        string tenantId, Guid groupId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollGroupDeactivated,
            $"Payroll group \"{name}\" was deactivated",
            "payroll-group", groupId, name, currentUser,
            correlationSuffix: $"payroll-group-deactivated:{groupId:N}");

    public static RecordTenantActivityRequest ForPayrollGroupDeleted(
        string tenantId, Guid groupId, string name, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollGroupDeleted,
            $"Payroll group \"{name}\" was deleted",
            "payroll-group", groupId, name, currentUser,
            correlationSuffix: $"payroll-group-deleted:{groupId:N}");

    public static RecordTenantActivityRequest ForPayrollGroupMemberAdded(
        string tenantId, Guid groupId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollGroupMemberAdded,
            summary, "payroll-group", groupId, summary, currentUser,
            correlationSuffix: $"payroll-group-member-added:{groupId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPayrollGroupMemberRemoved(
        string tenantId, Guid groupId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollGroupMemberRemoved,
            summary, "payroll-group", groupId, summary, currentUser,
            correlationSuffix: $"payroll-group-member-removed:{groupId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Payroll: Tax documents / configuration ──────────────────────────────
    public static RecordTenantActivityRequest ForTaxDocumentCreated(
        string tenantId, Guid documentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxDocumentCreated,
            summary, "tax-document", documentId, summary, currentUser,
            correlationSuffix: $"tax-document-created:{documentId:N}");

    public static RecordTenantActivityRequest ForTaxDocumentGenerated(
        string tenantId, Guid documentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxDocumentGenerated,
            summary, "tax-document", documentId, summary, currentUser,
            correlationSuffix: $"tax-document-generated:{documentId:N}");

    public static RecordTenantActivityRequest ForTaxDocumentSent(
        string tenantId, Guid documentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxDocumentSent,
            summary, "tax-document", documentId, summary, currentUser,
            correlationSuffix: $"tax-document-sent:{documentId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTaxDocumentFiled(
        string tenantId, Guid documentId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxDocumentFiled,
            summary, "tax-document", documentId, summary, currentUser,
            correlationSuffix: $"tax-document-filed:{documentId:N}");

    public static RecordTenantActivityRequest ForTaxDocumentsBulkGenerated(
        string tenantId, int count, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxDocumentsBulkGenerated,
            $"Generated {count} pending tax document{(count == 1 ? "" : "s")}",
            "tax-document", Guid.Empty, "tax-documents-bulk-generated", currentUser,
            correlationSuffix: $"tax-documents-bulk-generated:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeTaxProfileCreated(
        string tenantId, Guid profileId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.EmployeeTaxProfileCreated,
            summary, "employee-tax-profile", profileId, summary, currentUser,
            correlationSuffix: $"employee-tax-profile-created:{profileId:N}");

    public static RecordTenantActivityRequest ForEmployeeTaxProfileUpdated(
        string tenantId, Guid profileId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.EmployeeTaxProfileUpdated,
            summary, "employee-tax-profile", profileId, summary, currentUser,
            correlationSuffix: $"employee-tax-profile-updated:{profileId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTaxBracketCreated(
        string tenantId, Guid bracketId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxBracketCreated,
            summary, "tax-bracket", bracketId, summary, currentUser,
            correlationSuffix: $"tax-bracket-created:{bracketId:N}");

    public static RecordTenantActivityRequest ForTaxBracketUpdated(
        string tenantId, Guid bracketId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxBracketUpdated,
            summary, "tax-bracket", bracketId, summary, currentUser,
            correlationSuffix: $"tax-bracket-updated:{bracketId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTaxBracketDeleted(
        string tenantId, Guid bracketId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxBracketDeleted,
            summary, "tax-bracket", bracketId, summary, currentUser,
            correlationSuffix: $"tax-bracket-deleted:{bracketId:N}");

    public static RecordTenantActivityRequest ForTaxConfigurationCreated(
        string tenantId, Guid configId, string code, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxConfigurationCreated,
            $"Tax configuration \"{code}\" was created",
            "tax-configuration", configId, code, currentUser,
            correlationSuffix: $"tax-configuration-created:{configId:N}");

    public static RecordTenantActivityRequest ForTaxConfigurationUpdated(
        string tenantId, Guid configId, string code, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxConfigurationUpdated,
            $"Tax configuration \"{code}\" was updated",
            "tax-configuration", configId, code, currentUser,
            correlationSuffix: $"tax-configuration-updated:{configId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTaxConfigurationDeleted(
        string tenantId, Guid configId, string code, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.TaxConfigurationDeleted,
            $"Tax configuration \"{code}\" was deleted",
            "tax-configuration", configId, code, currentUser,
            correlationSuffix: $"tax-configuration-deleted:{configId:N}");

    // ── Payroll: Direct deposit / mobile money ───────────────────────────────
    public static RecordTenantActivityRequest ForDirectDepositAccountCreated(
        string tenantId, Guid accountId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DirectDepositAccountCreated,
            summary, "direct-deposit-account", accountId, summary, currentUser,
            correlationSuffix: $"direct-deposit-account-created:{accountId:N}");

    public static RecordTenantActivityRequest ForDirectDepositAccountUpdated(
        string tenantId, Guid accountId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DirectDepositAccountUpdated,
            summary, "direct-deposit-account", accountId, summary, currentUser,
            correlationSuffix: $"direct-deposit-account-updated:{accountId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForDirectDepositAccountDeleted(
        string tenantId, Guid accountId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.DirectDepositAccountDeleted,
            summary, "direct-deposit-account", accountId, summary, currentUser,
            correlationSuffix: $"direct-deposit-account-deleted:{accountId:N}");

    public static RecordTenantActivityRequest ForMobileMoneyAccountCreated(
        string tenantId, Guid accountId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.MobileMoneyAccountCreated,
            summary, "mobile-money-account", accountId, summary, currentUser,
            correlationSuffix: $"mobile-money-account-created:{accountId:N}");

    public static RecordTenantActivityRequest ForMobileMoneyAccountUpdated(
        string tenantId, Guid accountId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.MobileMoneyAccountUpdated,
            summary, "mobile-money-account", accountId, summary, currentUser,
            correlationSuffix: $"mobile-money-account-updated:{accountId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForMobileMoneyAccountDeleted(
        string tenantId, Guid accountId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.MobileMoneyAccountDeleted,
            summary, "mobile-money-account", accountId, summary, currentUser,
            correlationSuffix: $"mobile-money-account-deleted:{accountId:N}");

    // ── Payroll: SSNIT / statutory submissions ───────────────────────────────
    public static RecordTenantActivityRequest ForSsnitConfigurationCreated(
        string tenantId, Guid configId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SsnitConfigurationCreated,
            summary, "ssnit-configuration", configId, summary, currentUser,
            correlationSuffix: $"ssnit-configuration-created:{configId:N}");

    public static RecordTenantActivityRequest ForSsnitConfigurationUpdated(
        string tenantId, Guid configId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SsnitConfigurationUpdated,
            summary, "ssnit-configuration", configId, summary, currentUser,
            correlationSuffix: $"ssnit-configuration-updated:{configId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSsnitConfigurationDeleted(
        string tenantId, Guid configId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.SsnitConfigurationDeleted,
            summary, "ssnit-configuration", configId, summary, currentUser,
            correlationSuffix: $"ssnit-configuration-deleted:{configId:N}");

    public static RecordTenantActivityRequest ForStatutorySubmissionFiled(
        string tenantId, Guid submissionId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.StatutorySubmissionFiled,
            summary, "statutory-submission", submissionId, summary, currentUser,
            correlationSuffix: $"statutory-submission-filed:{submissionId:N}");

    public static RecordTenantActivityRequest ForStatutorySubmissionsBulkFiled(
        string tenantId, int filedCount, int alreadyFiledCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.StatutorySubmissionsBulkFiled,
            $"Bulk-filed {filedCount} statutory submission{(filedCount == 1 ? "" : "s")} ({alreadyFiledCount} already filed)",
            "statutory-submission", Guid.Empty, "statutory-submissions-bulk-filed", currentUser,
            correlationSuffix: $"statutory-submissions-bulk-filed:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadataJson: $$"""{"filedCount":{{filedCount}},"alreadyFiledCount":{{alreadyFiledCount}}}""");

    // ── Payroll: Payroll schedules ────────────────────────────────────────────
    public static RecordTenantActivityRequest ForPayrollScheduleCreated(
        string tenantId, Guid scheduleId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollScheduleCreated,
            summary, "payroll-schedule", scheduleId, summary, currentUser,
            correlationSuffix: $"payroll-schedule-created:{scheduleId:N}");

    public static RecordTenantActivityRequest ForPayrollScheduleActivated(
        string tenantId, Guid scheduleId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.PayrollScheduleActivated,
            summary, "payroll-schedule", scheduleId, summary, currentUser,
            correlationSuffix: $"payroll-schedule-activated:{scheduleId:N}");

    // ── Payroll: Provident fund (system/bulk) ────────────────────────────────
    public static RecordTenantActivityRequest ForProvidentFundContributionsBulkProcessed(
        string tenantId, int count, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ProvidentFundContributionsBulkProcessed,
            $"Processed {count} provident fund contribution{(count == 1 ? "" : "s")} in bulk",
            "provident-fund", Guid.Empty, "provident-fund-contributions-bulk-processed", actorUserId,
            correlationSuffix: $"provident-fund-contributions-bulk-processed:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForProvidentFundInterestBulkCalculated(
        string tenantId, int count, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ProvidentFundInterestBulkCalculated,
            $"Calculated interest for {count} provident fund account{(count == 1 ? "" : "s")} in bulk",
            "provident-fund", Guid.Empty, "provident-fund-interest-bulk-calculated", actorUserId,
            correlationSuffix: $"provident-fund-interest-bulk-calculated:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForProvidentFundArrearsBulkApplied(
        string tenantId, int count, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Payroll, PayrollActivityTypes.ProvidentFundArrearsBulkApplied,
            $"Applied arrears for {count} provident fund account{(count == 1 ? "" : "s")} in bulk",
            "provident-fund", Guid.Empty, "provident-fund-arrears-bulk-applied", actorUserId,
            correlationSuffix: $"provident-fund-arrears-bulk-applied:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Platform Workflow ──────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForWorkflowConfigCreated(
        string tenantId, Guid configId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowConfigCreated,
            summary, "workflow-config", configId, summary, currentUser,
            correlationSuffix: $"workflow-config-created:{configId:N}");

    public static RecordTenantActivityRequest ForWorkflowConfigUpdated(
        string tenantId, Guid configId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowConfigUpdated,
            summary, "workflow-config", configId, summary, currentUser,
            correlationSuffix: $"workflow-config-updated:{configId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForWorkflowConfigDeleted(
        string tenantId, Guid configId, string summary, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowConfigDeleted,
            summary, "workflow-config", configId, summary, currentUser,
            correlationSuffix: $"workflow-config-deleted:{configId:N}");

    public static RecordTenantActivityRequest ForWorkflowTemplateCreated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateCreated,
            $"Workflow template \"{templateName}\" was created",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-created:{templateId:N}");

    public static RecordTenantActivityRequest ForWorkflowTemplateUpdated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateUpdated,
            $"Workflow template \"{templateName}\" was updated",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-updated:{templateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForWorkflowTemplateDeleted(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateDeleted,
            $"Workflow template \"{templateName}\" was deleted",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-deleted:{templateId:N}");

    public static RecordTenantActivityRequest ForWorkflowTemplateDuplicated(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateDuplicated,
            $"Workflow template \"{templateName}\" was duplicated",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-duplicated:{templateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForWorkflowTemplateImported(
        string tenantId, Guid templateId, string templateName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.WorkflowTemplateImported,
            $"Workflow template \"{templateName}\" was imported",
            "workflow-template", templateId, templateName, currentUser,
            correlationSuffix: $"workflow-template-imported:{templateId:N}");

    public static RecordTenantActivityRequest ForEntityWorkflowDefinitionPublished(
        string tenantId, string entityType, string workflowCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.EntityWorkflowDefinitionPublished,
            $"Workflow \"{workflowCode}\" was published for entity type \"{entityType}\"",
            "entity-workflow-definition", Guid.Empty, $"{entityType}:{workflowCode}", currentUser,
            correlationSuffix: $"entity-workflow-definition-published:{entityType}:{workflowCode}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForEmployeeCreateWorkflowSynced(
        string tenantId, string workflowCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformWorkflowActivityTypes.EmployeeCreateWorkflowSynced,
            $"Employee-create approval workflow \"{workflowCode}\" was synced to the canonical definition",
            "workflow-template", Guid.Empty, workflowCode, currentUser,
            correlationSuffix: $"employee-create-workflow-synced:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Platform Notifications (plugin settings) ───────────────────────────
    public static RecordTenantActivityRequest ForChatNotifySettingsUpdated(
        string tenantId, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformNotificationActivityTypes.ChatNotifySettingsUpdated,
            "Chat notification settings were updated",
            "notification-settings", Guid.Empty, "chat-notify", currentUser,
            correlationSuffix: $"chat-notify-settings-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForConferenceNotifySettingsUpdated(
        string tenantId, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformNotificationActivityTypes.ConferenceNotifySettingsUpdated,
            "Conference notification settings were updated",
            "notification-settings", Guid.Empty, "conference-notify", currentUser,
            correlationSuffix: $"conference-notify-settings-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSmsNotifySettingsUpdated(
        string tenantId, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformNotificationActivityTypes.SmsNotifySettingsUpdated,
            "SMS notification settings were updated",
            "notification-settings", Guid.Empty, "sms-notify", currentUser,
            correlationSuffix: $"sms-notify-settings-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForWebhookNotifySettingsUpdated(
        string tenantId, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Platform, PlatformNotificationActivityTypes.WebhookNotifySettingsUpdated,
            "Webhook notification settings were updated",
            "notification-settings", Guid.Empty, "webhook-notify", currentUser,
            correlationSuffix: $"webhook-notify-settings-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── IAM ────────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForUserCreated(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserCreated,
            $"User {userName} was created",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-created:{userId:N}");

    /// <summary>
    /// Explicit-actor overload for admin-invite flows that resolve the inviting admin
    /// (e.g. <c>tenantAdmin.Id</c>) directly and don't have <see cref="ICurrentUserService"/>
    /// in scope.
    /// </summary>
    public static RecordTenantActivityRequest ForUserCreated(
        string tenantId, Guid userId, string userName, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserCreated,
            $"User {userName} was created",
            "user", userId, userName, actorUserId,
            correlationSuffix: $"user-created:{userId:N}");

    public static RecordTenantActivityRequest ForUserActivated(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserActivated,
            $"User {userName} was activated",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-activated:{userId:N}");

    public static RecordTenantActivityRequest ForUserDeactivated(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserDeactivated,
            $"User {userName} was deactivated",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-deactivated:{userId:N}");

    public static RecordTenantActivityRequest ForUserPasswordReset(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserPasswordReset,
            $"Password reset for user {userName}",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-password-reset:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    /// <summary>
    /// Anonymous/token-based reset (e.g. "forgot password" email flow) — no
    /// <see cref="ICurrentUserService"/> context exists since no one is authenticated yet.
    /// </summary>
    public static RecordTenantActivityRequest ForUserPasswordReset(
        string tenantId, Guid userId, string userName) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserPasswordReset,
            $"Password reset for user {userName}",
            "user", userId, userName, userId,
            correlationSuffix: $"user-password-reset:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForUserProfileUpdated(
        string tenantId, Guid userId, string userName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserProfileUpdated,
            $"Profile updated for user {userName}",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-profile-updated:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForUserRoleAssigned(
        string tenantId, Guid userId, string userName, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserRoleAssigned,
            $"Role \"{roleName}\" assigned to user {userName}",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-role-assigned:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForUserRoleRemoved(
        string tenantId, Guid userId, string userName, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserRoleRemoved,
            $"Role \"{roleName}\" removed from user {userName}",
            "user", userId, userName, currentUser,
            correlationSuffix: $"user-role-removed:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForRoleCreated(
        string tenantId, Guid roleId, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.RoleCreated,
            $"Role \"{roleName}\" was created",
            "role", roleId, roleName, currentUser,
            correlationSuffix: $"role-created:{roleId:N}");

    public static RecordTenantActivityRequest ForRoleDeleted(
        string tenantId, Guid roleId, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.RoleDeleted,
            $"Role \"{roleName}\" was deleted",
            "role", roleId, roleName, currentUser,
            correlationSuffix: $"role-deleted:{roleId:N}");

    public static RecordTenantActivityRequest ForRolePermissionsAssigned(
        string tenantId, Guid roleId, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.RolePermissionsAssigned,
            $"Permissions assigned to role \"{roleName}\"",
            "role", roleId, roleName, currentUser,
            correlationSuffix: $"role-permissions-assigned:{roleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForRolePermissionRemoved(
        string tenantId, Guid roleId, string roleName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.RolePermissionRemoved,
            $"Permission removed from role \"{roleName}\"",
            "role", roleId, roleName, currentUser,
            correlationSuffix: $"role-permission-removed:{roleId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenantRegistered(
        string tenantId, string tenantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantRegistered,
            $"Tenant \"{tenantName}\" was registered",
            "tenant", Guid.Empty, tenantName, currentUser,
            correlationSuffix: $"tenant-registered:{tenantId}");

    public static RecordTenantActivityRequest ForTenantLogoUpdated(
        string tenantId, string tenantName, ICurrentUserService currentUser, string? logoUrl = null) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantLogoUpdated,
            $"Logo updated for tenant \"{tenantName}\"",
            "tenant", Guid.Empty, tenantName, currentUser,
            correlationSuffix: $"tenant-logo-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            pictureUrl: logoUrl);

    public static RecordTenantActivityRequest ForTenantThemeUpdated(
        string tenantId, string tenantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantThemeUpdated,
            $"Theme updated for tenant \"{tenantName}\"",
            "tenant", Guid.Empty, tenantName, currentUser,
            correlationSuffix: $"tenant-theme-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenantSubscriptionUpdated(
        string tenantId, string tenantName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantSubscriptionUpdated,
            $"Subscription updated for tenant \"{tenantName}\"",
            "tenant", Guid.Empty, tenantName, currentUser,
            correlationSuffix: $"tenant-subscription-updated:{tenantId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── IAM: Lookups (database-types, billing-cycles, company-types, etc.) ────
    public static RecordTenantActivityRequest ForLookupCreated(
        string tenantId, Guid lookupId, string lookupName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.LookupCreated,
            $"Lookup \"{lookupName}\" was created",
            "lookup", lookupId, lookupName, currentUser,
            correlationSuffix: $"lookup-created:{lookupId:N}");

    public static RecordTenantActivityRequest ForLookupUpdated(
        string tenantId, Guid lookupId, string lookupName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.LookupUpdated,
            $"Lookup \"{lookupName}\" was updated",
            "lookup", lookupId, lookupName, currentUser,
            correlationSuffix: $"lookup-updated:{lookupId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForLookupDeleted(
        string tenantId, Guid lookupId, string lookupName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.LookupDeleted,
            $"Lookup \"{lookupName}\" was deleted",
            "lookup", lookupId, lookupName, currentUser,
            correlationSuffix: $"lookup-deleted:{lookupId:N}");

    // ── IAM: App Store ──────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForAppStoreItemInstalled(
        string tenantId, string itemKey, string itemName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.AppStoreItemInstalled,
            $"App Store item \"{itemName}\" was installed",
            "app-store-item", Guid.Empty, itemKey, currentUser,
            correlationSuffix: $"app-store-item-installed:{itemKey}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForAppStoreItemUninstalled(
        string tenantId, string itemKey, string itemName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.AppStoreItemUninstalled,
            $"App Store item \"{itemName}\" was uninstalled",
            "app-store-item", Guid.Empty, itemKey, currentUser,
            correlationSuffix: $"app-store-item-uninstalled:{itemKey}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── IAM: Tenant reference — currencies ──────────────────────────────────
    public static RecordTenantActivityRequest ForTenantCurrencyCreated(
        string tenantId, Guid currencyId, string currencyCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantCurrencyCreated,
            $"Currency \"{currencyCode}\" was added",
            "tenant-currency", currencyId, currencyCode, currentUser,
            correlationSuffix: $"tenant-currency-created:{currencyId:N}");

    public static RecordTenantActivityRequest ForTenantCurrencyUpdated(
        string tenantId, Guid currencyId, string currencyCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantCurrencyUpdated,
            $"Currency \"{currencyCode}\" was updated",
            "tenant-currency", currencyId, currencyCode, currentUser,
            correlationSuffix: $"tenant-currency-updated:{currencyId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenantCurrencyDeleted(
        string tenantId, Guid currencyId, string currencyCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantCurrencyDeleted,
            $"Currency \"{currencyCode}\" was removed",
            "tenant-currency", currencyId, currencyCode, currentUser,
            correlationSuffix: $"tenant-currency-deleted:{currencyId:N}");

    // ── IAM: Tenant reference — exchange rates ──────────────────────────────
    public static RecordTenantActivityRequest ForTenantExchangeRateCreated(
        string tenantId, Guid rateId, string label, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantExchangeRateCreated,
            $"Exchange rate \"{label}\" was added",
            "tenant-exchange-rate", rateId, label, currentUser,
            correlationSuffix: $"tenant-exchange-rate-created:{rateId:N}");

    public static RecordTenantActivityRequest ForTenantExchangeRateUpdated(
        string tenantId, Guid rateId, string label, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantExchangeRateUpdated,
            $"Exchange rate \"{label}\" was updated",
            "tenant-exchange-rate", rateId, label, currentUser,
            correlationSuffix: $"tenant-exchange-rate-updated:{rateId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenantExchangeRateDeleted(
        string tenantId, Guid rateId, string label, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantExchangeRateDeleted,
            $"Exchange rate \"{label}\" was removed",
            "tenant-exchange-rate", rateId, label, currentUser,
            correlationSuffix: $"tenant-exchange-rate-deleted:{rateId:N}");

    // ── IAM: Tenant reference — holidays ─────────────────────────────────────
    public static RecordTenantActivityRequest ForTenantHolidayCreated(
        string tenantId, Guid holidayId, string holidayName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantHolidayCreated,
            $"Holiday \"{holidayName}\" was added",
            "tenant-holiday", holidayId, holidayName, currentUser,
            correlationSuffix: $"tenant-holiday-created:{holidayId:N}");

    public static RecordTenantActivityRequest ForTenantHolidayUpdated(
        string tenantId, Guid holidayId, string holidayName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantHolidayUpdated,
            $"Holiday \"{holidayName}\" was updated",
            "tenant-holiday", holidayId, holidayName, currentUser,
            correlationSuffix: $"tenant-holiday-updated:{holidayId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenantHolidayDeleted(
        string tenantId, Guid holidayId, string holidayName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantHolidayDeleted,
            $"Holiday \"{holidayName}\" was removed",
            "tenant-holiday", holidayId, holidayName, currentUser,
            correlationSuffix: $"tenant-holiday-deleted:{holidayId:N}");

    // ── IAM: Tenant reference — banks ────────────────────────────────────────
    public static RecordTenantActivityRequest ForTenantBankCreated(
        string tenantId, Guid bankId, string bankName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantBankCreated,
            $"Bank \"{bankName}\" was added",
            "tenant-bank", bankId, bankName, currentUser,
            correlationSuffix: $"tenant-bank-created:{bankId:N}");

    public static RecordTenantActivityRequest ForTenantBankUpdated(
        string tenantId, Guid bankId, string bankName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantBankUpdated,
            $"Bank \"{bankName}\" was updated",
            "tenant-bank", bankId, bankName, currentUser,
            correlationSuffix: $"tenant-bank-updated:{bankId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTenantBankDeleted(
        string tenantId, Guid bankId, string bankName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.TenantBankDeleted,
            $"Bank \"{bankName}\" was removed",
            "tenant-bank", bankId, bankName, currentUser,
            correlationSuffix: $"tenant-bank-deleted:{bankId:N}");

    // ── IAM: RBAC permissions catalog ────────────────────────────────────────
    public static RecordTenantActivityRequest ForPermissionCreated(
        string tenantId, Guid permissionId, string permissionCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.PermissionCreated,
            $"Permission \"{permissionCode}\" was created",
            "permission", permissionId, permissionCode, currentUser,
            correlationSuffix: $"permission-created:{permissionId:N}");

    public static RecordTenantActivityRequest ForPermissionUpdated(
        string tenantId, Guid permissionId, string permissionCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.PermissionUpdated,
            $"Permission \"{permissionCode}\" was updated",
            "permission", permissionId, permissionCode, currentUser,
            correlationSuffix: $"permission-updated:{permissionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForPermissionDeleted(
        string tenantId, Guid permissionId, string permissionCode, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.PermissionDeleted,
            $"Permission \"{permissionCode}\" was deleted",
            "permission", permissionId, permissionCode, currentUser,
            correlationSuffix: $"permission-deleted:{permissionId:N}");

    // ── IAM: Account activation (first-time, self) ───────────────────────────
    public static RecordTenantActivityRequest ForAccountActivated(
        string tenantId, Guid userId, string userLabel, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.AccountActivated,
            $"{userLabel} activated their account",
            "user", userId, userLabel, actorUserId,
            correlationSuffix: $"account-activated:{userId:N}");

    // ── IAM: Tenant subscriptions ─────────────────────────────────────────────
    /// <summary>
    /// Explicit-actor overload — <c>CreateSubscriptionRequest</c> already threads
    /// <c>UserId</c>/<c>UserEmail</c>/<c>UserName</c> through from the HTTP layer, so the
    /// service records with those rather than resolving <see cref="ICurrentUserService"/> again.
    /// </summary>
    public static RecordTenantActivityRequest ForSubscriptionCreated(
        string tenantId, Guid subscriptionId, string planName, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.SubscriptionCreated,
            $"Subscription \"{planName}\" was created",
            "subscription", subscriptionId, planName, actorUserId,
            correlationSuffix: $"subscription-created:{subscriptionId:N}");

    public static RecordTenantActivityRequest ForSubscriptionUpdated(
        string tenantId, Guid subscriptionId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.SubscriptionUpdated,
            $"Subscription \"{planName}\" was updated",
            "subscription", subscriptionId, planName, currentUser,
            correlationSuffix: $"subscription-updated:{subscriptionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSubscriptionCancelled(
        string tenantId, Guid subscriptionId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.SubscriptionCancelled,
            $"Subscription \"{planName}\" was cancelled",
            "subscription", subscriptionId, planName, currentUser,
            correlationSuffix: $"subscription-cancelled:{subscriptionId:N}");

    public static RecordTenantActivityRequest ForSubscriptionSuspended(
        string tenantId, Guid subscriptionId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.SubscriptionSuspended,
            $"Subscription \"{planName}\" was suspended",
            "subscription", subscriptionId, planName, currentUser,
            correlationSuffix: $"subscription-suspended:{subscriptionId:N}");

    public static RecordTenantActivityRequest ForSubscriptionResumed(
        string tenantId, Guid subscriptionId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.SubscriptionResumed,
            $"Subscription \"{planName}\" was resumed",
            "subscription", subscriptionId, planName, currentUser,
            correlationSuffix: $"subscription-resumed:{subscriptionId:N}");

    public static RecordTenantActivityRequest ForSubscriptionDeleted(
        string tenantId, Guid subscriptionId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.SubscriptionDeleted,
            $"Subscription \"{planName}\" was deleted",
            "subscription", subscriptionId, planName, currentUser,
            correlationSuffix: $"subscription-deleted:{subscriptionId:N}");

    public static RecordTenantActivityRequest ForSubscriptionPaymentVerified(
        string tenantId, Guid subscriptionId, string planName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.SubscriptionPaymentVerified,
            $"Payment verified and subscription \"{planName}\" activated",
            "subscription", subscriptionId, planName, currentUser,
            correlationSuffix: $"subscription-payment-verified:{subscriptionId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Auth ───────────────────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForAuthLoginSuccess(
        string tenantId, Guid userId, string userLabel) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.LoginSuccess,
            $"User {userLabel} signed in successfully",
            "user", userId, userLabel, userId,
            correlationSuffix: $"login-success:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForAuthLoginFailed(
        string tenantId, Guid? userId, string identifier, string? failureReason = null) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.LoginFailed,
            string.IsNullOrWhiteSpace(failureReason)
                ? $"Failed sign-in attempt for {identifier}"
                : $"Failed sign-in attempt for {identifier}: {failureReason}",
            "user", userId ?? Guid.Empty, identifier, userId ?? Guid.Empty,
            correlationSuffix: $"login-failed:{(userId ?? Guid.Empty):N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadataJson: string.IsNullOrWhiteSpace(failureReason)
                ? null
                : $"{{\"failureReason\":\"{EscapeJson(failureReason)}\"}}");

    public static RecordTenantActivityRequest ForAuthLockout(
        string tenantId, Guid userId, string userLabel, string reason) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.AccountLockout,
            $"Account locked for user {userLabel}: {reason}",
            "user", userId, userLabel, Guid.Empty,
            correlationSuffix: $"account-lockout:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadataJson: $"{{\"reason\":\"{EscapeJson(reason)}\"}}");

    public static RecordTenantActivityRequest ForAccountSelfLocked(
        string tenantId, Guid userId, string userLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.AccountLockout,
            $"{userLabel} locked their own account",
            "user", userId, userLabel, currentUser,
            correlationSuffix: $"self-lock:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForLoginBlockedByLockout(
        string tenantId, Guid userId, string userLabel) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.LoginBlockedByLockout,
            $"Sign-in blocked for {userLabel}: account is locked",
            "user", userId, userLabel, userId,
            correlationSuffix: $"login-blocked-by-lockout:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForLoginTwoFactorRequired(
        string tenantId, Guid userId, string userLabel) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.LoginTwoFactorRequired,
            $"{userLabel} passed password check — two-factor verification required",
            "user", userId, userLabel, userId,
            correlationSuffix: $"login-two-factor-required:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForAccountUnlocked(
        string tenantId, Guid userId, string userLabel, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.AccountUnlocked,
            $"Account unlocked for {userLabel}",
            "user", userId, userLabel, actorUserId,
            correlationSuffix: $"account-unlocked:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTwoFactorEnabled(
        string tenantId, Guid userId, string userLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.TwoFactorEnabled,
            $"Two-factor authentication setup started for {userLabel}",
            "user", userId, userLabel, currentUser,
            correlationSuffix: $"two-factor-enabled:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTwoFactorVerified(
        string tenantId, Guid userId, string userLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.TwoFactorVerified,
            $"Two-factor authentication enabled for {userLabel}",
            "user", userId, userLabel, currentUser,
            correlationSuffix: $"two-factor-verified:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTwoFactorDisabled(
        string tenantId, Guid userId, string userLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.TwoFactorDisabled,
            $"Two-factor authentication disabled for {userLabel}",
            "user", userId, userLabel, currentUser,
            correlationSuffix: $"two-factor-disabled:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForTwoFactorChallengeFailed(
        string tenantId, Guid userId, string userLabel, string? failureReason = null) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.TwoFactorChallengeFailed,
            string.IsNullOrWhiteSpace(failureReason)
                ? $"Failed two-factor challenge for {userLabel}"
                : $"Failed two-factor challenge for {userLabel}: {failureReason}",
            "user", userId, userLabel, userId,
            correlationSuffix: $"two-factor-challenge-failed:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadataJson: string.IsNullOrWhiteSpace(failureReason)
                ? null
                : $"{{\"failureReason\":\"{EscapeJson(failureReason)}\"}}");

    public static RecordTenantActivityRequest ForBackupCodesRegenerated(
        string tenantId, Guid userId, string userLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.BackupCodesRegenerated,
            $"Backup codes regenerated for {userLabel}",
            "user", userId, userLabel, currentUser,
            correlationSuffix: $"backup-codes-regenerated:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSessionRevoked(
        string tenantId, Guid userId, string userLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.SessionRevoked,
            $"A session was revoked for {userLabel}",
            "user", userId, userLabel, currentUser,
            correlationSuffix: $"session-revoked:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForSessionsRevokedAll(
        string tenantId, Guid userId, string userLabel, int revokedCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.SessionsRevokedAll,
            $"All sessions ({revokedCount}) revoked for {userLabel}",
            "user", userId, userLabel, currentUser,
            correlationSuffix: $"sessions-revoked-all:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    /// <summary>
    /// Explicit-actor overload for the forced-logout token-revocation path (no
    /// <see cref="ICurrentUserService"/> in scope — the user is resolved by username).
    /// </summary>
    public static RecordTenantActivityRequest ForSessionsRevokedAll(
        string tenantId, Guid userId, string userLabel, int revokedCount, Guid actorUserId) =>
        Build(tenantId, TenantActivityModules.Auth, AuthActivityTypes.SessionsRevokedAll,
            $"All sessions ({revokedCount}) revoked for {userLabel}",
            "user", userId, userLabel, actorUserId,
            correlationSuffix: $"sessions-revoked-all:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForUserPasswordChanged(
        string tenantId, Guid userId, string userLabel, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Iam, IamActivityTypes.UserPasswordChanged,
            $"{userLabel} changed their password",
            "user", userId, userLabel, currentUser,
            correlationSuffix: $"user-password-changed:{userId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    // ── Operations · Inventory ──────────────────────────────────────────────
    public static RecordTenantActivityRequest ForProductCreated(
        string tenantId, Guid productId, string productName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.ProductCreated,
            $"Product \"{productName}\" was created",
            "product", productId, productName, currentUser,
            correlationSuffix: $"product-created:{productId:N}");

    public static RecordTenantActivityRequest ForProductUpdated(
        string tenantId, Guid productId, string productName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.ProductUpdated,
            $"Product \"{productName}\" was updated",
            "product", productId, productName, currentUser,
            correlationSuffix: $"product-updated:{productId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForProductDeleted(
        string tenantId, Guid productId, string productName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.ProductDeleted,
            $"Product \"{productName}\" was deleted",
            "product", productId, productName, currentUser,
            correlationSuffix: $"product-deleted:{productId:N}");

    // Warehouse.Id is a long (not a Guid), so the subject id is left empty and the
    // warehouse id travels in metadataJson; the warehouse name is the subject label.
    public static RecordTenantActivityRequest ForWarehouseCreated(
        string tenantId, long warehouseId, string warehouseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.WarehouseCreated,
            $"Warehouse \"{warehouseName}\" was created",
            "warehouse", Guid.Empty, warehouseName, currentUser,
            correlationSuffix: $"warehouse-created:{warehouseId}",
            metadataJson: $$"""{"warehouseId":{{warehouseId}}}""");

    public static RecordTenantActivityRequest ForWarehouseUpdated(
        string tenantId, long warehouseId, string warehouseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.WarehouseUpdated,
            $"Warehouse \"{warehouseName}\" was updated",
            "warehouse", Guid.Empty, warehouseName, currentUser,
            correlationSuffix: $"warehouse-updated:{warehouseId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadataJson: $$"""{"warehouseId":{{warehouseId}}}""");

    public static RecordTenantActivityRequest ForWarehouseDeleted(
        string tenantId, long warehouseId, string warehouseName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.WarehouseDeleted,
            $"Warehouse \"{warehouseName}\" was deleted",
            "warehouse", Guid.Empty, warehouseName, currentUser,
            correlationSuffix: $"warehouse-deleted:{warehouseId}",
            metadataJson: $$"""{"warehouseId":{{warehouseId}}}""");

    public static RecordTenantActivityRequest ForPriceAdjustmentCreated(
        string tenantId, Guid priceAdjustmentId, string reference, int lineCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.PriceAdjustmentCreated,
            $"Price adjustment \"{reference}\" was created — {lineCount} price adjustment lines were created",
            "price-adjustment", priceAdjustmentId, reference, currentUser,
            correlationSuffix: $"price-adjustment-created:{priceAdjustmentId:N}",
            metadataJson: $$"""{"lineCount":{{lineCount}}}""");

    public static RecordTenantActivityRequest ForQuantityAdjustmentCreated(
        string tenantId, Guid quantityAdjustmentId, string reference, int lineCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.QuantityAdjustmentCreated,
            $"Quantity adjustment \"{reference}\" was created — {lineCount} quantity adjustment lines were created",
            "quantity-adjustment", quantityAdjustmentId, reference, currentUser,
            correlationSuffix: $"quantity-adjustment-created:{quantityAdjustmentId:N}",
            metadataJson: $$"""{"lineCount":{{lineCount}}}""");

    public static RecordTenantActivityRequest ForStockMovementCreated(
        string tenantId, Guid stockMovementId, string reference, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.StockMovementCreated,
            $"Stock movement \"{reference}\" was created",
            "stock-movement", stockMovementId, reference, currentUser,
            correlationSuffix: $"stock-movement-created:{stockMovementId:N}");

    public static RecordTenantActivityRequest ForStockTransferCreated(
        string tenantId, Guid stockTransferId, string reference, int lineCount, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsInventoryActivityTypes.StockTransferCreated,
            $"Stock transfer \"{reference}\" was created — {lineCount} stock transfer lines were created",
            "stock-transfer", stockTransferId, reference, currentUser,
            correlationSuffix: $"stock-transfer-created:{stockTransferId:N}",
            metadataJson: $$"""{"lineCount":{{lineCount}}}""");

    // ── Operations · Project ─────────────────────────────────────────────────
    public static RecordTenantActivityRequest ForProjectCreated(
        string tenantId, Guid projectId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectCreated,
            $"Project \"{projectName}\" was created",
            "project", projectId, projectName, currentUser,
            correlationSuffix: $"project-created:{projectId:N}");

    public static RecordTenantActivityRequest ForProjectUpdated(
        string tenantId, Guid projectId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectUpdated,
            $"Project \"{projectName}\" was updated",
            "project", projectId, projectName, currentUser,
            correlationSuffix: $"project-updated:{projectId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForProjectDeleted(
        string tenantId, Guid projectId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectDeleted,
            $"Project \"{projectName}\" was deleted",
            "project", projectId, projectName, currentUser,
            correlationSuffix: $"project-deleted:{projectId:N}");

    public static RecordTenantActivityRequest ForProjectStatusChanged(
        string tenantId, Guid projectId, string projectName, string newStatus, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectStatusChanged,
            $"Project \"{projectName}\" status was changed to \"{newStatus}\"",
            "project", projectId, projectName, currentUser,
            correlationSuffix: $"project-status-changed:{projectId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForProjectMilestoneCreated(
        string tenantId, Guid milestoneId, string milestoneName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectMilestoneCreated,
            $"Project milestone \"{milestoneName}\" was created",
            "project-milestone", milestoneId, milestoneName, currentUser,
            correlationSuffix: $"project-milestone-created:{milestoneId:N}");

    public static RecordTenantActivityRequest ForProjectTaskCreated(
        string tenantId, Guid taskId, string taskName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectTaskCreated,
            $"Project task \"{taskName}\" was created",
            "project-task", taskId, taskName, currentUser,
            correlationSuffix: $"project-task-created:{taskId:N}");

    public static RecordTenantActivityRequest ForProjectTaskUpdated(
        string tenantId, Guid taskId, string taskName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectTaskUpdated,
            $"Project task \"{taskName}\" was updated",
            "project-task", taskId, taskName, currentUser,
            correlationSuffix: $"project-task-updated:{taskId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForProjectTaskDeleted(
        string tenantId, Guid taskId, string taskName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectTaskDeleted,
            $"Project task \"{taskName}\" was deleted",
            "project-task", taskId, taskName, currentUser,
            correlationSuffix: $"project-task-deleted:{taskId:N}");

    public static RecordTenantActivityRequest ForProjectTimeEntryCreated(
        string tenantId, Guid timeEntryId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectTimeEntryCreated,
            $"Time entry was created for {employeeName}",
            "project-time-entry", timeEntryId, employeeName, currentUser,
            correlationSuffix: $"project-time-entry-created:{timeEntryId:N}");

    public static RecordTenantActivityRequest ForProjectTimeEntryUpdated(
        string tenantId, Guid timeEntryId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectTimeEntryUpdated,
            $"Time entry was updated for {employeeName}",
            "project-time-entry", timeEntryId, employeeName, currentUser,
            correlationSuffix: $"project-time-entry-updated:{timeEntryId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForProjectTimeEntrySubmitted(
        string tenantId, Guid timeEntryId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectTimeEntrySubmitted,
            $"Time entry was submitted for {employeeName}",
            "project-time-entry", timeEntryId, employeeName, currentUser,
            correlationSuffix: $"project-time-entry-submitted:{timeEntryId:N}");

    public static RecordTenantActivityRequest ForProjectTimeEntryApproved(
        string tenantId, Guid timeEntryId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectTimeEntryApproved,
            $"Time entry was approved for {employeeName}",
            "project-time-entry", timeEntryId, employeeName, currentUser,
            correlationSuffix: $"project-time-entry-approved:{timeEntryId:N}");

    public static RecordTenantActivityRequest ForProjectTimeEntryRejected(
        string tenantId, Guid timeEntryId, string employeeName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectTimeEntryRejected,
            $"Time entry was rejected for {employeeName}",
            "project-time-entry", timeEntryId, employeeName, currentUser,
            correlationSuffix: $"project-time-entry-rejected:{timeEntryId:N}");

    public static RecordTenantActivityRequest ForProjectBudgetCreated(
        string tenantId, Guid budgetId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectBudgetCreated,
            $"Project budget was created for \"{projectName}\"",
            "project-budget", budgetId, projectName, currentUser,
            correlationSuffix: $"project-budget-created:{budgetId:N}");

    public static RecordTenantActivityRequest ForProjectBudgetBaselineMarked(
        string tenantId, Guid budgetId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectBudgetBaselineMarked,
            $"Project budget for \"{projectName}\" was marked as baseline",
            "project-budget", budgetId, projectName, currentUser,
            correlationSuffix: $"project-budget-baseline-marked:{budgetId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForProjectContractCreated(
        string tenantId, Guid contractId, string contractName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectContractCreated,
            $"Project contract \"{contractName}\" was created",
            "project-contract", contractId, contractName, currentUser,
            correlationSuffix: $"project-contract-created:{contractId:N}");

    public static RecordTenantActivityRequest ForProjectExpenditureCreated(
        string tenantId, Guid expenditureId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectExpenditureCreated,
            $"Project expenditure was created for \"{projectName}\"",
            "project-expenditure", expenditureId, projectName, currentUser,
            correlationSuffix: $"project-expenditure-created:{expenditureId:N}");

    public static RecordTenantActivityRequest ForProjectExpenditureUpdated(
        string tenantId, Guid expenditureId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectExpenditureUpdated,
            $"Project expenditure was updated for \"{projectName}\"",
            "project-expenditure", expenditureId, projectName, currentUser,
            correlationSuffix: $"project-expenditure-updated:{expenditureId:N}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

    public static RecordTenantActivityRequest ForProjectExpenditureDeleted(
        string tenantId, Guid expenditureId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectExpenditureDeleted,
            $"Project expenditure was deleted for \"{projectName}\"",
            "project-expenditure", expenditureId, projectName, currentUser,
            correlationSuffix: $"project-expenditure-deleted:{expenditureId:N}");

    public static RecordTenantActivityRequest ForProjectBillingRuleCreated(
        string tenantId, Guid billingRuleId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectBillingRuleCreated,
            $"Project billing rule was created for \"{projectName}\"",
            "project-billing-rule", billingRuleId, projectName, currentUser,
            correlationSuffix: $"project-billing-rule-created:{billingRuleId:N}");

    public static RecordTenantActivityRequest ForProjectBillGenerated(
        string tenantId, Guid billId, string projectName, ICurrentUserService currentUser) =>
        Build(tenantId, TenantActivityModules.Operations, OperationsProjectActivityTypes.ProjectBillGenerated,
            $"Project bill was generated for \"{projectName}\"",
            "project-bill", billId, projectName, currentUser,
            correlationSuffix: $"project-bill-generated:{billId:N}");

    /// <summary>
    /// Convenience overload for the common HTTP-request case — extracts the actor id from
    /// the current claims principal. Actor name/picture are no longer resolved here: IAM
    /// looks them up (cache-first, keyed by actor id) right before persisting.
    /// </summary>
    private static RecordTenantActivityRequest Build(
        string tenantId,
        string module,
        string activityType,
        string summary,
        string subjectType,
        Guid subjectId,
        string subjectLabel,
        ICurrentUserService currentUser,
        string correlationSuffix,
        string? metadataJson = null,
        string? pictureUrl = null)
    {
        var actorUserId = Guid.TryParse(currentUser.GetUserId(), out var parsed)
            ? parsed
            : Guid.Empty;

        return Build(tenantId, module, activityType, summary, subjectType, subjectId, subjectLabel,
            actorUserId, correlationSuffix, metadataJson, pictureUrl);
    }

    /// <summary>
    /// Base builder for call sites that only have a raw actor id in scope (no
    /// <see cref="ICurrentUserService"/> available, e.g. an actor acting on behalf of
    /// someone else). <paramref name="pictureUrl"/> is an explicit override reserved for
    /// events about a picture/logo itself changing — everyday events leave it null and get
    /// the actor's own picture filled in by IAM at persist time.
    /// </summary>
    private static RecordTenantActivityRequest Build(
        string tenantId,
        string module,
        string activityType,
        string summary,
        string subjectType,
        Guid subjectId,
        string subjectLabel,
        Guid actorUserId,
        string correlationSuffix,
        string? metadataJson = null,
        string? pictureUrl = null) =>
        new()
        {
            TenantId = tenantId,
            Module = module,
            ActivityType = activityType,
            Summary = summary,
            ActorUserId = actorUserId,
            SubjectType = subjectType,
            SubjectId = subjectId,
            SubjectLabel = subjectLabel,
            OccurredAt = DateTime.UtcNow,
            CorrelationId = $"{module}:{correlationSuffix}",
            MetadataJson = metadataJson,
            PictureUrl = pictureUrl
        };

    private static string BuildEmployeeJoinedSummary(string employeeName, string? organizationalUnitName)
    {
        var destination = string.IsNullOrWhiteSpace(organizationalUnitName)
            ? "the organization"
            : organizationalUnitName.Trim();
        return $"New employee {employeeName} joined {destination}";
    }

    private static string EscapeJson(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
}
