namespace QimErp.Shared.Common.Activities.TenantActivity;

public static class TenantActivityModules
{
    public const string Hr = "hr";
    public const string Accounting = "accounting";
    public const string Iam = "iam";
    public const string Payroll = "payroll";
    public const string Platform = "platform";
}

public static class HrActivityTypes
{
    public const string EmployeeCreated = "employee-created";
    public const string EmployeeUpdated = "employee-updated";
    public const string EmployeeDeactivated = "employee-deactivated";
    public const string EmployeeActivated = "employee-activated";
    public const string DepartmentCreated = "department-created";
    public const string DepartmentUpdated = "department-updated";
    public const string DepartmentDeleted = "department-deleted";
    public const string JobTitleCreated = "job-title-created";
    public const string JobTitleUpdated = "job-title-updated";
    public const string JobTitleDeleted = "job-title-deleted";
    public const string LeaveRequestSubmitted = "leave-request-submitted";
    public const string LeaveRequestApproved = "leave-request-approved";
    public const string LeaveRequestRejected = "leave-request-rejected";
    public const string LeaveRequestCancelled = "leave-request-cancelled";
    public const string OffboardingInitiated = "offboarding-initiated";
    public const string OffboardingApproved = "offboarding-approved";
    public const string OffboardingCancelled = "offboarding-cancelled";
    public const string OffboardingCompleted = "offboarding-completed";
    public const string EmployeeJobTitleChanged = "employee-job-title-changed";
    public const string EmployeeOrgUnitChanged = "employee-org-unit-changed";
    public const string BulkOperation = "bulk-operation";
    public const string RankCreated = "rank-created";
    public const string RankUpdated = "rank-updated";
    public const string RankDeleted = "rank-deleted";
    public const string StationCreated = "station-created";
    public const string StationUpdated = "station-updated";
    public const string StationDeleted = "station-deleted";
    public const string JobStatusCreated = "job-status-created";
    public const string JobStatusUpdated = "job-status-updated";
    public const string JobStatusDeleted = "job-status-deleted";
}

public static class HrOnboardingActivityTypes
{
    public const string OnboardingStarted = "onboarding-started";
    public const string OnboardingCancelled = "onboarding-cancelled";
    public const string OnboardingMetaUpdated = "onboarding-meta-updated";
    public const string OnboardingTaskCompleted = "onboarding-task-completed";
    public const string OnboardingTaskReopened = "onboarding-task-reopened";
    public const string OnboardingTaskSkipped = "onboarding-task-skipped";
    public const string OnboardingTaskReassigned = "onboarding-task-reassigned";
    public const string OnboardingTaskEscalated = "onboarding-task-escalated";
    public const string OnboardingAdhocTaskAdded = "onboarding-adhoc-task-added";
    public const string OnboardingTemplateCreated = "onboarding-template-created";
    public const string OnboardingTemplateUpdated = "onboarding-template-updated";
    public const string OnboardingTemplateDeleted = "onboarding-template-deleted";
    public const string OnboardingTemplateDefaultSet = "onboarding-template-default-set";
}

public static class HrPerformanceActivityTypes
{
    public const string ReviewCreated = "review-created";
    public const string SelfReviewSubmitted = "self-review-submitted";
    public const string ManagerReviewSubmitted = "manager-review-submitted";
    public const string HRReviewSubmitted = "hr-review-submitted";
    public const string ReviewFinalized = "review-finalized";
    public const string ReviewAcknowledged = "review-acknowledged";
    public const string GoalCreated = "goal-created";
    public const string GoalUpdated = "goal-updated";
    public const string GoalProgressUpdated = "goal-progress-updated";
    public const string GoalApproved = "goal-approved";
    public const string GoalCompleted = "goal-completed";
    public const string GoalAligned = "goal-aligned";
    public const string GoalEvidenceUploaded = "goal-evidence-uploaded";
    public const string CheckInCreated = "check-in-created";
    public const string CheckInUpdated = "check-in-updated";
    public const string CheckInManagerFeedbackAdded = "check-in-manager-feedback-added";
    public const string CalibrationCreated = "calibration-created";
    public const string CalibrationRatingAdjusted = "calibration-rating-adjusted";
    public const string CalibrationCompleted = "calibration-completed";
    public const string Feedback360Created = "feedback360-created";
    public const string Feedback360ProviderAdded = "feedback360-provider-added";
    public const string Feedback360Completed = "feedback360-completed";
    public const string DevelopmentPlanCreated = "development-plan-created";
    public const string DevelopmentPlanActivityAdded = "development-plan-activity-added";
    public const string DevelopmentPlanActivityCompleted = "development-plan-activity-completed";
    public const string DevelopmentPlanApproved = "development-plan-approved";
    public const string DevelopmentPlanCompleted = "development-plan-completed";
}

public static class HrTalentActivityTypes
{
    public const string TalentReviewCreated = "talent-review-created";
    public const string TalentManagerReviewSubmitted = "talent-manager-review-submitted";
    public const string TalentHRReviewSubmitted = "talent-hr-review-submitted";
    public const string TalentReviewLinkedToPerformance = "talent-review-linked-to-performance";
    public const string TalentReviewUpdated = "talent-review-updated";
    public const string TalentReviewCompleted = "talent-review-completed";
    public const string SuccessionPlanCreated = "succession-plan-created";
    public const string SuccessionPlanUpdated = "succession-plan-updated";
    public const string SuccessionPlanDeleted = "succession-plan-deleted";
    public const string SuccessionPlanActivated = "succession-plan-activated";
    public const string SuccessionPlanCompleted = "succession-plan-completed";
    public const string SuccessionPlanCancelled = "succession-plan-cancelled";
    public const string SuccessionCandidateAdded = "succession-candidate-added";
    public const string SuccessionCandidateReadinessUpdated = "succession-candidate-readiness-updated";
    public const string SuccessionCandidateRemoved = "succession-candidate-removed";
    public const string TalentPipelineCreated = "talent-pipeline-created";
    public const string TalentPipelineUpdated = "talent-pipeline-updated";
    public const string TalentPipelineStageUpdated = "talent-pipeline-stage-updated";
    public const string TalentNineBoxPositionUpdated = "talent-nine-box-position-updated";
    public const string TalentEmployeeMarkedHighPotential = "talent-employee-marked-high-potential";
    public const string TalentReviewTemplateCreated = "talent-review-template-created";
    public const string TalentReviewTemplateUpdated = "talent-review-template-updated";
    public const string TalentReviewTemplateDeleted = "talent-review-template-deleted";
    public const string TalentReviewTemplateActivated = "talent-review-template-activated";
    public const string TalentReviewTemplateDeactivated = "talent-review-template-deactivated";
}

public static class HrLeaveActivityTypes
{
    public const string TravelPermissionCreated = "travel-permission-created";
    public const string TravelPermissionApproved = "travel-permission-approved";
    public const string TravelPermissionRejected = "travel-permission-rejected";
    public const string EmployeeLeaveConfigured = "employee-leave-configured";
    public const string EmployeeLeaveBackfillCompleted = "employee-leave-backfill-completed";
}

public static class HrRecruitmentActivityTypes
{
    public const string ApplicationSubmitted = "application-submitted";
    public const string InternalApplicationSubmitted = "internal-application-submitted";
    public const string InterviewCompleted = "interview-completed";
    public const string ApplicationRejectedAfterInterview = "application-rejected-after-interview";
    public const string OfferApprovalStepApproved = "offer-approval-step-approved";
    public const string OfferFullyApproved = "offer-fully-approved";
    public const string CandidateHired = "candidate-hired";
}

public static class PayrollActivityTypes
{
    public const string GradeBulkAssigned = "grade-bulk-assigned";
    public const string PayRunCompleted = "pay-run-completed";
    public const string PayRunCreated = "pay-run-created";
    public const string PayRunProcessed = "pay-run-processed";
    public const string PayRunApproved = "pay-run-approved";
    public const string PaymentCreated = "payment-created";
    public const string PaymentProcessed = "payment-processed";
    public const string PayrollRunPaymentsProcessed = "payroll-run-payments-processed";
    public const string ClaimApproved = "claim-approved";
    public const string ClaimRejected = "claim-rejected";
    public const string ClaimPaymentProcessed = "claim-payment-processed";
    public const string ClaimProcessed = "claim-processed";
    public const string AdvanceApproved = "advance-approved";
    public const string AdvanceRejected = "advance-rejected";
    public const string LoanApproved = "loan-approved";
    public const string AllowanceAssigned = "allowance-assigned";
    public const string DeductionAssigned = "deduction-assigned";
    public const string PayrollAdjustmentApproved = "payroll-adjustment-approved";
    public const string PayrollAdjustmentRejected = "payroll-adjustment-rejected";
    public const string PayrollItemApproved = "payroll-item-approved";
    public const string OvertimeApproved = "overtime-approved";
    public const string OvertimeRejected = "overtime-rejected";
    public const string ProvidentFundWithdrawalApproved = "provident-fund-withdrawal-approved";
    public const string ProvidentFundWithdrawalRejected = "provident-fund-withdrawal-rejected";
    public const string DirectDepositAccountVerified = "direct-deposit-account-verified";
    public const string GradeSalaryStepsUpserted = "grade-salary-steps-upserted";
}

public static class PlatformWorkflowActivityTypes
{
    public const string WorkflowConfigCreated = "workflow-config-created";
    public const string WorkflowConfigUpdated = "workflow-config-updated";
    public const string WorkflowConfigDeleted = "workflow-config-deleted";
    public const string WorkflowTemplateCreated = "workflow-template-created";
    public const string WorkflowTemplateUpdated = "workflow-template-updated";
    public const string WorkflowTemplateDeleted = "workflow-template-deleted";
    public const string WorkflowTemplateDuplicated = "workflow-template-duplicated";
    public const string WorkflowTemplateImported = "workflow-template-imported";
}

public static class IamActivityTypes
{
    public const string UserCreated = "user-created";
    public const string UserActivated = "user-activated";
    public const string UserDeactivated = "user-deactivated";
    public const string UserPasswordReset = "user-password-reset";
    public const string UserProfileUpdated = "user-profile-updated";
    public const string UserRoleAssigned = "user-role-assigned";
    public const string UserRoleRemoved = "user-role-removed";
    public const string RoleCreated = "role-created";
    public const string RoleDeleted = "role-deleted";
    public const string RolePermissionsAssigned = "role-permissions-assigned";
    public const string RolePermissionRemoved = "role-permission-removed";
    public const string TenantRegistered = "tenant-registered";
    public const string TenantLogoUpdated = "tenant-logo-updated";
    public const string TenantThemeUpdated = "tenant-theme-updated";
    public const string TenantSubscriptionUpdated = "tenant-subscription-updated";
}
