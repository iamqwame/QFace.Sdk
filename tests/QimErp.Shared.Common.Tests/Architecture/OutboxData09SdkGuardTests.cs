using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-09: SDK durable outbox Appendix-1 shape and SaveChanges → outbox (not Temporal) hand-off.
/// Behaviour coverage: <c>OutboxWorkflowBridgeTests</c>; migration guide: <c>docs/outbox-data-09.md</c>.
/// </summary>
public class OutboxData09SdkGuardTests
{
    [Fact]
    public void OutboxMessage_is_Appendix1_shape()
    {
        var path = FindSourceFile("OutboxMessage.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("TenantId");
        content.Should().Contain("EventType");
        content.Should().Contain("Transport");
        content.Should().Contain("EntityId");
        content.Should().Contain("EntityVersion");
        content.Should().Contain("Payload");
        content.Should().Contain("OccurredAt");
        content.Should().Contain("NextAttemptAt");
        content.Should().Contain("LockedUntil");
        content.Should().Contain("LockedBy");
        content.Should().Contain("Attempts");
        content.Should().Contain("LastError");
        content.Should().Contain("ProcessedAt");
        content.Should().Contain("DeadAt");
    }

    [Fact]
    public void OutboxMessageConfiguration_maps_Appendix1_index()
    {
        var path = FindSourceFile("OutboxMessageConfiguration.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("OutboxMessages");
        content.Should().Contain("ix_outbox_ready");
        content.Should().Contain("ProcessedAt");
        content.Should().Contain("DeadAt");
        content.Should().Contain("Ignore");
        content.Should().Contain("IsProcessed");
    }

    [Fact]
    public void ApplicationDbContext_applies_OutboxMessageConfiguration()
    {
        var path = FindSourceFile("ApplicationDbContext.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("ApplyConfiguration(new OutboxMessageConfiguration())");
        content.Should().Contain("DbSet<OutboxMessage> OutboxMessages");
    }

    [Fact]
    public void AuditEntitySaveChangesInterceptor_does_not_start_Temporal_on_save()
    {
        var path = FindSourceFile("AuditEntitySaveChangesInterceptor.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().NotContain("IWorkflowStarter");
        content.Should().NotContain("StartWorkflowAsync");
        content.Should().NotContain("ITemporalClient");
        content.Should().NotContain("SignalWithStartAsync");
        content.Should().Contain("IOutbox");
    }

    [Fact]
    public void SharedServiceCollectionExtensions_registers_durable_outbox()
    {
        var path = FindSourceFile("SharedServiceCollectionExtensions.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("AddDbContextWithOutbox");
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
