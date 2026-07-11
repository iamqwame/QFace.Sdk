namespace QimErp.Shared.Common.TenantSetup;

/// <summary>Filters unknown module tokens, always unions <see cref="BaseModel.IncludedModuleKeys"/>, deduplicates.</summary>
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
    };

    public static IReadOnlyList<string> Resolve(IReadOnlyList<string>? selectedModules)
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

        return result.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static IReadOnlyList<string> ResolveFromCsv(string? selectedModulesCsv)
    {
        if (string.IsNullOrWhiteSpace(selectedModulesCsv))
            return Resolve([]);

        var tokens = selectedModulesCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        return Resolve(tokens);
    }

    /// <summary>Persist explicit module keys only (base model keys always included).</summary>
    public static string NormalizeForPersistence(IReadOnlyList<string>? selectedModules)
        => string.Join(",", Resolve(selectedModules));

    public static string NormalizeForPersistence(string? selectedModulesCsv)
        => NormalizeForPersistence(
            string.IsNullOrWhiteSpace(selectedModulesCsv)
                ? []
                : selectedModulesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList());
}
