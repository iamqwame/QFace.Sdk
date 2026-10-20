using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Sync;

/// <summary>
/// DATA-07: EmployeeSyncProjectionApplier updates EmployeeBase live mirror only;
/// never apply to owned VOs on transactional work rows.
/// </summary>
public class EmployeeSyncProjectionApplierData07GuardTests
{
    private static readonly string[] WorkRowTypeStrings =
    [
        "Imprest",
        "LeaveRequest",
        "EmployeeLeave",
        "EmployeeAppraisalPlan",
        "TenantAppStoreInstall",
        "WorkflowHistoryEntry",
        "EmployeeValueObject",
        "ActorSnapshot",
        "InitiatorSnapshot",
    ];

    [Fact]
    public void Apply_targets_EmployeeBase_only()
    {
        var content = ReadApplierSource();

        content.Should().Contain("void Apply(EmployeeBase ");
        AssertNoWorkRowTypeStrings(content);
    }

    [Fact]
    public void Apply_updates_CurrentSupervisor_from_projection()
    {
        var content = ReadApplierSource();

        content.Should().Contain("UpdateCurrentSupervisor(");
        content.Should().Contain("projection.SupervisorId");
        content.Should().Contain("projection.SupervisorName");
        content.Should().Contain("projection.SupervisorCode");
        content.Should().Contain("projection.SupervisorTitle");
        content.Should().Contain("projection.SupervisorEmail");
        content.Should().Contain("projection.SupervisorPhone");
    }

    [Fact]
    public void Apply_does_not_reference_work_row_entities()
    {
        var content = ReadApplierSource();

        AssertNoWorkRowTypeStrings(content);
        content.Should().NotMatchRegex(@"\bProject\b");
        content.Should().NotMatchRegex(@"\bWorkflow\b");
        content.Should().NotContain("ExecuteUpdateAsync");
        content.Should().NotContain(".Imprests");
        content.Should().NotContain(".Projects");
        content.Should().NotContain(".LeaveRequests");
        content.Should().NotContain(".Workflows");
    }

    private static void AssertNoWorkRowTypeStrings(string content)
    {
        foreach (var banned in WorkRowTypeStrings)
            content.Should().NotContain(banned);

        content.Should().NotMatchRegex(@"\bProject\b");
        content.Should().NotMatchRegex(@"\bWorkflow\b");
    }

    private static string ReadApplierSource()
    {
        var path = FindSourceFile("EmployeeSyncProjectionApplier.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        return File.ReadAllText(path!);
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
