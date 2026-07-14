using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using QFace.Sdk.ActorSystems;
using QimErp.Shared.Common.Activities.TenantActivity;
using QimErp.Shared.Common.Actors;
using QimErp.Shared.Common.Services.TenantActivity;
using Xunit;

namespace QimErp.Shared.Common.Tests.TenantActivity;

public class TenantActivityRecorderTests
{
    [Fact]
    public void Record_MergesRequestContextIntoMetadataBeforePublishing()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Forwarded-For"] = "203.0.113.44";
        httpContext.Request.Headers.UserAgent = "AuditTestAgent/1.0";
        httpContext.Items[SessionContextKeys.CurrentSessionId] = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";

        var accessor = new FakeHttpContextAccessor { HttpContext = httpContext };
        RecordTenantActivityRequest? captured = null;
        var actorService = new Mock<IActorService>();
        actorService
            .Setup(x => x.Tell<TenantActivityPublisherActor>(It.IsAny<TenantActivityPublishMessage>(), It.IsAny<string>()))
            .Callback<object, string>((message, _) =>
            {
                if (message is TenantActivityPublishMessage publishMessage)
                {
                    captured = publishMessage.Request;
                }
            });

        var recorder = new TenantActivityRecorder(actorService.Object, accessor);

        recorder.Record(new RecordTenantActivityRequest
        {
            TenantId = "tenant-1",
            Module = TenantActivityModules.Hr,
            ActivityType = HrActivityTypes.EmployeeCreated,
            Summary = "Employee created",
            ActorUserId = Guid.NewGuid(),
            MetadataJson = """{"employeeCode":"E001"}"""
        });

        captured.Should().NotBeNull();
        captured!.MetadataJson.Should().Contain("employeeCode");
        captured.MetadataJson.Should().Contain("203.0.113.44");
        captured.MetadataJson.Should().Contain("AuditTestAgent/1.0");
        captured.MetadataJson.Should().Contain("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        captured.CorrelationId.Should().NotBeNullOrWhiteSpace();
    }

    private sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
