using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Extensions;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Database;

public sealed class OutboxMessageMappingTests
{
    private sealed class OutboxOnlyDbContext(
        DbContextOptions<OutboxOnlyDbContext> options,
        ITenantContext tenantContext)
        : ApplicationDbContext<OutboxOnlyDbContext>(options, tenantContext)
    {
    }

    private static ConventionTestDbContext CreateModelContext()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-outbox");

        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseQimErpNpgsql("Host=localhost;Database=outbox_mapping_tests;Username=none;Password=none")
            .Options;

        return new ConventionTestDbContext(options, tenantContext);
    }

    private static OutboxOnlyDbContext CreateInMemoryContext()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-outbox");

        var options = new DbContextOptionsBuilder<OutboxOnlyDbContext>()
            .UseInMemoryDatabase($"outbox-{Guid.NewGuid()}")
            .Options;

        return new OutboxOnlyDbContext(options, tenantContext);
    }

    [Fact]
    public void Model_maps_OutboxMessage_to_OutboxMessages_table()
    {
        using var context = CreateModelContext();

        var entityType = context.Model.FindEntityType(typeof(OutboxMessage));
        entityType.Should().NotBeNull();
        var mapped = entityType!;
        mapped.GetTableName().Should().Be("OutboxMessages");
        mapped.FindPrimaryKey().Should().NotBeNull();
        mapped.FindProperty(nameof(OutboxMessage.IsProcessed)).Should().BeNull(
            "IsProcessed is a shim; EF maps ProcessedAt only");
        mapped.FindProperty(nameof(OutboxMessage.ProcessedAt)).Should().NotBeNull();
        mapped.FindProperty(nameof(OutboxMessage.OccurredAt)).Should().NotBeNull();
    }

    [Fact]
    public async Task Can_Add_and_SaveChanges_OutboxMessage_in_memory()
    {
        await using var context = CreateInMemoryContext();

        var message = OutboxMessage.Create("Test.Event", """{"ok":true}""");
        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();

        var loaded = await context.OutboxMessages.SingleAsync(m => m.Id == message.Id);
        loaded.EventType.Should().Be("Test.Event");
        loaded.Payload.Should().Be("""{"ok":true}""");
        loaded.IsProcessed.Should().BeFalse();
    }
}
