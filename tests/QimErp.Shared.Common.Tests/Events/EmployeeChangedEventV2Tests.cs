using FluentAssertions;
using QimErp.Shared.Common.Events;
using Xunit;

namespace QimErp.Shared.Common.Tests.Events;

public sealed class EmployeeChangedEventV2Tests
{
    [Fact(DisplayName = "V2 Create sets envelope Ids-only fields")]
    public void Create_sets_envelope_without_pii()
    {
        var employeeId = Guid.CreateVersion7();

        var @event = EmployeeChangedEventV2.Create(employeeId, "tenant-1", version: 7, correlationId: "c-1");

        @event.EventId.Should().NotBe(Guid.Empty);
        @event.EventType.Should().Be("EmployeeChanged");
        @event.TenantId.Should().Be("tenant-1");
        @event.EmployeeId.Should().Be(employeeId);
        @event.Version.Should().Be(7);
        @event.CorrelationId.Should().Be("c-1");
        @event.OccurredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Legacy ToV2 drops PII and keeps Ids")]
#pragma warning disable CS0618 // intentional dual-publish bridge
    public void ToV2_drops_pii_keeps_ids()
    {
        var employeeId = Guid.CreateVersion7();
        var supervisorId = Guid.CreateVersion7();
        var legacy = EmployeeChangedEvent
            .Create("ama@bank.com", "Ama", "Owusu", "tenant-1", "system@qimerp.com")
            .WithEmployeeId(employeeId.ToString())
            .WithCompanyId("co-1")
            .WithPhoneNumber("+233200000000")
            .WithSsnitNumber("SSNIT-1")
            .WithTaxIdentificationNumber("TIN-1")
            .WithSupervisor(supervisorId, "Boss", "B1", "Mgr", "boss@bank.com", "111")
            .WithJobTitle(Guid.CreateVersion7(), "Engineer", "ENG");
        legacy.Version = 9;
        legacy.CorrelationId = "corr";

        var v2 = legacy.ToV2();

        v2.EmployeeId.Should().Be(employeeId);
        v2.TenantId.Should().Be("tenant-1");
        v2.Version.Should().Be(9);
        v2.CompanyId.Should().Be("co-1");
        v2.SupervisorId.Should().Be(supervisorId);
        v2.JobTitleId.Should().Be(legacy.JobTitleId);
        v2.CorrelationId.Should().Be("corr");

        // V2 type has no PII properties — compile-time guarantee. Spot-check Ids only.
        v2.GetType().GetProperty("Email").Should().BeNull();
        v2.GetType().GetProperty("FirstName").Should().BeNull();
        v2.GetType().GetProperty("SsnitNumber").Should().BeNull();
    }

    [Fact(DisplayName = "StripGovernmentIds nulls gov IDs and keeps contact names")]
    public void StripGovernmentIds_nulls_gov_ids_keeps_email_and_names()
    {
        var legacy = EmployeeChangedEvent
            .Create("ama@bank.com", "Ama", "Owusu", "tenant-1", "system@qimerp.com")
            .WithSsnitNumber("SSNIT-1")
            .WithTaxIdentificationNumber("TIN-1")
            .WithNhilNumber("NHIL-1")
            .WithGhanaCardNumber("GHA-1");

        var result = legacy.StripGovernmentIds();

        result.Should().BeSameAs(legacy);
        legacy.SsnitNumber.Should().BeNull();
        legacy.TaxIdentificationNumber.Should().BeNull();
        legacy.NhilNumber.Should().BeNull();
        legacy.GhanaCardNumber.Should().BeNull();
        legacy.Email.Should().Be("ama@bank.com");
        legacy.FirstName.Should().Be("Ama");
        legacy.LastName.Should().Be("Owusu");
    }
#pragma warning restore CS0618
}
