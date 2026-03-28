namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Registration marker for a module's IModuleApprovalActivity implementation.
/// Registered by <c>AddModuleApprovalActivity</c>; used to build the module approval activity registry at startup.
/// </summary>
public sealed record ModuleApprovalActivityRegistration(
    string[] EntityTypes,
    Type ActivityType);
