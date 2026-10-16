using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QimErp.Shared.Common.Diagnostics.IntegrityChecks;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Diagnostics.IntegrityChecks;

public sealed class OutboxBackstopIntegrityCheckTests
{
    private static TestApplicationDbContext CreateContext()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-integrity");

        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase($"integrity-{Guid.NewGuid()}")
            .Options;

        return new TestApplicationDbContext(options, tenantContext);
    }

    private static OutboxMessage Stale(TimeSpan age) => new()
    {
        EventType = "Test.Event",
        Payload = "{}",
        IsProcessed = false,
        OccurredAt = DateTime.UtcNow - age
    };

    [Fact(DisplayName = "No unprocessed OutboxMessage rows => Clean result with zero count")]
    public async Task NoRows_ReturnsClean()
    {
        await using var context = CreateContext();
        var check = new OutboxBackstopIntegrityCheck<TestApplicationDbContext>(context);

        var result = await check.RunAsync(CancellationToken.None);

        result.HasFindings.Should().BeFalse();
        result.Count.Should().Be(0);
        result.CheckName.Should().Be(OutboxBackstopIntegrityCheck<TestApplicationDbContext>.CheckName);
    }

    [Fact(DisplayName = "Unprocessed row younger than the stale threshold is not counted")]
    public async Task RowYoungerThanThreshold_IsNotCounted()
    {
        await using var context = CreateContext();
        context.OutboxMessages.Add(Stale(TimeSpan.FromMinutes(30)));
        await context.SaveChangesAsync();

        var check = new OutboxBackstopIntegrityCheck<TestApplicationDbContext>(context, TimeSpan.FromHours(1));
        var result = await check.RunAsync(CancellationToken.None);

        result.HasFindings.Should().BeFalse();
        result.Count.Should().Be(0);
    }

    [Fact(DisplayName = "Unprocessed row older than the stale threshold is counted with its Id as a sample")]
    public async Task RowOlderThanThreshold_IsCounted_WithSampleId()
    {
        await using var context = CreateContext();
        var stuck = Stale(TimeSpan.FromHours(2));
        context.OutboxMessages.Add(stuck);
        await context.SaveChangesAsync();

        var check = new OutboxBackstopIntegrityCheck<TestApplicationDbContext>(context, TimeSpan.FromHours(1));
        var result = await check.RunAsync(CancellationToken.None);

        result.HasFindings.Should().BeTrue();
        result.Count.Should().Be(1);
        result.SampleIds.Should().ContainSingle().Which.Should().Be(stuck.Id.ToString());
    }

    [Fact(DisplayName = "Processed rows are never counted, no matter how old")]
    public async Task ProcessedRow_IsNeverCounted()
    {
        await using var context = CreateContext();
        var processed = Stale(TimeSpan.FromDays(1));
        processed.IsProcessed = true;
        context.OutboxMessages.Add(processed);
        await context.SaveChangesAsync();

        var check = new OutboxBackstopIntegrityCheck<TestApplicationDbContext>(context, TimeSpan.FromHours(1));
        var result = await check.RunAsync(CancellationToken.None);

        result.HasFindings.Should().BeFalse();
        result.Count.Should().Be(0);
    }

    [Fact(DisplayName = "Count reflects every stuck row even when sample ids are capped at 10")]
    public async Task CountReflectsAllRows_SampleIdsCappedAt10()
    {
        await using var context = CreateContext();
        for (var i = 0; i < 15; i++)
            context.OutboxMessages.Add(Stale(TimeSpan.FromHours(2)));
        await context.SaveChangesAsync();

        var check = new OutboxBackstopIntegrityCheck<TestApplicationDbContext>(context, TimeSpan.FromHours(1));
        var result = await check.RunAsync(CancellationToken.None);

        result.Count.Should().Be(15);
        result.SampleIds.Should().HaveCount(10);
    }

    [Fact(DisplayName = "Default stale threshold is one hour when none is supplied")]
    public async Task DefaultThreshold_IsOneHour()
    {
        await using var context = CreateContext();
        context.OutboxMessages.Add(Stale(TimeSpan.FromMinutes(50)));
        var stuck = Stale(TimeSpan.FromMinutes(90));
        context.OutboxMessages.Add(stuck);
        await context.SaveChangesAsync();

        var check = new OutboxBackstopIntegrityCheck<TestApplicationDbContext>(context);
        var result = await check.RunAsync(CancellationToken.None);

        result.Count.Should().Be(1);
        result.SampleIds.Should().ContainSingle().Which.Should().Be(stuck.Id.ToString());
    }
}
