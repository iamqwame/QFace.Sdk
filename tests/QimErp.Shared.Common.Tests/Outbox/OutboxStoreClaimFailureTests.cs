using System.Data.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Outbox;
using Xunit;

namespace QimErp.Shared.Common.Tests.Outbox;

public class OutboxStoreClaimFailureTests
{
    [Fact]
    public async Task PostgreSql_claim_failure_does_not_attempt_an_unlocked_fallback_query()
    {
        var interceptor = new RejectConnectionInterceptor();
        var options = new DbContextOptionsBuilder<ClaimContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .AddInterceptors(interceptor).Options;
        await using var db = new ClaimContext(options);
        var store = new OutboxStore(db, NullLogger<OutboxStore>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.ClaimAsync("test-node", 10, TimeSpan.FromSeconds(30), CancellationToken.None));

        exception.Should().BeSameAs(interceptor.Failure);
        interceptor.ConnectionAttempts.Should().Be(1);
    }

    private sealed class ClaimContext(DbContextOptions<ClaimContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessages");
    }

    private sealed class RejectConnectionInterceptor : DbConnectionInterceptor
    {
        public InvalidOperationException Failure { get; } = new("Simulated PostgreSQL claim failure");
        public int ConnectionAttempts { get; private set; }

        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection, ConnectionEventData eventData, InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            ConnectionAttempts++;
            throw Failure;
        }
    }
}
