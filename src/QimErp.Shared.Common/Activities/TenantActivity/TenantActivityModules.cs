namespace QimErp.Shared.Common.Activities.TenantActivity;

public static class TenantActivityModules
{
    public const string Hr = "hr";
    public const string Accounting = "accounting";
    public const string Iam = "iam";
    public const string Auth = "auth";
    public const string Payroll = "payroll";
    public const string Platform = "platform";
    public const string Operations = "operations";
}

public static class HrActivityTypes
{
    public const string EmployeeCreated = "employee-created";
    public const string EmployeeUpdated = "employee-updated";
    public const string EmployeeProfilePictureUpdated = "employee-profile-picture-updated";
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
    public const string EmployeeJobTitleAssignmentDeleted = "employee-job-title-assignment-deleted";
    public const string EmployeeOrgUnitAssignmentDeleted = "employee-org-unit-assignment-deleted";
    public const string EmployeeJobStatusChanged = "employee-job-status-changed";
    public const string EmployeeStationChanged = "employee-station-changed";
    public const string EmployeeRankChanged = "employee-rank-changed";
    public const string EmployeeSupervisorChanged = "employee-supervisor-changed";
    public const string EmployeeDeleted = "employee-deleted";
}

public static class HrEmployeeRecordActivityTypes
{
    public const string EmployeeCertificationAdded = "employee-certification-added";
    public const string EmployeeCertificationDeleted = "employee-certification-deleted";
    public const string EmployeeCertificationUpdated = "employee-certification-updated";
    public const string EmployeeDependantAdded = "employee-dependant-added";
    public const string EmployeeDependantRemoved = "employee-dependant-removed";
    public const string EmployeeDependantUpdated = "employee-dependant-updated";
    public const string EmployeeDocumentUploaded = "employee-document-uploaded";
    public const string EmployeeDocumentDeleted = "employee-document-deleted";
    public const string EmployeeEducationalQualificationAdded = "employee-educational-qualification-added";
    public const string EmployeeEducationalQualificationDeleted = "employee-educational-qualification-deleted";
    public const string EmployeeEducationalQualificationUpdated = "employee-educational-qualification-updated";
    public const string EmployeeEmergencyContactRemoved = "employee-emergency-contact-removed";
    public const string EmployeeEmergencyContactUpdated = "employee-emergency-contact-updated";
    public const string EmployeeNextOfKinAdded = "employee-next-of-kin-added";
    public const string EmployeeNextOfKinDeleted = "employee-next-of-kin-deleted";
    public const string EmployeeNextOfKinUpdated = "employee-next-of-kin-updated";
    public const string EmployeeTrainingAdded = "employee-training-added";
    public const string EmployeeTrainingDeleted = "employee-training-deleted";
    public const string EmployeeTrainingUpdated = "employee-training-updated";
    public const string OrgChartRootSet = "org-chart-root-set";
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
    public const string CalibrationAnalyzed = "calibration-analyzed";
    public const string ModerationCommitteeEnabled = "moderation-committee-enabled";
    public const string Feedback360Created = "feedback360-created";
    public const string Feedback360ProviderAdded = "feedback360-provider-added";
    public const string Feedback360Completed = "feedback360-completed";
    public const string DevelopmentPlanCreated = "development-plan-created";
    public const string DevelopmentPlanActivityAdded = "development-plan-activity-added";
    public const string DevelopmentPlanActivityCompleted = "development-plan-activity-completed";
    public const string DevelopmentPlanApproved = "development-plan-approved";
    public const string DevelopmentPlanCompleted = "development-plan-completed";
    public const string CompetencyCreated = "competency-created";
    public const string CompetencyUpdated = "competency-updated";
    public const string CompetencyDeleted = "competency-deleted";
    public const string ConversationCreated = "conversation-created";
    public const string ConversationNoteAdded = "conversation-note-added";
    public const string ConversationActionItemAdded = "conversation-action-item-added";
    public const string ConversationCompleted = "conversation-completed";
    public const string StrategicPerspectiveCreated = "strategic-perspective-created";
    public const string StrategicPerspectiveUpdated = "strategic-perspective-updated";
    public const string StrategicObjectiveCreated = "strategic-objective-created";
    public const string KpiCreated = "kpi-created";
    public const string OuPerspectiveWeightCreated = "ou-perspective-weight-created";
    public const string OuPerspectiveWeightUpdated = "ou-perspective-weight-updated";
    public const string PerformanceTemplateCreated = "performance-template-created";
    public const string PerformanceTemplateUpdated = "performance-template-updated";
    public const string PerformanceTemplateDeleted = "performance-template-deleted";
    public const string PerformanceRatingScaleCreated = "performance-rating-scale-created";
    public const string PerformanceRatingScaleUpdated = "performance-rating-scale-updated";
    public const string PerformanceRatingScaleDeleted = "performance-rating-scale-deleted";
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

public static class HrLearningActivityTypes
{
    public const string CourseCreated = "course-created";
    public const string CoursePublished = "course-published";
    public const string EnrollmentCreated = "enrollment-created";
    public const string EnrollmentApproved = "enrollment-approved";
    public const string EnrollmentCompleted = "enrollment-completed";
    public const string TranscriptUploaded = "transcript-uploaded";
    public const string CertificationUploaded = "certification-uploaded";
    public const string LearningPathEnrolled = "learning-path-enrolled";
    public const string TrainingPaymentRequestCreated = "training-payment-request-created";
    public const string TrainingPaymentRequestApproved = "training-payment-request-approved";
    public const string TrainingPaymentMarkedPaid = "training-payment-marked-paid";
    public const string TrainingRefundRequestCreated = "training-refund-request-created";
    public const string TrainingRefundRequestApproved = "training-refund-request-approved";
    public const string TrainingRefundMarkedProcessed = "training-refund-marked-processed";
    public const string ProfessionalSubscriptionCreated = "professional-subscription-created";
    public const string ProfessionalSubscriptionApproved = "professional-subscription-approved";
    public const string ProfessionalSubscriptionMarkedPaid = "professional-subscription-marked-paid";

    // ── Courses ──────────────────────────────────────────────────────────────
    public const string CourseUpdated = "course-updated";
    public const string CourseDeleted = "course-deleted";
    public const string CourseArchived = "course-archived";

    // ── Assessments ──────────────────────────────────────────────────────────
    public const string AssessmentQuestionAdded = "assessment-question-added";
    public const string AssessmentCreated = "assessment-created";
    public const string AssessmentAttemptStarted = "assessment-attempt-started";
    public const string AssessmentAttemptSubmitted = "assessment-attempt-submitted";

    // ── Certificates ─────────────────────────────────────────────────────────
    public const string CertificateRevoked = "certificate-revoked";

    // ── Content management ───────────────────────────────────────────────────
    public const string CourseContentAdded = "course-content-added";
    public const string CourseModuleAdded = "course-module-added";
    public const string CourseContentDeleted = "course-content-deleted";
    public const string CourseModuleDeleted = "course-module-deleted";
    public const string CourseContentUpdated = "course-content-updated";
    public const string CourseModuleUpdated = "course-module-updated";

    // ── Enrollments ──────────────────────────────────────────────────────────
    public const string EnrollmentStarted = "enrollment-started";

    // ── Learning paths ───────────────────────────────────────────────────────
    public const string LearningPathCreated = "learning-path-created";
    public const string LearningPathDeleted = "learning-path-deleted";
    public const string LearningPathUpdated = "learning-path-updated";

    // ── Progress tracking ────────────────────────────────────────────────────
    public const string CourseContentProgressCompleted = "course-content-progress-completed";
    public const string CourseContentProgressStarted = "course-content-progress-started";

    // ── Recommendations ──────────────────────────────────────────────────────
    public const string CourseRecommendationAccepted = "course-recommendation-accepted";
    public const string CourseRecommendationRejected = "course-recommendation-rejected";
    public const string CourseRecommendationsGenerated = "course-recommendations-generated";

    // ── Skills ───────────────────────────────────────────────────────────────
    public const string EmployeeSkillAdded = "employee-skill-added";
    public const string SkillCreated = "skill-created";
    public const string SkillDeleted = "skill-deleted";
    public const string EmployeeSkillUpdated = "employee-skill-updated";
    public const string SkillUpdated = "skill-updated";
}

public static class HrNewsActivityTypes
{
    public const string NewsCreated = "news-created";
    public const string NewsPublished = "news-published";
    public const string NewsAttachmentAdded = "news-attachment-added";
    public const string NewsContributionCreated = "news-contribution-created";
    public const string NewsContributionUpdated = "news-contribution-updated";
    public const string NewsContributionDeleted = "news-contribution-deleted";
}

public static class HrLeaveActivityTypes
{
    public const string TravelPermissionCreated = "travel-permission-created";
    public const string TravelPermissionApproved = "travel-permission-approved";
    public const string TravelPermissionRejected = "travel-permission-rejected";
    public const string EmployeeLeaveConfigured = "employee-leave-configured";
    public const string EmployeeLeaveBackfillCompleted = "employee-leave-backfill-completed";
    public const string LeaveRecallCreated = "leave-recall-created";
    public const string LeaveRequestAttachmentDeleted = "leave-request-attachment-deleted";
    public const string LeaveRequestAttachmentUploaded = "leave-request-attachment-uploaded";
    public const string LeaveTypeUpdated = "leave-type-updated";
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

    // ── Applications / Candidates / Interviews (2026-07-14 audit sweep) ─────
    public const string ApplicationStageMoved = "application-stage-moved";
    public const string CandidateProfileCreated = "candidate-profile-created";
    public const string CandidateExperienceAdded = "candidate-experience-added";
    public const string InterviewScheduled = "interview-scheduled";

    // ── Job Requisitions ─────────────────────────────────────────────────────
    public const string JobRequisitionCreated = "job-requisition-created";
    public const string JobRequisitionUpdated = "job-requisition-updated";
    public const string JobRequisitionApprovalStepApproved = "job-requisition-approval-step-approved";
    public const string JobRequisitionApprovalStepAdded = "job-requisition-approval-step-added";
    public const string JobRequisitionRejected = "job-requisition-rejected";

    // ── Jobs ──────────────────────────────────────────────────────────────────
    public const string JobCreated = "job-created";
    public const string JobUpdated = "job-updated";
    public const string JobPublished = "job-published";
    public const string JobDuplicated = "job-duplicated";
    public const string JobPostingChannelAdded = "job-posting-channel-added";

    // ── Offers / Scorecards ───────────────────────────────────────────────────
    public const string OfferCreated = "offer-created";
    public const string ScorecardCreated = "scorecard-created";

    // ── Screening Rules ───────────────────────────────────────────────────────
    public const string ScreeningRuleCreated = "screening-rule-created";
    public const string ScreeningRuleUpdated = "screening-rule-updated";
    public const string ScreeningRuleDeleted = "screening-rule-deleted";
    public const string ScreeningRuleToggled = "screening-rule-toggled";
    public const string ScreeningRuleDuplicated = "screening-rule-duplicated";
}

public static class HrEngagementActivityTypes
{
    public const string DisciplinaryCaseCreated = "disciplinary-case-created";
    public const string DisciplinaryCaseInterdicted = "disciplinary-case-interdicted";
    public const string DisciplinaryCaseDecisionMade = "disciplinary-case-decision-made";
    public const string DisciplinaryCaseBonusWithheld = "disciplinary-case-bonus-withheld";
    public const string DisciplinaryCaseBonusReleased = "disciplinary-case-bonus-released";
    public const string DisciplinaryCaseHearingInvitationGenerated = "disciplinary-case-hearing-invitation-generated";
    public const string DisciplinaryCaseInterdictionLetterGenerated = "disciplinary-case-interdiction-letter-generated";
    public const string DisciplinaryCaseAuditReportRedacted = "disciplinary-case-audit-report-redacted";
    public const string DisciplinaryCaseAuditReportUploaded = "disciplinary-case-audit-report-uploaded";
    public const string DisciplinaryCaseHearingAudioUploaded = "disciplinary-case-hearing-audio-uploaded";
    public const string RecognitionCreated = "recognition-created";
    public const string HealthIssueCreated = "health-issue-created";
}

public static class SurveyActivityTypes
{
    public const string SurveyCreated = "survey-created";
    public const string SurveyUpdated = "survey-updated";
    public const string SurveyDeleted = "survey-deleted";
    public const string SurveyPublished = "survey-published";
    public const string SurveyClosed = "survey-closed";
    public const string SurveyArchived = "survey-archived";
    public const string SurveyCloned = "survey-cloned";
    public const string SurveyDistributed = "survey-distributed";
    public const string SurveyReminderSent = "survey-reminder-sent";
    public const string SurveyQuestionAdded = "survey-question-added";
    public const string SurveyQuestionRemoved = "survey-question-removed";
    public const string SurveyQuestionsReordered = "survey-questions-reordered";
    public const string SurveyQuestionUpdated = "survey-question-updated";
    public const string ShareLinkGenerated = "share-link-generated";
    public const string SurveyLookupCreated = "survey-lookup-created";
    public const string SurveyLookupUpdated = "survey-lookup-updated";
    public const string SurveyLookupDeleted = "survey-lookup-deleted";
    public const string SurveyResponseSubmitted = "survey-response-submitted";
    public const string SurveysBulkOperated = "surveys-bulk-operated";
}

public static class BenefitActivityTypes
{
    public const string AccommodationCreated = "accommodation-created";
    public const string AccommodationAllocated = "accommodation-allocated";
    public const string AccommodationVacated = "accommodation-vacated";
    public const string BenefitPlanActivated = "benefit-plan-activated";
    public const string BenefitPlanCreated = "benefit-plan-created";
    public const string BenefitPlanDeactivated = "benefit-plan-deactivated";
    public const string BenefitPlanDeleted = "benefit-plan-deleted";
    public const string BenefitPlanUpdated = "benefit-plan-updated";
    public const string BenefitTypeCreated = "benefit-type-created";
    public const string BenefitTypeDeleted = "benefit-type-deleted";
    public const string BenefitTypeUpdated = "benefit-type-updated";
    public const string AutoEnrollmentConfigUpdated = "auto-enrollment-config-updated";
    public const string LifeEventConfigUpdated = "life-event-config-updated";
    public const string ManualEnrollmentConfigUpdated = "manual-enrollment-config-updated";
    public const string OpenEnrollmentConfigUpdated = "open-enrollment-config-updated";
    public const string DependentCreated = "dependent-created";
    public const string DependentDeleted = "dependent-deleted";
    public const string DependentUpdated = "dependent-updated";
    public const string BenefitEnrollmentCreated = "benefit-enrollment-created";
    public const string BenefitEnrollmentTerminated = "benefit-enrollment-terminated";
    public const string BenefitEnrollmentUpdated = "benefit-enrollment-updated";
    public const string HouseCategoryCreated = "house-category-created";
    public const string HouseCategoryDeleted = "house-category-deleted";
    public const string HouseCategoryUpdated = "house-category-updated";
    public const string BenefitLoanCreated = "benefit-loan-created";
    public const string BenefitLoanRepaymentRecorded = "benefit-loan-repayment-recorded";
    public const string BenefitLoanStatusUpdated = "benefit-loan-status-updated";
    public const string BenefitLookupCreated = "benefit-lookup-created";
    public const string BenefitLookupDeleted = "benefit-lookup-deleted";
    public const string BenefitLookupUpdated = "benefit-lookup-updated";
    public const string TenancyAgreementCreated = "tenancy-agreement-created";
    public const string TenancyAgreementDocumentAdded = "tenancy-agreement-document-added";
    public const string TenancyAgreementDocumentRemoved = "tenancy-agreement-document-removed";
    public const string TenancyAgreementRenewed = "tenancy-agreement-renewed";
    public const string WaitingListEntryAdded = "waiting-list-entry-added";
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
    public const string ClaimReadyForPayment = "claim-ready-for-payment";
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

    // ── Pay runs / payslips / payroll items ─────────────────────────────────
    public const string PayRunUpdated = "pay-run-updated";
    public const string PayRunCancelled = "pay-run-cancelled";
    public const string PayRunDeleted = "pay-run-deleted";
    public const string PayRunFinalized = "pay-run-finalized";
    public const string PayRunReversed = "pay-run-reversed";
    public const string PayRunSubmittedForApproval = "pay-run-submitted-for-approval";
    public const string PayslipGenerated = "payslip-generated";
    public const string PayslipsGeneratedBulk = "payslips-generated-bulk";
    public const string PayrollRunStaleDraftsCleanedUp = "payroll-run-stale-drafts-cleaned-up";
    public const string PayrollItemMarkedAsPaid = "payroll-item-marked-as-paid";

    // ── Accrual runs / statutory submissions ────────────────────────────────
    public const string PayrollAccrualRunCreated = "payroll-accrual-run-created";
    public const string PayrollAccrualRunPosted = "payroll-accrual-run-posted";
    public const string PayrollAccrualRunSubmittedForApproval = "payroll-accrual-run-submitted-for-approval";
    public const string StatutoryFilingSubmittedForApproval = "statutory-filing-submitted-for-approval";

    // ── Claims / advances / loans ────────────────────────────────────────────
    public const string ClaimDocumentCreated = "claim-document-created";
    public const string ClaimCreated = "claim-created";
    public const string AdvanceCreated = "advance-created";
    public const string AdvanceDeleted = "advance-deleted";
    public const string AdvanceUpdated = "advance-updated";
    public const string LoanCreated = "loan-created";
    public const string LoanUpdated = "loan-updated";
    public const string LoanDeleted = "loan-deleted";
    public const string LoanCancelled = "loan-cancelled";
    public const string LoanRepaymentCreated = "loan-repayment-created";
    public const string LoanRepaymentPaid = "loan-repayment-paid";

    // ── Overtime / adjustments ───────────────────────────────────────────────
    public const string OvertimeCreated = "overtime-created";
    public const string OvertimeRateConfigsUpserted = "overtime-rate-configs-upserted";
    public const string PayrollAdjustmentCreated = "payroll-adjustment-created";

    // ── Allowances / deductions ───────────────────────────────────────────────
    public const string AllowanceActivated = "allowance-activated";
    public const string AllowanceCreated = "allowance-created";
    public const string AllowanceDeleted = "allowance-deleted";
    public const string AllowanceUpdated = "allowance-updated";
    public const string AllowanceAssignmentUpdated = "allowance-assignment-updated";
    public const string AllowanceAssignmentDeleted = "allowance-assignment-deleted";
    public const string DeductionActivated = "deduction-activated";
    public const string DeductionCreated = "deduction-created";
    public const string DeductionDeleted = "deduction-deleted";
    public const string DeductionUpdated = "deduction-updated";
    public const string DeductionAssignmentUpdated = "deduction-assignment-updated";
    public const string DeductionAssignmentDeleted = "deduction-assignment-deleted";
    public const string DeductionAssignmentEnded = "deduction-assignment-ended";

    // ── Salary structures / components / grades ─────────────────────────────
    public const string SalaryComponentCreated = "salary-component-created";
    public const string SalaryComponentUpdated = "salary-component-updated";
    public const string SalaryComponentDeleted = "salary-component-deleted";
    public const string SalaryStructureCreated = "salary-structure-created";
    public const string SalaryStructureUpdated = "salary-structure-updated";
    public const string SalaryStructureActivated = "salary-structure-activated";
    public const string SalaryStructureDeactivated = "salary-structure-deactivated";
    public const string GradeCreated = "grade-created";
    public const string GradeUpdated = "grade-updated";
    public const string GradeDeleted = "grade-deleted";
    public const string SalaryComponentsBulkAssigned = "salary-components-bulk-assigned";
    public const string GradeNotchBulkAssigned = "grade-notch-bulk-assigned";

    // ── Payroll groups ────────────────────────────────────────────────────────
    public const string PayrollGroupCreated = "payroll-group-created";
    public const string PayrollGroupUpdated = "payroll-group-updated";
    public const string PayrollGroupActivated = "payroll-group-activated";
    public const string PayrollGroupDeactivated = "payroll-group-deactivated";
    public const string PayrollGroupDeleted = "payroll-group-deleted";
    public const string PayrollGroupMemberAdded = "payroll-group-member-added";
    public const string PayrollGroupMemberRemoved = "payroll-group-member-removed";

    // ── Tax documents / configuration ────────────────────────────────────────
    public const string TaxDocumentCreated = "tax-document-created";
    public const string TaxDocumentGenerated = "tax-document-generated";
    public const string TaxDocumentSent = "tax-document-sent";
    public const string TaxDocumentFiled = "tax-document-filed";
    public const string TaxDocumentsBulkGenerated = "tax-documents-bulk-generated";
    public const string EmployeeTaxProfileCreated = "employee-tax-profile-created";
    public const string EmployeeTaxProfileUpdated = "employee-tax-profile-updated";
    public const string TaxBracketCreated = "tax-bracket-created";
    public const string TaxBracketUpdated = "tax-bracket-updated";
    public const string TaxBracketDeleted = "tax-bracket-deleted";
    public const string TaxConfigurationCreated = "tax-configuration-created";
    public const string TaxConfigurationUpdated = "tax-configuration-updated";
    public const string TaxConfigurationDeleted = "tax-configuration-deleted";

    // ── Direct deposit / mobile money ────────────────────────────────────────
    public const string DirectDepositAccountCreated = "direct-deposit-account-created";
    public const string DirectDepositAccountUpdated = "direct-deposit-account-updated";
    public const string DirectDepositAccountDeleted = "direct-deposit-account-deleted";
    public const string MobileMoneyAccountCreated = "mobile-money-account-created";
    public const string MobileMoneyAccountUpdated = "mobile-money-account-updated";
    public const string MobileMoneyAccountDeleted = "mobile-money-account-deleted";

    // ── SSNIT / statutory submissions ────────────────────────────────────────
    public const string SsnitConfigurationCreated = "ssnit-configuration-created";
    public const string SsnitConfigurationUpdated = "ssnit-configuration-updated";
    public const string SsnitConfigurationDeleted = "ssnit-configuration-deleted";
    public const string StatutorySubmissionFiled = "statutory-submission-filed";
    public const string StatutorySubmissionsBulkFiled = "statutory-submissions-bulk-filed";

    // ── Payroll schedules ────────────────────────────────────────────────────
    public const string PayrollScheduleCreated = "payroll-schedule-created";
    public const string PayrollScheduleActivated = "payroll-schedule-activated";

    // ── Provident fund (system/bulk) ─────────────────────────────────────────
    public const string ProvidentFundContributionsBulkProcessed = "provident-fund-contributions-bulk-processed";
    public const string ProvidentFundInterestBulkCalculated = "provident-fund-interest-bulk-calculated";
    public const string ProvidentFundArrearsBulkApplied = "provident-fund-arrears-bulk-applied";
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
    public const string EntityWorkflowDefinitionPublished = "entity-workflow-definition-published";
    public const string EmployeeCreateWorkflowSynced = "employee-create-workflow-synced";
}

public static class PlatformNotificationActivityTypes
{
    public const string ChatNotifySettingsUpdated = "chat-notify-settings-updated";
    public const string ConferenceNotifySettingsUpdated = "conference-notify-settings-updated";
    public const string SmsNotifySettingsUpdated = "sms-notify-settings-updated";
    public const string WebhookNotifySettingsUpdated = "webhook-notify-settings-updated";
}

public static class IamActivityTypes
{
    public const string UserCreated = "user-created";
    public const string UserActivated = "user-activated";
    public const string UserDeactivated = "user-deactivated";
    public const string UserPasswordReset = "user-password-reset";
    public const string UserPasswordChanged = "user-password-changed";
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

    // ── Lookups (database-types, billing-cycles, company-types, industry-types, auth-provider-types) ──
    public const string LookupCreated = "lookup-created";
    public const string LookupUpdated = "lookup-updated";
    public const string LookupDeleted = "lookup-deleted";

    // ── App Store ────────────────────────────────────────────────────────────
    public const string AppStoreItemInstalled = "app-store-item-installed";
    public const string AppStoreItemUninstalled = "app-store-item-uninstalled";

    // ── Tenant reference data: currencies / exchange rates / holidays / banks ─
    public const string TenantCurrencyCreated = "tenant-currency-created";
    public const string TenantCurrencyUpdated = "tenant-currency-updated";
    public const string TenantCurrencyDeleted = "tenant-currency-deleted";
    public const string TenantExchangeRateCreated = "tenant-exchange-rate-created";
    public const string TenantExchangeRateUpdated = "tenant-exchange-rate-updated";
    public const string TenantExchangeRateDeleted = "tenant-exchange-rate-deleted";
    public const string TenantHolidayCreated = "tenant-holiday-created";
    public const string TenantHolidayUpdated = "tenant-holiday-updated";
    public const string TenantHolidayDeleted = "tenant-holiday-deleted";
    public const string TenantBankCreated = "tenant-bank-created";
    public const string TenantBankUpdated = "tenant-bank-updated";
    public const string TenantBankDeleted = "tenant-bank-deleted";

    // ── RBAC permissions catalog ───────────────────────────────────────────────
    public const string PermissionCreated = "permission-created";
    public const string PermissionUpdated = "permission-updated";
    public const string PermissionDeleted = "permission-deleted";

    // ── Account activation (first-time, self) ──────────────────────────────────
    public const string AccountActivated = "account-activated";

    // ── Tenant subscriptions ────────────────────────────────────────────────────
    public const string SubscriptionCreated = "subscription-created";
    public const string SubscriptionUpdated = "subscription-updated";
    public const string SubscriptionCancelled = "subscription-cancelled";
    public const string SubscriptionSuspended = "subscription-suspended";
    public const string SubscriptionResumed = "subscription-resumed";
    public const string SubscriptionDeleted = "subscription-deleted";
    public const string SubscriptionPaymentVerified = "subscription-payment-verified";
}

public static class AuthActivityTypes
{
    public const string LoginSuccess = "login-success";
    public const string LoginFailed = "login-failed";
    public const string LoginBlockedByLockout = "login-blocked-by-lockout";
    public const string LoginTwoFactorRequired = "login-two-factor-required";
    public const string AccountLockout = "account-lockout";
    public const string AccountUnlocked = "account-unlocked";
    public const string TwoFactorEnabled = "two-factor-enabled";
    public const string TwoFactorVerified = "two-factor-verified";
    public const string TwoFactorDisabled = "two-factor-disabled";
    public const string TwoFactorChallengeFailed = "two-factor-challenge-failed";
    public const string BackupCodesRegenerated = "backup-codes-regenerated";
    public const string SessionRevoked = "session-revoked";
    public const string SessionsRevokedAll = "sessions-revoked-all";
}

public static class OperationsInventoryActivityTypes
{
    public const string ProductCreated = "product-created";
    public const string ProductUpdated = "product-updated";
    public const string ProductDeleted = "product-deleted";
    public const string WarehouseCreated = "warehouse-created";
    public const string WarehouseUpdated = "warehouse-updated";
    public const string WarehouseDeleted = "warehouse-deleted";
    public const string PriceAdjustmentCreated = "price-adjustment-created";
    public const string QuantityAdjustmentCreated = "quantity-adjustment-created";
    public const string StockMovementCreated = "stock-movement-created";
    public const string StockTransferCreated = "stock-transfer-created";
}

public static class OperationsProjectActivityTypes
{
    public const string ProjectCreated = "project-created";
    public const string ProjectUpdated = "project-updated";
    public const string ProjectDeleted = "project-deleted";
    public const string ProjectStatusChanged = "project-status-changed";
    public const string ProjectMilestoneCreated = "project-milestone-created";
    public const string ProjectTaskCreated = "project-task-created";
    public const string ProjectTaskUpdated = "project-task-updated";
    public const string ProjectTaskDeleted = "project-task-deleted";
    public const string ProjectTimeEntryCreated = "project-time-entry-created";
    public const string ProjectTimeEntryUpdated = "project-time-entry-updated";
    public const string ProjectTimeEntrySubmitted = "project-time-entry-submitted";
    public const string ProjectTimeEntryApproved = "project-time-entry-approved";
    public const string ProjectTimeEntryRejected = "project-time-entry-rejected";
    public const string ProjectBudgetCreated = "project-budget-created";
    public const string ProjectBudgetBaselineMarked = "project-budget-baseline-marked";
    public const string ProjectContractCreated = "project-contract-created";
    public const string ProjectExpenditureCreated = "project-expenditure-created";
    public const string ProjectExpenditureUpdated = "project-expenditure-updated";
    public const string ProjectExpenditureDeleted = "project-expenditure-deleted";
    public const string ProjectBillingRuleCreated = "project-billing-rule-created";
    public const string ProjectBillGenerated = "project-bill-generated";
}
