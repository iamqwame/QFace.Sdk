namespace QimErp.Shared.Common.TenantSetup;

/// <summary>
/// Guards tenant seeding steps behind module selection.
/// Null or empty selections resolve to the base model only via <see cref="BaseModelResolver"/>.
/// </summary>
public static class ModuleGuard
{
    public static bool IsSelected(IReadOnlyList<string>? selectedModules, string moduleKey)
    {
        var resolved = BaseModelResolver.Resolve(selectedModules ?? []);
        return resolved.Contains(moduleKey, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsSelectedFromCsv(string? selectedModulesCsv, string moduleKey)
    {
        var resolved = BaseModelResolver.ResolveFromCsv(selectedModulesCsv);
        return resolved.Contains(moduleKey, StringComparer.OrdinalIgnoreCase);
    }
}
