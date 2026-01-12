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
    /// CoreHR module constants
    /// </summary>
    public static class CoreHr
    {
        /// <summary>
        /// News type codes used in the CoreHR module
        /// </summary>
        public static class NewsType
        {
            public const string Announcement = "ANNOUNCEMENT";
            public const string Meeting = "MEETING";
            public const string Reminder = "REMINDER";
            public const string Update = "UPDATE";
            public const string Event = "EVENT";
            public const string Policy = "POLICY";
            public const string Training = "TRAINING";
            public const string FuneralAnnouncement = "FUNERAL_ANNOUNCEMENT";
            public const string Wedding = "WEDDING";
            public const string Retirement = "RETIREMENT";
            public const string Birth = "BIRTH";
            public const string Circular = "CIRCULAR";
        }
    }

    /// <summary>
    /// General Ledger (GL) module constants
    /// </summary>
    public static class Gl
    {
        /// <summary>
        /// Default system account codes used across the GL module
        /// These codes are seeded as system accounts and should be used consistently throughout the application
        /// </summary>
        public static class DefaultAccounts
        {
            // Assets (10000-19999)
            public static class Assets
            {
                // Cash & Cash Equivalents (10100-10199)
                public const string CashAndCashEquivalents = "10100";
                public const string RegularCheckingAccount = "10111";
                public const string CashInHand = "10113";
                public const string UndepositedFunds = "10115";
                public const string PayrollClearingAccount = "10117";
                
                // Accounts Receivable (10120-10129)
                public const string AccountsReceivable = "10120";
                public const string AllowanceForDoubtfulAccounts = "10121";
                
                // Prepaid Expenses (10200-10299)
                public const string PrepaidExpenses = "10200";
                
                // Inventory (10800-10899)
                public const string Inventory = "10800";
                public const string GoodsReceivedClearingAccount = "10810";
                public const string StocksOfWorkInProgress = "10900";
                
                // Fixed Assets (15000-15999)
                public const string FixedAssets = "15000";
                public const string AccumulatedDepreciation = "15010";
            }

            // Liabilities (20000-29999)
            public static class Liabilities
            {
                // Accounts Payable (20100-20199)
                public const string AccountsPayable = "20110";
                public const string CustomerAdvances = "20120";
                public const string VendorAdvances = "20130";
                
                // Accrued Expenses (20200-20299)
                public const string AccruedExpenses = "20200";
                public const string AccruedPayroll = "20210";
                
                // Taxes Payable (20300-20399)
                public const string SalesTax = "20300";
                public const string EmployeeIncomeTaxWithholdingPayable = "20310";
                public const string EmployeeSocialSecurityWithholdingPayable = "20320";
                public const string EmployeeHealthInsuranceWithholdingPayable = "20330";
                public const string EmployerPayrollTaxPayable = "20340";
                
                // Notes Payable (20400-20499)
                public const string NotesPayable = "20400";
                
                // Unearned Revenue (20500-20599)
                public const string UnearnedRevenue = "20500";
            }

            // Equity (30000-39999)
            public static class Equity
            {
                public const string CommonStock = "30000";
                public const string OpeningBalanceEquity = "30010";
                public const string RetainedEarnings = "31000";
                public const string Dividends = "32000";
            }

            // Revenue (40000-49999)
            public static class Revenue
            {
                public const string Sales = "40100";
                public const string ServiceRevenue = "40200";
                public const string SalesDiscounts = "40400";
                public const string ShippingAndHandling = "40500";
                public const string OtherIncome = "40600";
                public const string InterestIncome = "40700";
            }

            // Expenses (50000-69999)
            public static class Expenses
            {
                // Cost of Goods Sold (50000-50999)
                public const string PurchaseAccount = "50200";
                public const string CostOfGoodsSold = "50300";
                public const string PurchaseDiscounts = "50400";
                public const string PurchasePriceVariance = "50500";
                public const string PurchaseTax = "50700";
                
                // Operating Expenses (60000-69999)
                public const string DepreciationExpense = "60100";
                public const string BadDebtExpense = "60200";
                public const string InterestExpense = "60300";
                public const string RentExpense = "60400";
                public const string UtilitiesExpense = "60500";
                
                // Payroll Expenses (60600-60699)
                public const string SalariesAndWagesExpense = "60600";
                public const string OvertimeExpense = "60610";
                public const string BonusesAndCommissionsExpense = "60620";
                
                // Benefits Expenses (60700-60799)
                public const string EmployeeBenefitsExpense = "60700";
                public const string HealthInsuranceExpense = "60710";
                public const string RetirementPlanContributionsExpense = "60720";
                public const string LifeInsuranceExpense = "60730";
                public const string DisabilityInsuranceExpense = "60740";
            }

            // System Accounts (90000-99999)
            public static class SystemAccounts
            {
                /// <summary>
                /// Currency revaluation gain account code
                /// Used for recording currency revaluation gains
                /// </summary>
                public const string CurrencyRevaluationGain = "99999";

                /// <summary>
                /// Currency revaluation loss account code
                /// Used for recording currency revaluation losses
                /// </summary>
                public const string CurrencyRevaluationLoss = "99998";
            }
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

        /// <summary>
        /// Utilities for vendor document folder paths
        /// </summary>
        public static class VendorDocuments
        {
            /// <summary>
            /// Gets the folder path for vendor documents
            /// </summary>
            /// <param name="vendorId">The vendor ID</param>
            /// <param name="documentType">The document type (optional)</param>
            /// <returns>Folder path for vendor documents</returns>
            public static string GetVendorDocumentsFolder(Guid vendorId, string? documentType = null)
            {
                var basePath = $"{Folders.Vendors}/{vendorId}";
                return documentType != null ? $"{basePath}/{documentType}" : basePath;
            }

            /// <summary>
            /// Gets the folder path for vendor documents with tenant support (for future use)
            /// </summary>
            /// <param name="vendorId">The vendor ID</param>
            /// <param name="documentType">The document type (optional)</param>
            /// <param name="tenantId">The tenant ID (optional, defaults to DefaultTenantId)</param>
            /// <returns>Folder path for vendor documents with tenant</returns>
            public static string GetVendorDocumentsFolderWithTenant(Guid vendorId, string? documentType = null,
                string? tenantId = null)
            {
                var tenant = tenantId ?? DefaultTenantId;
                var basePath = $"{tenant}/{Folders.Vendors}/{vendorId}";
                return documentType != null ? $"{basePath}/{documentType}" : basePath;
            }
        }

        /// <summary>
        /// Utilities for Learning module document folder paths
        /// </summary>
        public static class Learning
        {
            /// <summary>
            /// Gets the folder path for transcripts
            /// </summary>
            /// <param name="employeeId">The employee ID</param>
            /// <returns>Folder path for transcripts</returns>
            public static string GetTranscriptsFolder(Guid employeeId)
            {
                return $"qimerp/learning/transcripts/{employeeId}";
            }

            /// <summary>
            /// Gets the folder path for certifications
            /// </summary>
            /// <param name="employeeId">The employee ID</param>
            /// <returns>Folder path for certifications</returns>
            public static string GetCertificationsFolder(Guid employeeId)
            {
                return $"qimerp/learning/certifications/{employeeId}";
            }
        }

        /// <summary>
        /// Utilities for customer document folder paths
        /// </summary>
        public static class CustomerDocuments
        {
            /// <summary>
            /// Gets the folder path for customer documents
            /// </summary>
            /// <param name="customerId">The customer ID</param>
            /// <param name="documentType">The document type (optional)</param>
            /// <returns>Folder path for customer documents</returns>
            public static string GetCustomerDocumentsFolder(Guid customerId, string? documentType = null)
            {
                var basePath = $"{Folders.Customers}/{customerId}";
                return documentType != null ? $"{basePath}/{documentType}" : basePath;
            }

            /// <summary>
            /// Gets the folder path for customer documents with tenant support (for future use)
            /// </summary>
            /// <param name="customerId">The customer ID</param>
            /// <param name="documentType">The document type (optional)</param>
            /// <param name="tenantId">The tenant ID (optional, defaults to DefaultTenantId)</param>
            /// <returns>Folder path for customer documents with tenant</returns>
            public static string GetCustomerDocumentsFolderWithTenant(Guid customerId, string? documentType = null,
                string? tenantId = null)
            {
                var tenant = tenantId ?? DefaultTenantId;
                var basePath = $"{tenant}/{Folders.Customers}/{customerId}";
                return documentType != null ? $"{basePath}/{documentType}" : basePath;
            }
        }

        /// <summary>
        /// Utilities for company news image folder paths
        /// </summary>
        public static class News
        {
            /// <summary>
            /// Gets the folder path for news images
            /// </summary>
            /// <returns>Folder path for news images</returns>
            public static string GetNewsImagesFolder()
            {
                return "qimerp/news/images";
            }

            /// <summary>
            /// Gets the folder path for news images with tenant support (for future use)
            /// </summary>
            /// <param name="tenantId">The tenant ID (optional, defaults to DefaultTenantId)</param>
            /// <returns>Folder path for news images with tenant</returns>
            public static string GetNewsImagesFolderWithTenant(string? tenantId = null)
            {
                var tenant = tenantId ?? DefaultTenantId;
                return $"{tenant}/news/images";
            }

            /// <summary>
            /// Gets the folder path for news attachments
            /// </summary>
            /// <returns>Folder path for news attachments</returns>
            public static string GetNewsAttachmentsFolder()
            {
                return "qimerp/news/attachments";
            }

            /// <summary>
            /// Gets the folder path for news attachments with tenant support (for future use)
            /// </summary>
            /// <param name="tenantId">The tenant ID (optional, defaults to DefaultTenantId)</param>
            /// <returns>Folder path for news attachments with tenant</returns>
            public static string GetNewsAttachmentsFolderWithTenant(string? tenantId = null)
            {
                var tenant = tenantId ?? DefaultTenantId;
                return $"{tenant}/news/attachments";
            }
        }
    }

    public static class Api
    {
        public static class Tags
        {
            public const string Auth = "QimErp.Modules.Auth.WebApi";
            public const string Ap = "QimErp.Modules.Ap.WebApi";
            public const string Ar = "QimErp.Modules.Ar.WebApi";
            public const string Hr = "QimErp.Modules.Hr.WebApi";
            public const string Inventory = "QimErp.Modules.Inventory.WebApi";
            public const string HrRecruitment = "QimErp.Modules.Hr.Recruitment.WebApi";
            public const string HrSurveys = "QimErp.Modules.Hr.Surveys.WebApi";
            public const string HrEmployeeEngagement = "QimErp.Modules.Hr.EmployeeEngagement.WebApi";
            public const string HrPerformance = "QimErp.Modules.Hr.Performance.WebApi";
            public const string HrLearning = "QimErp.Modules.Hr.Learning.WebApi";
            public const string HrTalent = "QimErp.Modules.Hr.Talent.WebApi";
            public const string HrBenefit = "QimErp.Modules.Hr.Benefit.WebApi";
            public const string HrWorkforcePlanning = "QimErp.Modules.Hr.WorkforcePlanning.WebApi";
            public const string HrLeave = "QimErp.Modules.Leave.WebApi";
            public const string Payroll = "QimErp.Modules.Payroll.WebApi";
            public const string Gl = "QimErp.Modules.Gl.Core.WebApi";
            public const string CashManagement = "QimErp.Modules.CashManagement.WebApi";
            public const string Project = "QimErp.Modules.Project.WebApi";
            public static string Core => "QimErp.Modules.Core.WebApi";
        }

        public static class Url
        {
          

            public static class Hr
            {
                // Employee URLs
                public const string Employee = "/api/hr/employees";

                public const string QuickEmployee = "/api/hr/employee/quick";
                public const string EmployeeSearch = "/api/hr/employee/search";
                public const string EmployeeBulk = "/api/hr/employees/bulk";
                public const string EmployeeExport = "/api/hr/employee/export";
                public const string EmployeeTerminate = "/api/hr/employee/terminate";
                public const string EmployeesPage = "/api/hr/employees/page";
                public const string EmployeesExport = "/api/hr/employees/export";
                public const string EmployeesImport = "/api/hr/employees/import";

                // Organization URLs
                public const string OrganizationalUnits = "/api/hr/organizational-units";
                public const string OrganizationalUnitsPage = "/api/hr/organizational-units/page";
                public const string OrganizationalUnitsImport = "/api/hr/organizational-units/import";

                public const string JobTitles = "/api/hr/job-titles";
                public const string JobTitlesPage = "/api/hr/job-titles/page";
                public const string JobTitlesExport = "/api/hr/job-titles/export";
                public const string JobTitlesImport = "/api/hr/job-titles/import";

                public const string Stations = "/api/hr/stations";
                public const string StationsPage = "/api/hr/stations/page";
                public const string StationsImport = "/api/hr/stations/import";

                public const string Import = "/api/hr/import";

                public const string TestSignalRProgress = "/api/hr/test/signalr-progress";

                public const string EmployeeStatuses = "/api/hr/employee-statuses";
                public const string EmployeeStatusesPage = "/api/hr/employee-statuses/page";

                public const string JobStatuses = "/api/hr/job-statuses";
                public const string JobStatusesPage = "/api/hr/job-statuses/page";

                public const string EmployeeJobStatuses = "/api/hr/employee-job-statuses";
                public const string EmployeeJobStatusesPage = "/api/hr/employee-job-statuses/page";

                public const string LeaveTypes = "/api/hr/leave-types";
                
                public static class Recruitment
                {
                    public const string Dashboard = "/api/hr/recruitment/dashboard";
                    public const string Base = "/api/hr/recruitment";
                
                    // Candidates
                    public const string Candidates = $"{Base}/candidates";
                
                    // Jobs
                    public const string Jobs = $"{Base}/jobs";
                
                    // Applications
                    public const string Applications = $"{Base}/applications";
                
                    // Interviews
                    public const string Interviews = $"{Base}/interviews";
                
                    // Offers
                    public const string Offers = $"{Base}/offers";
                
                    // Hires
                    public const string Hires = $"{Base}/hires";
                
                    // Job Requisitions
                    public const string JobRequisitions = $"{Base}/job-requisitions";
                
                    // Scorecards
                    public const string Scorecards = $"{Base}/scorecards";
                
                    // Screening Rules
                    public const string ScreeningRules = $"{Base}/screening-rules";
                }
                
                public static class Surveys
                {
                    public const string Base = "/api/hr/surveys";
                    public const string Responses = $"{Base}/responses";
                    public const string Analytics = $"{Base}/analytics";
                }
                
                public static class Engagement
                {
                    public const string Base = "/api/hr/engagement";
                    public const string Risks = $"{Base}/risks";
                    public const string DisciplinaryCases = $"{Base}/disciplinary-cases";
                    public const string Messages = $"{Base}/messages";
                    public const string Recognitions = $"{Base}/recognitions";
                    public const string HealthIssues = $"{Base}/health-issues";
                    public const string CompliancePolicies = $"{Base}/compliance-policies";
                    public const string ComplianceAudits = $"{Base}/compliance-audits";
                    public const string Analytics = $"{Base}/analytics";
                }
                
                public static class Performance
                {
                    public const string Base = "/api/hr/performance";
                    public const string Reviews = $"{Base}/reviews";
                    public const string Goals = $"{Base}/goals";
                    public const string Competencies = $"{Base}/competencies";
                    public const string Feedback360 = $"{Base}/feedback-360";
                    public const string DevelopmentPlans = $"{Base}/development-plans";
                    public const string CheckIns = $"{Base}/check-ins";
                    public const string Calibrations = $"{Base}/calibrations";
                    public const string Conversations = $"{Base}/conversations";
                    public const string Templates = $"{Base}/templates";
                    public const string StrategicFramework = $"{Base}/strategic-framework";
                    public const string Analytics = $"{Base}/analytics";
                }
                
                public static class Learning
                {
                    public const string Base = "/api/hr/learning";
                    public const string Courses = $"{Base}/courses";
                    public const string Enrollments = $"{Base}/enrollments";
                    public const string Modules = $"{Base}/modules";
                    public const string Content = $"{Base}/content";
                    public const string Assessments = $"{Base}/assessments";
                    public const string Progress = $"{Base}/progress";
                    public const string Certificates = $"{Base}/certificates";
                    public const string Skills = $"{Base}/skills";
                    public const string LearningPaths = $"{Base}/learning-paths";
                    public const string Recommendations = $"{Base}/recommendations";
                    public const string Analytics = $"{Base}/analytics";
                    public const string Employees = $"{Base}/employees";
                }
                
                public static class Talent
                {
                    public const string Base = "/api/hr/talent";
                    public const string Pipeline = $"{Base}/pipeline";
                    public const string SuccessionPlans = $"{Base}/succession-plans";
                    public const string Reviews = $"{Base}/reviews";
                    public const string ReviewTemplates = $"{Base}/review-templates";
                    public const string Templates = $"{Base}/templates";
                    public const string Analytics = $"{Base}/analytics";
                }

                public static class Benefit
                {
                    public const string Base = "/api/hr/benefit";
                    public const string BenefitTypes = $"{Base}/types";
                    public const string BenefitPlans = $"{Base}/plans";
                    public const string Enrollments = $"{Base}/enrollments";
                    public const string Loans = $"{Base}/loans";
                    public const string Accommodations = $"{Base}/accommodations";
                    public const string HouseCategories = $"{Base}/house-categories";
                    public const string WaitingLists = $"{Base}/waiting-lists";
                    public const string EnrollmentConfiguration = $"{Base}/enrollment-configuration";
                    public const string Analytics = $"{Base}/analytics";
                }

                public static class WorkforcePlanning
                {
                    public const string Base = "/api/hr/workforce-planning";
                    public const string Forecasts = $"{Base}/forecasts";
                    public const string HeadcountPlans = $"{Base}/headcount-plans";
                    public const string SkillsGaps = $"{Base}/skills-gaps";
                    public const string Scenarios = $"{Base}/scenarios";
                    public const string Plans = $"{Base}/plans";
                    public const string Analytics = $"{Base}/analytics";
                }
                
                public const string LeaveTypesPage = "/api/hr/leave-types/page";

                // Leave Management URLs - Organized in nested class
                public static class Leave
                {
                    public const string Base = "/api/hr/leave";
                    
                    public const string LeaveRequests = $"{Base}/leave-requests";
                    public const string LeaveRequestsPage = $"{Base}/leave-requests/page";
                    public const string AllLeaveRequestsPage = $"{Base}/leave-requests/all/page";
                    public const string PendingLeaveRequests = $"{Base}/leave-requests/pending";
                    public const string RespondToLeaveRequest = $"{Base}/respond";
                    public const string SetupEmployeeLeave = $"{Base}/setup";

                    // Leave Planner URLs
                    public const string LeavePlannerRequests = $"{Base}/leave-planner";
                    public const string LeavePlannerRequestsPage = $"{Base}/leave-planner/page";

                    // Holidays URLs
                    public const string Holidays = $"{Base}/holidays";
                    public const string HolidaysPage = $"{Base}/holidays/page";

                    // Leave Recall URLs
                    public const string LeaveRecalls = $"{Base}/leave-recall";
                    public const string LeaveRecallsPage = $"{Base}/leave-recall/page";

                    // TimeOff URLs
                    public const string TimeOffBase = $"{Base}/timeoff";
                    public const string TimeOffBalance = $"{Base}/timeoff/balance";
                    public const string TimeOffHistory = $"{Base}/timeoff/history";
                    public const string TimeOffRequests = $"{Base}/timeoff/requests";
                    public const string TimeOffMyRequests = $"{Base}/timeoff/my-requests";
                    public const string TimeOffUpcoming = $"{Base}/timeoff/upcoming";
                    public const string TimeOffCalculate = $"{Base}/timeoff/calculate";
                    public const string TimeOffDashboardStats = $"{Base}/timeoff/dashboard-stats";
                    public const string TimeOffRequest = $"{Base}/timeoff/request";
                    public const string TimeOffWorkflowHistory = $"{Base}/timeoff/{{requestId}}/workflow-history";

                    // Leave Management URLs
                    public const string LeaveManagementBase = $"{Base}/leave-management";
                    public const string LeaveManagementDashboard = $"{Base}/leave-management/dashboard";
                    public const string LeaveManagementStatistics = $"{Base}/leave-management/statistics";
                    public const string LeaveManagementEmployeesOnLeave = $"{Base}/leave-management/employees-on-leave";
                    public const string LeaveManagementPendingApprovals = $"{Base}/leave-management/pending-approvals";
                    public const string LeaveManagementUpcomingLeaves = $"{Base}/leave-management/upcoming-leaves";

                    // Admin/Configuration URLs
                    public const string ConfigureEmployeeLeave = $"{Base}/timeoff/configure-employee-leave";
                    public const string EmployeesLeaves = $"{Base}/employees/leaves";
                    
                    // Leave Admin URLs
                    public const string LeaveAdminDashboard = $"{Base}/leave-admin/dashboard";
                }

                // Admin URLs
                public const string Dashboard = "/api/hr/dashboard";
                public const string News = "/api/hr/news";
                public const string Contributions = "/api/hr/contributions";
                public const string AdminDashboard = "/api/hr/admin/dashboard";
                public const string AdminActivities = "/api/hr/admin/activities";
                public const string AdminLeaveRequests = "/api/hr/admin/leave-requests";
                public const string AdminLeaveRequestsApprove = "/api/hr/admin/leave-requests/{id}/approve";
                public const string AdminLeaveRequestsReject = "/api/hr/admin/leave-requests/{id}/reject";
                public const string AdminEmployeeLeaveBalance = "/api/hr/admin/employees/{employeeId}/leave-balance";
                public const string AdminDataConsistencyValidate = "/api/hr/admin/data-consistency/validate";
                public const string AdminDataConsistencyRepairEmployee = "/api/hr/admin/data-consistency/repair/employee";
                public const string AdminDataConsistencyRepairOrganizationalUnit = "/api/hr/admin/data-consistency/repair/organizational-unit";
                public const string AdminDataConsistencyStatistics = "/api/hr/admin/data-consistency/statistics";
            }

            public static class Payroll
            {
                public const string Base = "/api/payroll";
                public const string PayrollRuns = $"{Base}/payroll-runs";
                public const string PayrollItems = $"{Base}/payroll-items";
                public const string SalaryStructures = $"{Base}/salary-structures";
                public const string Allowances = $"{Base}/allowances";
                public const string Grades = $"{Base}/grades";
                public const string Deductions = $"{Base}/deductions";
                public const string Loans = $"{Base}/loans";
                public const string Advances = $"{Base}/advances";
                public const string Payslips = $"{Base}/payslips";
                public const string Tax = $"{Base}/tax";
                public const string SSNIT = $"{Base}/ssnit";
                public const string ProvidentFund = $"{Base}/provident-fund";
                public const string ThirdTier = $"{Base}/third-tier";
                public const string Arrears = $"{Base}/arrears";
                public const string Exemptions = $"{Base}/exemptions";
                public const string Suspensions = $"{Base}/suspensions";
                public const string Claims = $"{Base}/claims";
                public const string Bonuses = $"{Base}/bonuses";
                public const string Insurance = $"{Base}/insurance";
                public const string GradeNotch = $"{Base}/grade-notch";
                public const string Payments = $"{Base}/payments";
                public const string Reports = $"{Base}/reports";

                public static class Dashboard
                {
                    public const string Base = $"{Payroll.Base}/dashboard";
                    public const string Stats = $"{Base}/stats";
                    public const string MySummary = $"{Base}/my-summary";
                    public const string Upcoming = $"{Base}/upcoming";
                    public const string MyPayslips = $"{Base}/my-payslips";
                    public const string MyLoans = $"{Base}/my-loans";
                    public const string MyAdvances = $"{Base}/my-advances";
                    public const string MyClaims = $"{Base}/my-claims";
                    public const string Admin = $"{Base}/admin";
                    public const string AdminStatistics = $"{Base}/admin/statistics";
                    public const string AdminDistributionAnalysis = $"{Base}/admin/distribution-analysis";
                }
            }

           

            public static class Ar
            {
                public const string QuickCustomer = "/api/ar/customer/quick";
                public const string Customer = "/api/ar/customers";
                public const string CustomerUpload = "/api/ar/customer/upload";
                public const string Invoice = "/api/ar/invoices";
                public const string InvoicePay = "/api/ar/invoices/pay";
                public const string SaleOrder = "api/ar/orders";
            }

            public static class Ap
            {
                public const string Vendor = "api/ap/vendors";
                public const string VendorUpload = "api/ap/vendors/upload";
            }

            public static class Inventory
            {
                public const string QuantityAdjustment = "/api/inventory/quantity-adjustments";
                public const string Product = "/api/inventory/products";
                public const string WareHouse = "/api/inventory/warehouses";
                public const string PriceAdjustment = "/api/inventory/adjust-price";
                public const string StockTransfer = "/api/inventory/stock-transfers";
            }

            public static class Gl
            {
                public const string Base = "/api/gl";
                public const string ChartOfAccounts = $"{Base}/chart-of-accounts";
                public const string AccountCategories = $"{Base}/account-categories";
                public const string JournalEntries = $"{Base}/journal-entries";
                public const string FiscalYears = $"{Base}/fiscal-years";
                public const string FiscalPeriods = $"{Base}/fiscal-periods";
                public const string AccountBalances = $"{Base}/account-balances";
                public const string Currencies = $"{Base}/currencies";
                public const string ExchangeRates = $"{Base}/exchange-rates";
                public const string CurrencyRevaluations = $"{Base}/currency-revaluations";
                public const string CostCenters = $"{Base}/cost-centers";
                public const string Reports = $"{Base}/reports";
                
                public static class BudgetPlanning
                {
                    public const string Base = $"{Gl.Base}/budgets";
                    public const string Budgets = Base;
                    public const string BudgetLines = $"{Base}/{{budgetId}}/lines";
                    public const string BudgetVersions = $"{Base}/{{budgetId}}/versions";
                    public const string BudgetRevisions = $"{Base}/{{budgetId}}/revisions";
                    public const string BudgetActuals = $"{Base}/{{budgetId}}/actuals";
                    public const string BudgetVariance = $"{Base}/{{budgetId}}/variance";
                    public const string SyncActuals = $"{Base}/{{budgetId}}/sync-actuals";
                    public const string ReportsSummary = $"{Base}/reports/summary";
                    public const string ReportsVariance = $"{Base}/reports/variance";
                    public const string ReportsByCostCenter = $"{Base}/reports/by-cost-center";
                    public const string ReportsByDepartment = $"{Base}/reports/by-department";
                    public const string BudgetTemplates = $"{Base}/templates";
                    public const string BudgetConsolidations = $"{Base}/consolidations";
                    public const string BudgetAllocationRules = $"{Base}/allocation-rules";
                }
            }

            public static class CashManagement
            {
                public const string Base = "/api/cash-management";
                public const string BankAccounts = $"{Base}/bank-accounts";
                public const string BankReconciliation = $"{Base}/bank-reconciliation";
                public const string CashForecast = $"{Base}/cash-forecast";
                public const string Payments = $"{Base}/payments";
                public const string BankTransfers = $"{Base}/bank-transfers";
                public const string PettyCash = $"{Base}/petty-cash";
                public const string Checks = $"{Base}/checks";
                public const string WireTransfers = $"{Base}/wire-transfers";
            }

            public static class Project
            {
                public const string Base = "/api/project";
                public const string Projects = $"{Base}/projects";
            }

            public static class Core
            {
                public const string PaymentTerms = "api/core/payment-terms";
                public const string Tax = "api/core/vendors/upload";
            }


           
        }
    }

    /// <summary>
    /// Cache-related constants for Redis caching strategy
    /// </summary>
    public static class Cache
    {
            /// <summary>
            /// Prefix for all cache keys in Redis
            /// </summary>
        private const string CacheKeyPrefix = "qface:qimerp:";

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

            /// <summary>
            /// Cache key templates with tenant support
            /// </summary>
        public static class Keys
        {
            // Auth Module Keys
            public static string CurrentUser(string tenantId, string userId) =>
            $"{CacheKeyPrefix}{tenantId}:auth:current_user_{userId}";

            public static string CompanyInfo(string tenantId) =>
            $"{CacheKeyPrefix}{tenantId}:auth:company_info";

            public static string ActivationToken(string email) =>
            $"{CacheKeyPrefix}auth:activation_token_{email}";

            // HR Module Keys
            public static string EmployeeByEmail(string tenantId, string email) =>
            $"{CacheKeyPrefix}{tenantId}:hr:employee_by_email_{email}";

            public static string Employee(string tenantId, Guid employeeId) =>
            $"{CacheKeyPrefix}{tenantId}:hr:employee_{employeeId}";

            public static string PeoplePage(string tenantId, int page, int size, string? search,
                string? filter) =>
            $"{CacheKeyPrefix}{tenantId}:hr:people_page_{page}_{size}_{search ?? "null"}_{filter ?? "null"}";

            public static string PeopleFilters(string tenantId) => $"{CacheKeyPrefix}{tenantId}:hr:people_filters";

            public static string EmployeeSimpleList(string tenantId, int top) =>
            $"{CacheKeyPrefix}{tenantId}:hr:employee_simple_list_{top}";

            // Dashboard Keys
            public static string MyTeam(string tenantId, string email) => $"{CacheKeyPrefix}{tenantId}:hr:my_team_{email}";

            public static string MyTimeOff(string tenantId, string email) =>
            $"{CacheKeyPrefix}{tenantId}:hr:my_timeoff_{email}";

            public static string MyOnboardingProgress(string tenantId, string email) =>
            $"{CacheKeyPrefix}{tenantId}:hr:my_onboarding_progress_{email}";

            // News Keys
            public static string UnreadNewsCount(string tenantId, string email) =>
            $"{CacheKeyPrefix}{tenantId}:hr:unread_news_count_{email}";

            public static string NewsForEmployee(string tenantId, string email) =>
            $"{CacheKeyPrefix}{tenantId}:hr:news_for_employee_{email}";

            // Dashboard Keys
            public static string Celebrations(string tenantId) => $"{CacheKeyPrefix}{tenantId}:hr:celebrations";

            // Leave Management Keys
            public static string TimeOffBalance(string employeeId, int year) =>
            $"{CacheKeyPrefix}hr:time_off_balance_{employeeId}_{year}";

            public static string TimeOffHistory(string employeeId, string? leaveTypeId) =>
            $"{CacheKeyPrefix}hr:time_off_history_{employeeId}_{leaveTypeId ?? "all"}";

            public static string UsedDays(string employeeId, string leaveTypeId, int year) =>
            $"{CacheKeyPrefix}hr:used_days_{employeeId}_{leaveTypeId}_{year}";

            public static string PendingDays(string employeeId, string leaveTypeId, int year) =>
            $"{CacheKeyPrefix}hr:pending_days_{employeeId}_{leaveTypeId}_{year}";

            // HR Recruitment Dashboard Keys
            public static string PendingApprovals(string tenantId, int limit) =>
            $"{CacheKeyPrefix}{tenantId}:hr:recruitment:pending_approvals_{limit}";

            public static string RecentActivity(string tenantId, int limit) =>
            $"{CacheKeyPrefix}{tenantId}:hr:recruitment:recent_activity_{limit}";

            public static string RecruitmentPipeline(string tenantId) =>
            $"{CacheKeyPrefix}{tenantId}:hr:recruitment:pipeline";

            public static string RecruitmentStats(string tenantId) =>
            $"{CacheKeyPrefix}{tenantId}:hr:recruitment:stats";

            public static string UpcomingInterviews(string tenantId, int daysAhead) =>
            $"{CacheKeyPrefix}{tenantId}:hr:recruitment:upcoming_interviews_{daysAhead}";

            // Pattern-based keys for invalidation
            public static string PeoplePagePattern(string tenantId) => $"{CacheKeyPrefix}{tenantId}:hr:people_page_*";
            public static string NewsPattern(string tenantId) => $"{CacheKeyPrefix}{tenantId}:hr:news_*";

            public static string UnreadNewsCountPattern(string tenantId) =>
            $"{CacheKeyPrefix}{tenantId}:hr:unread_news_count_*";

            // Leave Management Pattern-based keys for invalidation
            public static string TimeOffBalancePattern(string employeeId) =>
            $"{CacheKeyPrefix}hr:time_off_balance_{employeeId}_*";

            public static string TimeOffHistoryPattern(string employeeId) =>
            $"{CacheKeyPrefix}hr:time_off_history_{employeeId}_*";

            public static string UsedDaysPattern(string employeeId) => $"{CacheKeyPrefix}hr:used_days_{employeeId}_*";
            public static string PendingDaysPattern(string employeeId) => $"{CacheKeyPrefix}hr:pending_days_{employeeId}_*";

            // Import Keys
            public static string Import(string tenantId, Guid importId) =>
            $"{CacheKeyPrefix}{tenantId}:hr:import_{importId}";

            // Workflow Module Keys
            public static string WorkflowConfiguration(string module, string entityType) =>
                $"{CacheKeyPrefix}workflow:config_{module}_{entityType}";

            public static string WorkflowTemplate(string templateId) =>
            $"{CacheKeyPrefix}workflow:template_{templateId}";

            public static string WorkflowTemplateByCode(string workflowCode) =>
            $"{CacheKeyPrefix}workflow:template_code_{workflowCode}";

            public static string WorkflowTemplatesByCategory(string category) =>
            $"{CacheKeyPrefix}workflow:templates_category_{category}";

            public static string WorkflowTemplatesPage(int pageNumber, int pageSize, string category, string search, string status) =>
            $"{CacheKeyPrefix}workflow:templates_page_{pageNumber}_{pageSize}_{category ?? "all"}_{search ?? "null"}_{status ?? "all"}";

            public static string PendingApprovals(string? entityType, string? approverId, int pageNumber) =>
            $"{CacheKeyPrefix}workflow:pending_{entityType ?? "all"}_{approverId ?? "all"}_page_{pageNumber}";

            public static string SearchWorkflows(string hash) =>
            $"{CacheKeyPrefix}workflow:search_{hash}";

            public static string WorkflowStats(string userId) =>
            $"{CacheKeyPrefix}workflow:stats_{userId}";

            public static string WorkflowStatsByHash(string hash) =>
            $"{CacheKeyPrefix}workflow:stats_{hash}";

            public static string WorkflowDashboard(string userId) =>
            $"{CacheKeyPrefix}workflow:dashboard_{userId}";

            public static string WorkflowHistory(string entityId, string entityType) =>
            $"{CacheKeyPrefix}workflow:history_{entityType}_{entityId}";

            // Pattern-based keys for invalidation
            public static string WorkflowTemplatePattern() => $"{CacheKeyPrefix}workflow:template_*";
            public static string WorkflowTemplatesPagePattern() => $"{CacheKeyPrefix}workflow:templates_page_*";
            public static string PendingApprovalsPattern() => $"{CacheKeyPrefix}workflow:pending_*";
            public static string SearchWorkflowsPattern() => $"{CacheKeyPrefix}workflow:search_*";
            public static string WorkflowStatsPattern() => $"{CacheKeyPrefix}workflow:stats_*";
            public static string WorkflowDashboardPattern() => $"{CacheKeyPrefix}workflow:dashboard_*";
            public static string WorkflowHistoryPattern(string entityType) => $"{CacheKeyPrefix}workflow:history_{entityType}_*";
            }

            /// <summary>
            /// Cache TTL (Time To Live) constants in minutes
            /// </summary>
        public static class Ttl
        {
            // Frequently changing data (2-5 minutes)
            public const int UnreadNewsCount = 2;
            public const int NewsForEmployee = 3;
            public const int EmployeeByEmail = 5;
            public const int MyTeam = 5;

            // Semi-static data (10-15 minutes)
            public const int CurrentUser = 10;
            public const int MyTimeOff = 10;
            public const int PeoplePage = 10;
            public const int Employee = 15;
            public const int EmployeeSimpleList = 10;
            public const int MyOnboardingProgress = 15;

            // Auth Module TTL
            public const int ActivationToken = 2880; // 48 hours for activation tokens

            // Static data (1 hour+)
            public const int PeopleFilters = 60;
            public const int Celebrations = 60;

            // Leave Management TTL
            public const int TimeOffBalance = 15;
            public const int TimeOffHistory = 30;
            public const int UsedDays = 10;
            public const int PendingDays = 5;

            // Import TTL
            public const int Import = 5;

            // Workflow Module TTL
            public const int WorkflowConfiguration = 1440; // 24 hours for workflow configurations
            public const int WorkflowTemplate = 60; // 1 hour for templates
            public const int PendingApprovals = 5; // 5 minutes for pending approvals
            public const int SearchWorkflows = 2; // 2 minutes for search results
            public const int WorkflowStats = 10; // 10 minutes for stats
            public const int WorkflowDashboard = 10; // 10 minutes for dashboard
            public const int WorkflowHistory = 15; // 15 minutes for history
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

    /// <summary>
    /// Benefit module constants
    /// </summary>
    public static class Benefit
    {
        /// <summary>
        /// Accommodation vacate prompt configuration
        /// </summary>
        public static class VacatePrompt
        {
            public static TimeSpan CheckInterval => TimeSpan.FromHours(24);
            public static string CheckTime => "09:00"; // HH:mm format
            public static TimeSpan NotificationCooldownPeriod => TimeSpan.FromDays(7);
            public static int CriticalVacatePromptDays => 30;
        }
    }
}