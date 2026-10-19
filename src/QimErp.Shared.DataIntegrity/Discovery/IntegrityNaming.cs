using System.Text.RegularExpressions;

namespace QimErp.Shared.DataIntegrity.Discovery;

internal static partial class IntegrityNaming
{
    public const int MaxModuleKeyLength = 40;
    public const string WorkflowType = "DataIntegrityWorkflow";
    public const string ActivityName = "RunIntegrityChecks:DataIntegrity";
    public const string CustomContextKey = "custom";

    [GeneratedRegex("^[a-z][a-z0-9]*(-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex ModuleKeyRegex();

    [GeneratedRegex("(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.CultureInvariant)]
    private static partial Regex WordBoundaryRegex();

    [GeneratedRegex("_{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex RepeatedUnderscoreRegex();

    public static bool IsValidModuleKey(string moduleKey)
        => moduleKey.Length <= MaxModuleKeyLength && ModuleKeyRegex().IsMatch(moduleKey);

    public static string TaskQueue(string moduleKey) => $"qimerp-{moduleKey}-data-integrity";

    public static string ScheduleId(string moduleKey, string suffix) => $"qimerp-{moduleKey}-data-integrity-nightly{suffix}";

    public static string WorkflowId(string moduleKey, string suffix) => $"data-integrity-{moduleKey}{suffix}";

    public static IReadOnlyList<string> Words(string name)
        => WordBoundaryRegex().Split(name).Where(w => w.Length > 0).ToArray();

    public static string Snake(string name)
    {
        var snake = WordBoundaryRegex().Replace(name, "_").ToLowerInvariant();
        return RepeatedUnderscoreRegex().Replace(snake, "_");
    }

    public static string ContextKey(Type contextType)
    {
        var name = contextType.Name;
        foreach (var suffix in new[] { "ApplicationDbContext", "DbContext" })
        {
            if (name.Length > suffix.Length && name.EndsWith(suffix, StringComparison.Ordinal))
            {
                name = name[..^suffix.Length];
                break;
            }
        }

        return Snake(name);
    }
}
