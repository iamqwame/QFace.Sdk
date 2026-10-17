using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services.Retention;
using Xunit;

namespace QimErp.Shared.Common.Tests.Services;

public class RetentionPolicyClientTests
{
    [Fact]
    public async Task ReturnsNull_WhenTenantBlank_WithoutHttp()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("should not call HTTP"));
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.GetRetentionDaysAfterCloseAsync("  ", RetentionClass.Transient);

        result.Should().BeNull();
        handler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task ReturnsNull_WhenWorkflowServiceUrlMissing()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("should not call HTTP"));
        var client = CreateClient(handler, workflowUrl: null, apiKey: "key");

        var result = await client.GetRetentionDaysAfterCloseAsync("tenant-a", RetentionClass.Transient);

        result.Should().BeNull();
        handler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task ReturnsNull_WhenApiKeyMissing()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("should not call HTTP"));
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "");

        var result = await client.GetRetentionDaysAfterCloseAsync("tenant-a", RetentionClass.Transient);

        result.Should().BeNull();
        handler.RequestCount.Should().Be(0);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ReturnsNull_WhenHttpNonSuccess(HttpStatusCode statusCode)
    {
        var handler = new RecordingHandler(_ =>
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.GetRetentionDaysAfterCloseAsync("tenant-a", RetentionClass.Transient);

        result.Should().BeNull();
        handler.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task ReturnsDays_WhenHttp200_WithValue()
    {
        var handler = new RecordingHandler(request =>
        {
            request.Headers.GetValues("X-Internal-Api-Key").Should().ContainSingle().Which.Should().Be("secret-key");
            request.RequestUri!.AbsolutePath.Should().Be("/internal/retention-policies/Transient");
            request.RequestUri.Query.Should().Contain("tenantId=tenant-a");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"retentionDaysAfterClose\":14}",
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test/", apiKey: "secret-key");

        var result = await client.GetRetentionDaysAfterCloseAsync("tenant-a", RetentionClass.Transient);

        result.Should().Be(14);
    }

    [Fact]
    public async Task ReturnsNull_WhenHttp200_WithNullDays()
    {
        var handler = new RecordingHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"retentionDaysAfterClose\":null}",
                    Encoding.UTF8,
                    "application/json")
            });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.GetRetentionDaysAfterCloseAsync("tenant-a", RetentionClass.Permanent);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ReturnsNull_WhenJsonParseFails()
    {
        var handler = new RecordingHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", Encoding.UTF8, "application/json")
            });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.GetRetentionDaysAfterCloseAsync("tenant-a", RetentionClass.Transient);

        result.Should().BeNull();
    }

    [Fact]
    public async Task NormalizesGuidTenantId_BeforeSendingQuery()
    {
        var tenantGuid = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var expectedTenantId = tenantGuid.ToString("D").ToLowerInvariant();
        Uri? sentUri = null;
        var handler = new RecordingHandler(request =>
        {
            sentUri = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"retentionDaysAfterClose\":7}", Encoding.UTF8, "application/json")
            };
        });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        await client.GetRetentionDaysAfterCloseAsync(
            tenantGuid.ToString("D").ToUpperInvariant(),
            RetentionClass.Transient);

        sentUri!.Query.Should().Contain($"tenantId={Uri.EscapeDataString(expectedTenantId)}");
    }

    private static RetentionPolicyClient CreateClient(
        RecordingHandler handler,
        string? workflowUrl,
        string apiKey)
    {
        var values = new Dictionary<string, string?>();
        if (workflowUrl is not null)
            values["WorkflowServiceUrl"] = workflowUrl;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new RetentionPolicyClient(
            new StubHttpClientFactory(handler),
            configuration,
            Microsoft.Extensions.Options.Options.Create(new InternalApiOptions { ExpectedApiKey = apiKey }),
            NullLogger<RetentionPolicyClient>.Instance);
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(responder(request));
        }
    }
}
