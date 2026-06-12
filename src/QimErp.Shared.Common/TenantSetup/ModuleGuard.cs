namespace QimErp.Shared.Common.TenantSetup;

/// <summary>
/// Guards tenant seeding steps behind module selection.
/// A null or empty SelectedModules list means "all modules" (legacy / full-suite tenant).
/// </summary>
public static class ModuleGuard
{
    public static bool IsSelected(IReadOnlyList<string>? selectedModules, string moduleKey)
    {
        if (selectedModules is null || selectedModules.Count == 0)
            return true;
        return selectedModules.Contains(moduleKey, StringComparer.OrdinalIgnoreCase);
    }
}
