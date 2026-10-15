using System.Text.Json;
using FluentAssertions;
using QimErp.Shared.Common.Outbox;
using Xunit;

namespace QimErp.Shared.Common.Tests.Outbox;

public sealed class OutboxEnvelopeTests
{
    private sealed record SamplePayload(string Name, int Count);

    [Fact(DisplayName = "Wrap/Unwrap round-trips TenantId and the serialized message")]
    public void Wrap_Unwrap_RoundTrips_TenantId_And_Data()
    {
        var message = new SamplePayload("widget", 3);

        var payload = OutboxEnvelope.Wrap(message, "tenant-1");
        var envelope = OutboxEnvelope.Unwrap(payload);

        envelope.Should().NotBeNull();
        envelope!.TenantId.Should().Be("tenant-1");

        var roundTripped = JsonSerializer.Deserialize<SamplePayload>(envelope.Data);
        roundTripped.Should().BeEquivalentTo(message);
    }

    [Fact(DisplayName = "Wrap defaults a null tenantId to empty string")]
    public void Wrap_NullTenantId_DefaultsToEmpty()
    {
        var payload = OutboxEnvelope.Wrap(new SamplePayload("x", 1), null!);

        var envelope = OutboxEnvelope.Unwrap(payload);

        envelope!.TenantId.Should().Be(string.Empty);
    }

    [Theory(DisplayName = "Unwrap returns null for null, empty, or whitespace payloads")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Unwrap_ReturnsNull_ForBlankPayload(string? payload)
    {
        OutboxEnvelope.Unwrap(payload).Should().BeNull();
    }

    [Fact(DisplayName = "Wrap/Unwrap round-trips CompanyId alongside TenantId")]
    public void Wrap_Unwrap_RoundTrips_CompanyId()
    {
        var payload = OutboxEnvelope.Wrap(new SamplePayload("widget", 3), "tenant-1", companyId: "company-A");

        var envelope = OutboxEnvelope.Unwrap(payload);

        envelope!.TenantId.Should().Be("tenant-1");
        envelope.CompanyId.Should().Be("company-A");
    }

    [Fact(DisplayName = "Wrap defaults a null or omitted CompanyId to empty string (tenant-shared)")]
    public void Wrap_NullCompanyId_DefaultsToEmpty()
    {
        var payload = OutboxEnvelope.Wrap(new SamplePayload("widget", 3), "tenant-1");

        var envelope = OutboxEnvelope.Unwrap(payload);

        envelope!.CompanyId.Should().BeEmpty();
    }
}
