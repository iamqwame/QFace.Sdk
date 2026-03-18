namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Registration marker for a module's IModuleApprovalActivity implementation.
/// Registered by AddModuleApprovalActivity; consumed by the Platform Workflow Worker
/// to populate IModuleApprovalActivityRegistry on startup.
/// </summary>
public sealed record ModuleApprovalActivityRegistration(
    string[] EntityTypes,
    Type ActivityType);
