namespace QimErp.Shared.Common.Sync;

/// <summary>
/// Canonical App Store module wiring: catalog slug, module key, setup steps, and install backfill steps.
/// </summary>
public sealed record ModuleSyncDefinition(
    string ItemKey,
    string ModuleKey,
    string[] SetupSteps,
    string? RequiresItemKey = null,
    string[]? PrerequisiteItemKeys = null,
    string? EmployeeBackfillStep = null,
    string? AdminDataBackfillStep = null,
    bool IsPlugin = false,
    string? DisableStep = null);

public sealed record SetupStepRoute(string StepName, string TaskQueue);
