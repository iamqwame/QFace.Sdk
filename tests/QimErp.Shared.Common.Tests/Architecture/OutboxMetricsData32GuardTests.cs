using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

public class OutboxMetricsData32GuardTests
{
    [Fact]
    public void OutboxMetrics_declares_DATA32_meter_and_record_APIs()
    {
        var path = FindSourceFile("OutboxMetrics.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("DATA-32");
        content.Should().Contain("MeterName");
        content.Should().Contain("outbox.messages.enqueued");
        content.Should().Contain("outbox.messages.published");
        content.Should().Contain("outbox.messages.processed");
        content.Should().Contain("outbox.messages.failed");
        content.Should().Contain("outbox.batch.size");
        content.Should().Contain("outbox.messages.pending");
        content.Should().Contain("RecordEnqueued");
        content.Should().Contain("RecordPublished");
        content.Should().Contain("RecordProcessed");
        content.Should().Contain("RecordFailed");
        content.Should().Contain("RecordSweepBatch");
    }

    [Fact]
    public void EfOutbox_records_enqueued_on_Add()
    {
        var path = FindSourceFile("EfOutbox.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("OutboxMetrics.RecordEnqueued");
    }

    [Fact]
    public void OutboxProcessorBackgroundService_records_sweep_processed_failed()
    {
        var path = FindSourceFile("OutboxProcessorBackgroundService.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("OutboxMetrics.RecordSweepBatch");
        content.Should().Contain("OutboxMetrics.RecordProcessed");
        content.Should().Contain("OutboxMetrics.RecordFailed");
    }

    [Fact]
    public void Akka_outbox_path_records_sweep_processed_failed()
    {
        var poller = FindSourceFile("OutboxPollerActor.cs", "QimErp.Shared.Common");
        poller.Should().NotBeNull();
        File.ReadAllText(poller!).Should().Contain("OutboxMetrics.RecordSweepBatch");

        var worker = FindSourceFile("OutboxDispatchWorkerActor.cs", "QimErp.Shared.Common");
        worker.Should().NotBeNull();
        var workerContent = File.ReadAllText(worker!);
        workerContent.Should().Contain("OutboxMetrics.RecordProcessed");
        workerContent.Should().Contain("OutboxMetrics.RecordFailed");
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
