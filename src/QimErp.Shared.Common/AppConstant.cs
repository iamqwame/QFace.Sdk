namespace QimErp.Shared.Common;

public static class AppConstant
{
    public const string DefaultTenantId = "default";
    public const string DevTenantId = "hubtel";

    public static class Service
    {
        public static class Core
        {
            public const string DistributionBaseOnTheBase = "Base";
            public const string DistributionBaseOnThePercentageOfTheBase = "% of Tax";
        }

        public static class IncludedInPrice
        {
            public const string Default = "None";
            public const string TaxIncluded = "TaxIncluded";
        }

        public static class TaxType
        {
            public const string Default = "None";
            public const string Sales = "Sales";
            public const string Purchases = "Purchases";
        }

        public static class TaxComputationMethod
        {
            public const string Fixed = "Fixed";
            public const string Percentage = "Percentage";
            public const string PercentageOfTaxIncluded = "PercentageOfTaxIncluded";
            public const string Group = "Group";
        }

        public class TaxScope
        {
            public const string Default = "None";
        }
    }

    /// <summary>
    /// Payroll module constants for lookup codes and system defaults
    /// </summary>
    public static class Payroll
    {
        /// <summary>
        /// Deduction type codes used across the system
        /// </summary>
        public static class DeductionType
        {
            public const string Tax = "TAX";
            public const string SocialSecurity = "SOCIALSECURITY";
            public const string Pension = "PENSION";
            public const string Insurance = "INSURANCE";
            public const string Loan = "LOAN";
            public const string Housing = "HOUSING";
            public const string Other = "OTHER";
        }

        /// <summary>
        /// Allowance type codes used across the system
        /// </summary>
        public static class AllowanceType
        {
            public const string Housing = "HOUSING";
            public const string Transport = "TRANSPORT";
            public const string Medical = "MEDICAL";
            public const string Meal = "MEAL";
            public const string Communication = "COMMUNICATION";
            public const string Acting = "ACTING";
            public const string Special = "SPECIAL";
            public const string Other = "OTHER";
        }

        /// <summary>
        /// Loan type codes used across the system
        /// </summary>
        public static class LoanType
        {
            public const string Personal = "PERSONAL";
            public const string Emergency = "EMERGENCY";
            public const string SalaryAdvance = "SALARYADVANCE";
            public const string Vehicle = "VEHICLE";
            public const string Housing = "HOUSING";
        }

        /// <summary>
        /// Common deduction codes used across the system
        /// </summary>
        public static class Deduction
        {
            public const string AccommodationRent = "ACCOMMODATION-RENT";
            public const string BonusWithheld = "BONUS-WITHHELD";
        }

        /// <summary>
        /// Claim type codes used across the system
        /// </summary>
        public static class ClaimType
        {
            public const string Transportation = "TRANSPORTATION";
            public const string Halting = "HALTING";
            public const string Overtime = "OVERTIME";
            public const string Inconvenience = "INCONVENIENCE";
            public const string Acting = "ACTING";
            public const string PermanentTransfer = "PERMANENTTRANSFER";
            public const string KeyHolding = "KEYHOLDING";
            public const string Special = "SPECIAL";
            public const string DayTrip = "DAYTRIP";
            public const string Medical = "MEDICAL";
        }

        /// <summary>
        /// Bonus type codes used across the system
        /// </summary>
        public static class BonusType
        {
            public const string Performance = "PERFORMANCE";
            public const string Annual = "ANNUAL";
            public const string Project = "PROJECT";
            public const string Referral = "REFERRAL";
            public const string Custom = "CUSTOM";
        }

        /// <summary>
        /// Expense type codes used in claim processing
        /// </summary>
        public static class ExpenseType
        {
            public const string Mileage = "MILEAGE";
            public const string Accommodation = "ACCOMMODATION";
        }
    }

    /// <summary>
    /// Constants and utilities for blob storage operations
    /// </summary>
    public static class BlobStorage
    {
        /// <summary>
        /// Folder name constants for blob storage organization
        /// </summary>
        public static class Folders
        {
            public const string Employees = "employees";
            public const string Vendors = "vendors";
            public const string Customers = "customers";
            public const string Products = "products";
            public const string Invoices = "invoices";
            public const string Payments = "payments";
            public const string Reports = "reports";
            public const string Templates = "templates";
        }

        /// <summary>
        /// Utilities for employee document folder paths
        /// </summary>
        public static class EmployeeDocuments
        {
            /// <summary>
            /// Gets the folder path for employee documents
            /// </summary>
            /// <param name="employeeId">The employee ID</param>
            /// <param name="documentType">The document type (optional)</param>
            /// <returns>Folder path for employee documents</returns>
            public static string GetEmployeeDocumentsFolder(Guid employeeId, string? documentType = null)
            {
                var basePath = $"qimerp/{Folders.Employees}/{employeeId}";
                return documentType != null ? $"{basePath}/{documentType}" : basePath;
            }

            /// <summary>
            /// Gets the folder path for employee documents with tenant support (for future use)
            /// </summary>
            /// <param name="employeeId">The employee ID</param>
            /// <param name="documentType">The document type (optional)</param>
            /// <param name="tenantId">The tenant ID (optional, defaults to DefaultTenantId)</param>
            /// <returns>Folder path for employee documents with tenant</returns>
            public static string GetEmployeeDocumentsFolderWithTenant(Guid employeeId, string? documentType = null,
                string? tenantId = null)
            {
                var tenant = tenantId ?? DefaultTenantId;
                var basePath = $"{tenant}/{Folders.Employees}/{employeeId}";
                return documentType != null ? $"{basePath}/{documentType}" : basePath;
            }
        }

    }

    // Api.Tags and Api.Url.Hr have been moved to the modules that own them:
    // - CoreHR tags + URLs: SharedConstant in QimErp.CoreHr.Employee.Shared.Constants
    // - HROperations Recruitment: SharedConstant in QimErp.HrOperations.Recruitment.Shared.Constants
    // - HROperations Surveys: SurveysConstants in QimErp.HrOperations.Surveys.Shared.Constants
    // - HROperations EmployeeEngagement: EngagementConstants in QimErp.HrOperations.EmployeeEngagement.Shared.Constants
    // - Learning blob storage: LearningStorageConstants in QimErp.CoreHr.Learning.Shared.Constants

    // PLACEHOLDER — remove this comment block once all projects have migrated
    // to their local constants. This class is intentionally empty now.
    // ReSharper disable once UnusedType.Global
    private static class ApiMigrationNote
    {
        // Intentionally empty.
    }

    /// <summary>
    /// Shared cache constants. Module-specific keys, TTLs, and patterns live in each module:
    /// - CoreHR: HrCacheConstants in QimErp.CoreHr.Employee.Shared.Constants
    /// - IAM: SharedConstants.Cache in QimErp.IAM.Shared
    /// - Leave: LeaveCacheConstants in QimErp.HrOperations.Leave.Shared
    /// - Recruitment: RecruitmentCacheConstants in QimErp.HrOperations.Recruitment.Shared.Constants
    /// - Workflow: WorkflowCacheConstants in QimErp.Platform.Workflow.Shared.Constants
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// Cache regions for different modules
        /// </summary>
        public static class Regions
        {
            public const string Auth = "auth";
            public const string Hr = "hr";
            public const string Ap = "ap";
            public const string Ar = "ar";
            public const string Inventory = "inventory";
            public const string Core = "core";
            public const string Workflow = "workflow";
        }
    }

    /// <summary>
    /// RabbitMQ messaging constants
    /// </summary>
    public static class RabbitMq
    {
        /// <summary>
        /// Exchange names for RabbitMQ message routing
        /// </summary>
        public static class Exchanges
        {
            /// <summary>
            /// Core notification exchange for email and SMS messages
            /// </summary>
            public const string Notifications = "qimerp.core.notify.prod_exchange";

            /// <summary>
            /// Tenant-related exchanges
            /// </summary>
            public static class Tenant
            {
                public const string Registered = "qimerp.tenant.registered.prod_exchange";
            }

            /// <summary>
            /// HR module exchanges
            /// </summary>
            public static class Hr
            {
                /// <summary>
                /// Employee lifecycle exchanges
                /// </summary>
                public static class Employee
                {
                    public const string Created = "qimerp.hr.employee_created.prod_exchange";
                    public const string Updated = "qimerp.hr.employee_updated.prod_exchange";
                    public const string Deleted = "qimerp.hr.employee_deleted.prod_exchange";
                    public const string JobTitleChanged = "qimerp.hr.employee_job_title_changed.prod_exchange";
                    public const string DepartmentChanged = "qimerp.hr.employee_department_changed.prod_exchange";
                    public const string OrganizationalUnitChanged = "qimerp.hr.employee_org_unit_changed.prod_exchange";
                    public const string JobStatusChanged = "qimerp.hr.employee_job_status_changed.prod_exchange";
                }

                /// <summary>
                /// HR administrative data exchanges (departments, ranks, etc.)
                /// </summary>
                public static class Admin
                {
                    public const string JobTitleUpdated = "qimerp.hr.job_title_updated.prod_exchange";
                    public const string JobTitleDeleted = "qimerp.hr.job_title_deleted.prod_exchange";
                    public const string DepartmentUpdated = "qimerp.hr.department_updated.prod_exchange";
                    public const string DepartmentDeleted = "qimerp.hr.department_deleted.prod_exchange";

                    public const string OrganizationalUnitUpdated =
                        "qimerp.hr.organizational_unit_updated.prod_exchange";

                    public const string OrganizationalUnitDeleted =
                        "qimerp.hr.organizational_unit_deleted.prod_exchange";

                    public const string StationUpdated = "qimerp.hr.station_updated.prod_exchange";
                    public const string StationDeleted = "qimerp.hr.station_deleted.prod_exchange";
                    public const string JobStatusUpdated = "qimerp.hr.job_status_updated.prod_exchange";
                    public const string JobStatusDeleted = "qimerp.hr.job_status_deleted.prod_exchange";
                    public const string LeaveTypeUpdated = "qimerp.hr.leave_type_updated.prod_exchange";
                }

                /// <summary>
                /// Company News module exchanges
                /// </summary>
                public static class News
                {
                    public const string NewsPublished = "qimerp.hr.news_published.prod_exchange";
                }

                /// <summary>
                /// Recruitment module exchanges
                /// </summary>
                public static class Recruitment
                {
                    public const string JobRequisitionCreated =
                        "qimerp.recruitment.job_requisition_created.prod_exchange";

                    public const string JobRequisitionUpdated =
                        "qimerp.recruitment.job_requisition_updated.prod_exchange";

                    public const string JobRequisitionDeleted =
                        "qimerp.recruitment.job_requisition_deleted.prod_exchange";
                }

                /// <summary>
                /// Employee Engagement module exchanges
                /// </summary>
                public static class Engagement
                {
                    public const string RiskCreated = "qimerp.engagement.risk_created.prod_exchange";

                    public const string DisciplinaryCaseCreated =
                        "qimerp.engagement.disciplinary_case_created.prod_exchange";

                    public const string DisciplinaryCaseInterdicted =
                        "qimerp.engagement.disciplinary_case_interdicted.prod_exchange";

                    public const string DisciplinaryCaseExonerated =
                        "qimerp.engagement.disciplinary_case_exonerated.prod_exchange";

                    public const string DisciplinaryCaseBonusWithheld =
                        "qimerp.engagement.disciplinary_case_bonus_withheld.prod_exchange";

                    public const string DisciplinaryCaseBonusReleased =
                        "qimerp.engagement.disciplinary_case_bonus_released.prod_exchange";

                    public const string HealthIssueCreated = "qimerp.engagement.health_issue_created.prod_exchange";
                }

                /// <summary>
                /// Performance Management module exchanges
                /// </summary>
                public static class Performance
                {
                    public const string ReviewCreated = "qimerp.performance.review_created.prod_exchange";
                    public const string ReviewCompleted = "qimerp.performance.review_completed.prod_exchange";
                    public const string GoalCreated = "qimerp.performance.goal_created.prod_exchange";
                    public const string GoalCompleted = "qimerp.performance.goal_completed.prod_exchange";
                    public const string GoalProgressUpdated = "qimerp.performance.goal_progress_updated.prod_exchange";

                    public const string DevelopmentPlanCreated =
                        "qimerp.performance.development_plan_created.prod_exchange";

                    public const string Feedback360Completed = "qimerp.performance.feedback360_completed.prod_exchange";
                    public const string CheckInCreated = "qimerp.performance.check_in_created.prod_exchange";
                    public const string CalibrationCompleted = "qimerp.performance.calibration_completed.prod_exchange";
                }

                /// <summary>
                /// Learning and Development module exchanges
                /// </summary>
                public static class Learning
                {
                    public const string CourseCreated = "qimerp.learning.course_created.prod_exchange";
                    public const string CoursePublished = "qimerp.learning.course_published.prod_exchange";
                    public const string EnrollmentCreated = "qimerp.learning.enrollment_created.prod_exchange";
                    public const string EnrollmentApproved = "qimerp.learning.enrollment_approved.prod_exchange";
                    public const string EnrollmentCompleted = "qimerp.learning.enrollment_completed.prod_exchange";
                    public const string CertificateIssued = "qimerp.learning.certificate_issued.prod_exchange";
                    public const string SubscriptionCreated = "qimerp.learning.subscription_created.prod_exchange";
                    public const string SubscriptionFinanceApproved = "qimerp.learning.subscription_finance_approved.prod_exchange";
                    public const string SubscriptionPaid = "qimerp.learning.subscription_paid.prod_exchange";
                    public const string PaymentRequestCreated = "qimerp.learning.payment_request_created.prod_exchange";
                    public const string PaymentRequestFinanceApproved = "qimerp.learning.payment_request_finance_approved.prod_exchange";
                    public const string PaymentRequestPaid = "qimerp.learning.payment_request_paid.prod_exchange";
                    public const string RefundRequestCreated = "qimerp.learning.refund_request_created.prod_exchange";
                    public const string RefundRequestFinanceApproved = "qimerp.learning.refund_request_finance_approved.prod_exchange";
                    public const string RefundRequestProcessed = "qimerp.learning.refund_request_processed.prod_exchange";
                }

                /// <summary>
                /// Talent Management module exchanges
                /// </summary>
                public static class Talent
                {
                    public const string PipelineCreated = "qimerp.talent.pipeline_created.prod_exchange";
                    public const string PipelineUpdated = "qimerp.talent.pipeline_updated.prod_exchange";
                    public const string HighPotentialIdentified = "qimerp.talent.high_potential_identified.prod_exchange";
                    public const string SuccessionPlanCreated = "qimerp.talent.succession_plan_created.prod_exchange";
                    public const string SuccessionPlanUpdated = "qimerp.talent.succession_plan_updated.prod_exchange";
                    public const string TalentReviewCreated = "qimerp.talent.review_created.prod_exchange";
                    public const string TalentReviewCompleted = "qimerp.talent.review_completed.prod_exchange";
                    public const string SuccessionRiskIdentified = "qimerp.talent.succession_risk_identified.prod_exchange";
                    public const string TalentReviewTemplateCreated = "qimerp.talent.review_template_created.prod_exchange";
                }

                /// <summary>
                /// Benefit Management module exchanges
                /// </summary>
                public static class Benefit
                {
                    public const string EnrollmentCreated = "qimerp.benefit.enrollment_created.prod_exchange";
                    public const string EnrollmentUpdated = "qimerp.benefit.enrollment_updated.prod_exchange";
                    public const string EnrollmentTerminated = "qimerp.benefit.enrollment_terminated.prod_exchange";
                    public const string LoanCreated = "qimerp.benefit.loan_created.prod_exchange";
                    public const string LoanRepayment = "qimerp.benefit.loan_repayment.prod_exchange";
                    public const string PlanActivated = "qimerp.benefit.plan_activated.prod_exchange";
                    public const string AccommodationAllocated = "qimerp.benefit.accommodation_allocated.prod_exchange";
                    public const string AccommodationVacated = "qimerp.benefit.accommodation_vacated.prod_exchange";
                }

                /// <summary>
                /// Leave Management module exchanges
                /// </summary>
                public static class Leave
                {
                    public const string RequestApproved = "qimerp.leave.request_approved.prod_exchange";
                    public const string RequestRejected = "qimerp.leave.request_rejected.prod_exchange";
                    public const string TravelPermissionCreated = "qimerp.leave.travel_permission_created.prod_exchange";
                    public const string TravelPermissionApproved = "qimerp.leave.travel_permission_approved.prod_exchange";
                    public const string TravelPermissionRejected = "qimerp.leave.travel_permission_rejected.prod_exchange";
                }

                /// <summary>
                /// Payroll module exchanges
                /// </summary>
                public static class Payroll
                {
                    public const string RunCompleted = "qimerp.payroll.run_completed.prod_exchange";
                    public const string PayslipGenerated = "qimerp.payroll.payslip_generated.prod_exchange";
                }
            }

            /// <summary>
            /// General Ledger (GL) module exchanges
            /// </summary>
            public static class Gl
            {
                /// <summary>
                /// GL administrative data exchanges (currencies, chart of accounts, etc.)
                /// </summary>
                public static class Admin
                {
                    public const string CurrencyUpdated = "qimerp.gl.currency_updated.prod_exchange";
                    public const string CurrencyDeleted = "qimerp.gl.currency_deleted.prod_exchange";
                    public const string ChartOfAccountUpdated = "qimerp.gl.chart_of_account_updated.prod_exchange";
                    public const string ChartOfAccountDeleted = "qimerp.gl.chart_of_account_deleted.prod_exchange";
                    public const string CostCenterUpdated = "qimerp.gl.cost_center_updated.prod_exchange";
                    public const string CostCenterDeleted = "qimerp.gl.cost_center_deleted.prod_exchange";
                    public const string FiscalPeriodUpdated = "qimerp.gl.fiscal_period_updated.prod_exchange";
                    public const string FiscalYearUpdated = "qimerp.gl.fiscal_year_updated.prod_exchange";
                    public const string JournalEntryPosted = "qimerp.gl.journal_entry_posted.prod_exchange";
                }

                /// <summary>
                /// Budget Planning module exchanges
                /// </summary>
                public static class BudgetPlanning
                {
                    public const string BudgetCreated = "qimerp.gl.budget_created.prod_exchange";
                    public const string BudgetUpdated = "qimerp.gl.budget_updated.prod_exchange";
                    public const string BudgetApproved = "qimerp.gl.budget_approved.prod_exchange";
                    public const string BudgetActivated = "qimerp.gl.budget_activated.prod_exchange";
                    public const string BudgetClosed = "qimerp.gl.budget_closed.prod_exchange";
                }
            }

            /// <summary>
            /// Project Management module exchanges
            /// </summary>
            public static class Project
            {
                /// <summary>
                /// Project lifecycle exchanges
                /// </summary>
                public static class Admin
                {
                    public const string ProjectCreated = "qimerp.project.created.prod_exchange";
                    public const string ProjectUpdated = "qimerp.project.updated.prod_exchange";
                    public const string ProjectDeleted = "qimerp.project.deleted.prod_exchange";
                }
            }

            /// <summary>
            /// Cash Management module exchanges
            /// </summary>
            public static class CashManagement
            {
                public const string BankTransactionCreated = "qimerp.cash_management.bank_transaction_created.prod_exchange";
                public const string BankReconciliationCompleted = "qimerp.cash_management.bank_reconciliation_completed.prod_exchange";
            }
        }
    }

    /// <summary>
    /// Workflow module constants
    /// </summary>
    public static class Workflow
    {
        /// <summary>
        /// Workflow state constants
        /// </summary>
        public static class States
        {
            public const string Completed = "completed";
        }
    }

}