using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QimErp.Shared.DataIntegrity.Discovery;
using QimErp.Shared.DataIntegrity.Reporting;
using QimErp.Shared.DataIntegrity.Sql;

namespace QimErp.Shared.DataIntegrity;

public static class DataIntegrityHostExtensions
{
    private const string PrintLinksArgument = "--data-integrity-links";
    private const string PrintSqlArgument = "--sql";

    /// <summary>
    /// True when "--data-integrity-links" is in args or DataIntegrity:PrintLinks=true; "--sql" also prints generated SQL.
    /// Writes Markdown to stdout. No DB or Temporal access. Sets Environment.ExitCode = 1 on configuration errors.
    /// </summary>
    public static bool RunDataIntegrityLinkReportIfRequested(this IHost host, string[] args)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(args);

        var configuration = host.Services.GetService<IConfiguration>();
        var requested = args.Contains(PrintLinksArgument, StringComparer.Ordinal)
                        || (bool.TryParse(configuration?[$"{DataIntegrityOptions.SectionName}:{nameof(DataIntegrityOptions.PrintLinks)}"], out var print) && print);
        if (!requested)
            return false;

        var includeSql = args.Contains(PrintSqlArgument, StringComparer.Ordinal);
        var contextSet = host.Services.GetService<DataIntegrityContextSet>();
        if (contextSet is null)
        {
            Console.Error.WriteLine("Data integrity checks are not registered: call AddDataIntegrityChecks before building the host.");
            Environment.ExitCode = 1;
            return true;
        }

        var catalog = host.Services.GetRequiredService<IntegrityLinkCatalog>();
        var reports = new List<IntegrityContextReport>();
        var problems = new List<string>();

        foreach (var registration in contextSet.Contexts)
        {
            using var scope = host.Services.CreateScope();
            if (scope.ServiceProvider.GetService(registration.ContextType) is not DbContext context)
            {
                problems.Add($"{registration.ContextType.Name} is not registered in the service provider.");
                continue;
            }

            if (!context.Database.IsRelational())
            {
                reports.Add(new IntegrityContextReport(registration.ContextType.Name, context.Database.ProviderName ?? "unknown", null, null));
                continue;
            }

            try
            {
                var result = catalog.Get(registration, context);
                problems.AddRange(result.UnusedOverrides.Select(o => o.Problem));
                var sql = includeSql ? new IntegritySqlBuilder(context.GetService<ISqlGenerationHelper>()) : null;
                reports.Add(new IntegrityContextReport(registration.ContextType.Name, context.Database.ProviderName ?? "unknown", result, sql));
            }
            catch (DataIntegrityConfigurationException ex)
            {
                problems.AddRange(ex.Problems);
            }
        }

        if (problems.Count > 0)
        {
            Console.Error.WriteLine(new DataIntegrityConfigurationException(problems).Message);
            Environment.ExitCode = 1;
            return true;
        }

        IntegrityLinkReportWriter.Write(Console.Out, contextSet.ModuleKey, reports, includeSql);
        Console.Out.Flush();
        return true;
    }
}
