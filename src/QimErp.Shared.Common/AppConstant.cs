namespace QimErp.Shared.Common;

public static class AppConstant
{
    public const string DefaultTenantId = "default";
    public const string DevTenantId = "hubtel";

    /// <summary>
    /// Constants and utilities for blob storage operations.
    /// Employee document path helpers live here because they are referenced by both
    /// QimErp.CoreHr (upload/profile picture) and QimErp.HROperations.EmployeeEngagement
    /// (disciplinary case documents). Everything else has been moved to the module that owns it:
    ///   - Learning storage  → LearningStorageConstants (QimErp.CoreHr.Learning.Shared.Constants)
    ///   - News storage      → SharedConstant.BlobStorage (QimErp.CoreHr.Employee.Shared.Constants)
    ///   - Taxation constants → TaxationConstants (QimErp.Shared.Common.Processors — internal)
    ///   - Workflow states   → WorkflowConstants (QimErp.Shared.Common.Services.Workflow — internal)
    ///   - Cache regions     → SharedConstants.Cache (QimErp.IAM.Shared)
    ///   - Payroll lookups   → SharedConstant (QimErp.Payroll.Core.Shared.Constants)
    ///   - HR API tags/URLs  → SharedConstant (QimErp.CoreHr.Employee.Shared.Constants)
    ///   - Surveys constants → SurveysConstants (QimErp.HrOperations.Surveys.Shared.Constants)
    ///   - Engagement const. → EngagementConstants (QimErp.HrOperations.EmployeeEngagement.Shared.Constants)
    ///   - Recruitment const.→ SharedConstant (QimErp.HrOperations.Recruitment.Shared.Constants)
    /// </summary>
    public static class BlobStorage
    {
        /// <summary>
        /// Folder name constants for blob storage organisation
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
        /// Utilities for employee document folder paths.
        /// Used by both CoreHr employee management and HROperations disciplinary case uploads.
        /// </summary>
        public static class EmployeeDocuments
        {
            /// <summary>
            /// Gets the folder path for employee documents.
            /// </summary>
            public static string GetEmployeeDocumentsFolder(Guid employeeId, string? documentType = null)
            {
                var basePath = $"qimerp/{Folders.Employees}/{employeeId}";
                return documentType != null ? $"{basePath}/{documentType}" : basePath;
            }

            /// <summary>
            /// Gets the folder path for employee documents with tenant support.
            /// </summary>
            public static string GetEmployeeDocumentsFolderWithTenant(Guid employeeId, string? documentType = null,
                string? tenantId = null)
            {
                var tenant = tenantId ?? DefaultTenantId;
                var basePath = $"{tenant}/{Folders.Employees}/{employeeId}";
                return documentType != null ? $"{basePath}/{documentType}" : basePath;
            }
        }
    }
}
