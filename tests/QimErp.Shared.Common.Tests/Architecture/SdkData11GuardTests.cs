using System.Reflection;
using FluentAssertions;
using QimErp.Shared.Common.Events;
using QimErp.Shared.Common.Sync;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

public class SdkData11GuardTests
{
    private static readonly string[] ForbiddenV2Pii =
    [
        "FirstName",
        "LastName",
        "MiddleName",
        "PreferredName",
        "Email",
        "PhoneNumber",
        "SsnitNumber",
        "TaxIdentificationNumber",
        "NhilNumber",
        "GhanaCardNumber",
        "JobTitle",
        "OrganizationalUnitName",
        "LocationName",
        "SupervisorName",
        "SupervisorEmail",
        "AuthorizationHeader",
    ];

    [Fact]
    public void EmployeeChangedEventV2_is_ids_only_envelope()
    {
        var props = DeclaredNames(typeof(EmployeeChangedEventV2)).ToHashSet(StringComparer.Ordinal);

        props.Should().Contain([
            "EventId", "EventType", "TenantId", "EmployeeId", "Version", "OccurredAt", "CorrelationId"
        ]);
        props.Should().NotContain(ForbiddenV2Pii);

        var employeeId = Guid.CreateVersion7();
        var evt = EmployeeChangedEventV2.Create(employeeId, "tenant-1", version: 3, correlationId: "c-1");
        evt.EventType.Should().Be(EmployeeChangedEventV2.EventTypeName);
        evt.EmployeeId.Should().Be(employeeId);
        evt.TenantId.Should().Be("tenant-1");
        evt.Version.Should().Be(3);
        evt.EventId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Legacy_EmployeeChangedEvent_is_obsolete()
    {
#pragma warning disable CS0618
        typeof(EmployeeChangedEvent)
            .GetCustomAttribute<ObsoleteAttribute>()
            .Should().NotBeNull("legacy EmployeeChangedEvent must stay Obsolete under DATA-11");
#pragma warning restore CS0618
    }

    [Fact]
    public void EmployeeSyncV2Modules_lists_V2_only_publish_markers()
    {
        var expected = new[]
        {
            "Learning",
            "Performance",
            "Talent",
            "WorkforcePlanning",
            "Payroll",
            "TenantBilling",
            "Leave",
            "Benefit",
            "Surveys",
            "Recruitment",
            "EmployeeEngagement",
            "Project",
            "CashManagement",
        };

        foreach (var suffix in expected)
            EmployeeSyncV2Modules.UsesV2(suffix).Should().BeTrue(suffix);

        EmployeeSyncV2Modules.UsesV2("IAM").Should().BeFalse();
    }

    [Fact]
    public void EmployeeSyncV2Modules_source_contains_only_listed_suffixes()
    {
        var path = FindSourceFile("EmployeeSyncV2Modules.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("SyncEmployeeCreatedV2:");
        content.Should().Contain("\"Learning\"");
        content.Should().Contain("\"CashManagement\"");
        content.Should().NotContain("\"IAM\"");
    }

    [Fact]
    public void EmployeeSyncRequest_prefers_EmployeeV2_carrier()
    {
        var path = FindSourceFile("EmployeeSyncRequest.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("EmployeeV2");
        content.Should().Contain("EmployeeChangedEventV2");
        content.Should().Contain("EmployeeId");
        content.Should().Contain("TenantId");
    }

    private static string[] DeclaredNames(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(p => p.Name)
            .ToArray();

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
