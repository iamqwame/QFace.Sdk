using FluentAssertions;
using QimErp.Shared.Common.Activities;
using QimErp.Shared.Common.Events;
using Xunit;

namespace QimErp.Shared.Common.Tests.Events;

public sealed class PersonErasedEventTests
{
    [Fact(DisplayName = "Create sets envelope Ids-only fields")]
    public void Create_sets_envelope_without_pii()
    {
        var personId = Guid.CreateVersion7();

        var @event = PersonErasedEvent.Create(personId, "tenant-1", correlationId: "c-1");

        @event.EventId.Should().NotBe(Guid.Empty);
        @event.EventType.Should().Be("PersonErased");
        @event.TenantId.Should().Be("tenant-1");
        @event.PersonId.Should().Be(personId);
        @event.CorrelationId.Should().Be("c-1");
        @event.OccurredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Null correlation defaults to empty")]
    public void Create_null_correlation_defaults_empty()
    {
        var @event = PersonErasedEvent.Create(Guid.CreateVersion7(), "tenant-1", correlationId: null);

        @event.CorrelationId.Should().Be(string.Empty);
    }

    [Fact(DisplayName = "Type has no PII properties")]
    public void type_has_no_pii_properties()
    {
        var names = typeof(PersonErasedEvent).GetProperties().Select(p => p.Name).ToHashSet();

        names.Should().NotContain("Email");
        names.Should().NotContain("FirstName");
        names.Should().NotContain("LastName");
        names.Should().NotContain("Phone");
        names.Should().NotContain("Name");
        names.Should().NotContain("Code");
        names.Should().NotContain("EmployeeNo");
        names.Should().NotContain("SsnitNumber");
    }

    [Fact(DisplayName = "PersonErasureRequest.From maps ids")]
    public void PersonErasureRequest_From_maps_ids()
    {
        var personId = Guid.CreateVersion7();
        var erased = PersonErasedEvent.Create(personId, "tenant-1", "corr-9");

        var request = PersonErasureRequest.From(erased);

        request.PersonId.Should().Be(personId);
        request.TenantId.Should().Be("tenant-1");
        request.CorrelationId.Should().Be("corr-9");
    }
}
