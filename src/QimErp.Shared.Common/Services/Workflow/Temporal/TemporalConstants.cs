namespace QimErp.Shared.Common.Services.Workflow.Temporal;

public static class TemporalConstants
{
    /// <summary>Single task queue used by QimErp's Temporal worker.</summary>
    public const string TaskQueue = "qimerp-workflow-approvals";

    /// <summary>Temporal namespace — overridable via Temporal:Namespace config key.</summary>
    public const string DefaultNamespace = "qimerp";
}
