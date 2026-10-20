using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-22: Shared.Common ships staged, batched, re-runnable import platform (`Import`, `ImportService`, `AddImports`).
/// </summary>
public class ImportServiceData22GuardTests
{
    [Fact]
    public void ImportService_supports_batched_save_lifecycle()
    {
        var path = FindSourceFile("ImportService.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("BatchesQueued");
        content.Should().Contain("StartBatchSavingAsync");
        content.Should().Contain("StartBatchSaving(");
        content.Should().Contain("BatchesSaved + import.BatchesFailed >= import.BatchesQueued");
    }

    [Fact]
    public void ImportService_sync_outcome_is_atomic_via_ExecuteUpdateAsync()
    {
        var path = FindSourceFile("ImportService.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("UpdateSyncOutcomeAsync");
        content.Should().Contain("ExecuteUpdateAsync");
        content.Should().Contain("SyncSucceededCount");
        content.Should().Contain("SyncFailedCount");

        var methodBody = ExtractMethodBody(content, "UpdateSyncOutcomeAsync");
        methodBody.Should().NotBeNullOrEmpty();
        methodBody.Should().NotContain("RecordSyncOutcome");
    }

    [Fact]
    public void Import_entity_tracks_batch_and_sync_state()
    {
        var path = FindSourceFile("Import.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("BatchesQueued");
        content.Should().Contain("BatchesSaved");
        content.Should().Contain("BatchesFailed");
        content.Should().Contain("StartBatchSaving(");
        content.Should().Match(c => c.Contains("ImportStatus.SavingBatches") || c.Contains("SavingBatches = 5"));
        content.Should().Contain("RecordSyncOutcome");
        content.Should().Contain("SyncSucceededCount");
        content.Should().Contain("SyncFailedCount");
    }

    [Fact]
    public void Import_entity_supports_staged_re_runnable_lifecycle()
    {
        var path = FindSourceFile("Import.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("public static Import Create(");
        content.Should().Contain("CreateId()");
        content.Should().Contain("ImportStatus.NotStarted");
        content.Should().Contain("UpdateProgress(");
        content.Should().Contain("Complete(");
        content.Should().Contain("Fail(");
        content.Should().Contain("ImportStatus");
        content.Should().Contain("InProgress");
        content.Should().Contain("Completed");
        content.Should().Contain("Failed");
    }

    [Fact]
    public void IImportService_exposes_staged_batch_sync_contract()
    {
        var path = FindSourceFile("IImportService.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("StartImportAsync");
        content.Should().Contain("StartBatchSavingAsync");
        content.Should().Match(c =>
            c.Contains("UpdateBatchSaveProgressAsync") || c.Contains("IncrementBatchSaveOutcomeAsync"));
        content.Should().Contain("UpdateSyncOutcomeAsync");
        content.Should().Contain("GetImportAsync");
        content.Should().Contain("FailImportAsync");
    }

    [Fact]
    public void AddImports_registers_module_import_service()
    {
        var path = FindSourceFile("SharedServiceCollectionExtensions.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("AddImports<TContext, TImportService>");
        content.Should().Contain("AddScoped<IImportService, TImportService>");
        content.Should().Contain("where TImportService : class, IImportService");
    }

    private static string ExtractMethodBody(string source, string methodName)
    {
        var signatureIdx = source.IndexOf(methodName + "(", StringComparison.Ordinal);
        if (signatureIdx < 0)
            return string.Empty;

        var braceStart = source.IndexOf('{', signatureIdx);
        if (braceStart < 0)
            return string.Empty;

        var depth = 0;
        for (var i = braceStart; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return source[(braceStart + 1)..i];
            }
        }

        return string.Empty;
    }

    private static string? FindSourceFile(string fileName, string projectFolder)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var project = dir.GetDirectories(projectFolder, SearchOption.AllDirectories)
                .FirstOrDefault(d => !d.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !d.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !d.Name.EndsWith("Tests", StringComparison.Ordinal));
            if (project is not null)
            {
                var match = project.GetFiles(fileName, SearchOption.AllDirectories).FirstOrDefault();
                if (match is not null) return match.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }
}
