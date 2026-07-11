using QimErp.Shared.Common.TenantSetup;

namespace QimErp.Shared.Common.Sync;

public static class ModuleSyncActivityGuard
{
    public static bool ShouldProcess(string? moduleKey, IReadOnlyList<string>? selectedModules)
    {
        if (string.IsNullOrEmpty(moduleKey))
            return true;

        return ModuleGuard.IsSelected(selectedModules, moduleKey);
    }
}
