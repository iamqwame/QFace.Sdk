using FluentAssertions;
using QimErp.Shared.Common.Services.TenantActivity;
using Xunit;

namespace QimErp.Shared.Common.Tests.TenantActivity;

public class AuditMetadataMergerTests
{
    [Fact]
    public void Merge_ReturnsOriginal_WhenRequestContextMissing()
    {
        const string metadata = """{"invoiceId":"123"}""";

        var result = AuditMetadataMerger.Merge(metadata, null);

        result.Should().Be(metadata);
    }

    [Fact]
    public void Merge_AddsRequestContextWithoutOverwritingBusinessKeys()
    {
        const string metadata = """{"invoiceId":"123","ipAddress":"10.0.0.1"}""";
        var requestContext = new AuditRequestContext("203.0.113.10", "Mozilla/5.0", "session-1");

        var result = AuditMetadataMerger.Merge(metadata, requestContext);

        result.Should().Contain("invoiceId");
        result.Should().Contain("\"ipAddress\":\"10.0.0.1\"");
        result.Should().Contain("userAgent");
        result.Should().Contain("sessionId");
        result.Should().NotContain("203.0.113.10");
    }

    [Fact]
    public void Merge_FillsMissingRequestContextFields()
    {
        var requestContext = new AuditRequestContext("203.0.113.10", "Mozilla/5.0", "session-1");

        var result = AuditMetadataMerger.Merge(null, requestContext);

        result.Should().Contain("\"ipAddress\":\"203.0.113.10\"");
        result.Should().Contain("\"userAgent\":\"Mozilla/5.0\"");
        result.Should().Contain("\"sessionId\":\"session-1\"");
    }
}
