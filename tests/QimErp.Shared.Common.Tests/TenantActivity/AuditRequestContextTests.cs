using FluentAssertions;
using Microsoft.AspNetCore.Http;
using QimErp.Shared.Common.Services.TenantActivity;
using Xunit;

namespace QimErp.Shared.Common.Tests.TenantActivity;

public class AuditRequestContextTests
{
    [Fact]
    public void TryCapture_ReturnsNull_WhenHttpContextMissing()
    {
        var accessor = new FakeHttpContextAccessor { HttpContext = null };

        var result = AuditRequestContext.TryCapture(accessor);

        result.Should().BeNull();
    }

    [Fact]
    public void TryCapture_ReadsIpUserAgentAndSessionId()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Forwarded-For"] = "203.0.113.10";
        httpContext.Request.Headers.UserAgent = "Mozilla/5.0 Test";
        httpContext.Items[SessionContextKeys.CurrentSessionId] = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

        var accessor = new FakeHttpContextAccessor { HttpContext = httpContext };

        var result = AuditRequestContext.TryCapture(accessor);

        result.Should().NotBeNull();
        result!.IpAddress.Should().Be("203.0.113.10");
        result.UserAgent.Should().Be("Mozilla/5.0 Test");
        result.SessionId.Should().Be("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }

    private sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
