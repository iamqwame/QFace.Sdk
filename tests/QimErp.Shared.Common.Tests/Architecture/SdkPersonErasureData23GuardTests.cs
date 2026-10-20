using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

public class SdkPersonErasureData23GuardTests
{
    [Fact]
    public void ModuleSyncRegistry_PersonErasure_includes_CoreHr_CashManagement_Workflow_Leave_Payroll_and_IAM()
    {
        var path = FindSourceFile("ModuleSyncRegistry.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("SyncType.PersonErasure, ModuleKeys.CoreHR");
        content.Should().Contain("SyncType.PersonErasure, ModuleKeys.CashManagement");
        content.Should().Contain(
            "SyncType.PersonErasure, ModuleKeys.Workflow, \"qimerp-platform-workflow-actor-sync\", \"Workflow\"");
        content.Should().Contain(
            "SyncType.PersonErasure, ModuleKeys.Leave, \"qimerp-leave-employee-sync\", \"Leave\"");
        content.Should().Contain(
            "SyncType.PersonErasure, ModuleKeys.Payroll, \"qimerp-payroll-employee-sync\", \"Payroll\"");
        content.Should().Contain(
            "SyncType.PersonErasure, null, \"qimerp-iam-employee-sync\", \"IAM\", RequiresModuleSelection: false");
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
