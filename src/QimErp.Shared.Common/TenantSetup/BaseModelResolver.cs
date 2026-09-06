using QimErp.Shared.Common.Sync;

namespace QimErp.Shared.Common.TenantSetup;

/// <summary>
/// Filters unknown module tokens, always unions <see cref="BaseModel.IncludedModuleKeys"/>, deduplicates.
/// <see cref="Resolve"/> also adds transitive <see cref="ModuleSyncRegistry"/> prerequisites and drives
/// sync fan-out; <see cref="ResolveExplicit"/> omits them and drives HTTP entitlement, which must stay
/// billed-modules-only.
/// </summary>
public static class BaseModelResolver
{
    private static readonly HashSet<string> KnownModuleKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ModuleKeys.CoreHR,
        ModuleKeys.Payroll,
        ModuleKeys.Leave,
        ModuleKeys.Recruitment,
        ModuleKeys.Benefits,
        ModuleKeys.Surveys,
        ModuleKeys.EmployeeEngagement,
        ModuleKeys.Learning,
        ModuleKeys.Performance,
        ModuleKeys.Talent,
        ModuleKeys.WorkforcePlanning,
        ModuleKeys.Workflow,
        ModuleKeys.CoreAccounting,
        ModuleKeys.AccountsPayable,
        ModuleKeys.AccountsReceivable,
        ModuleKeys.BudgetPlanning,
        ModuleKeys.CashManagement,
        ModuleKeys.Inventory,
        ModuleKeys.Project,
        ModuleKeys.POS,
        ModuleKeys.Reporting,
    };

    public static IReadOnlyList<string> Resolve(IReadOnlyList<string>? selectedModules)
    {
        var result = CollectExplicit(selectedModules);
        ExpandPrerequisites(result);
        return Sorted(result);
    }

    public static IReadOnlyList<string> ResolveFromCsv(string? selectedModulesCsv)
        => Resolve(SplitCsv(selectedModulesCsv));

    public static IReadOnlyList<string> ResolveExplicit(IReadOnlyList<string>? selectedModules)
        => Sorted(CollectExplicit(selectedModules));

    public static IReadOnlyList<string> ResolveExplicitFromCsv(string? selectedModulesCsv)
        => ResolveExplicit(SplitCsv(selectedModulesCsv));

    /// <summary>Persist explicit module keys only — derived prerequisites are re-expanded on every read.</summary>
    public static string NormalizeForPersistence(IReadOnlyList<string>? selectedModules)
        => string.Join(",", Sorted(CollectExplicit(selectedModules)));

    public static string NormalizeForPersistence(string? selectedModulesCsv)
        => NormalizeForPersistence(SplitCsv(selectedModulesCsv));

    private static HashSet<string> CollectExplicit(IReadOnlyList<string>? selectedModules)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (selectedModules is not null)
        {
            foreach (var token in selectedModules)
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                var trimmed = token.Trim();
                if (KnownModuleKeys.Contains(trimmed))
                    result.Add(trimmed);
            }
        }

        foreach (var key in BaseModel.IncludedModuleKeys)
            result.Add(key);

        return result;
    }

    private static void ExpandPrerequisites(HashSet<string> resolved)
    {
        var pending = new Queue<string>(resolved);

        while (pending.Count > 0)
        {
            var itemKey = ModuleSyncRegistry.TryResolveItemKey(pending.Dequeue());
            if (itemKey is null)
                continue;

            foreach (var prerequisite in ModuleSyncRegistry.ResolvePrerequisites(itemKey))
            {
                if (!KnownModuleKeys.Contains(prerequisite.ModuleKey))
                    continue;

                if (resolved.Add(prerequisite.ModuleKey))
                    pending.Enqueue(prerequisite.ModuleKey);
            }
        }
    }

    private static List<string> SplitCsv(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? []
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static IReadOnlyList<string> Sorted(HashSet<string> keys) =>
        keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();
}
