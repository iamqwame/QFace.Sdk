namespace QimErp.Shared.Common;

/// <summary>
/// Constants for email template IDs used with ITemplateService.RenderEmailTemplateAsync.
/// Values must match S3 keys under templates/emails (e.g. WorkflowApproval.html).
/// </summary>
public static class EmailTemplateIds
{
    public static class Workflow
    {
        public const string Approval = "WorkflowApproval";
        public const string Rejection = "WorkflowRejection";
        public const string Started = "WorkflowStarted";
        public const string Completion = "WorkflowCompletion";
        public const string Timeout = "WorkflowTimeout";
        public const string StepApproved = "WorkflowStepApproved";
    }

    public static class Iam
    {
        public const string AccountActivation = "AccountActivation";
        public const string RegistrationWelcome = "RegistrationWelcome";
        public const string EmployeeAccountActivation = "EmployeeAccountActivation";
        public const string AdminPasswordReset = "AdminPasswordReset";
        public const string ForgotPassword = "ForgotPassword";
        public const string PasswordResetConfirmation = "PasswordResetConfirmation";
        public const string LoginSuccess = "LoginSuccess";
    }
}
