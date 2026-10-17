using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services.LegalHold;
using Xunit;

namespace QimErp.Shared.Common.Tests.Services;

public class LegalHoldGateClientTests
{
    private static readonly Guid PersonId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Fact]
    public async Task ReturnsTrue_WhenTenantBlank_WithoutHttp()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("should not call HTTP"));
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.HasActivePersonLegalHoldAsync("  ", PersonId);

        result.Should().BeTrue();
        handler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task ReturnsTrue_WhenPersonIdEmpty_WithoutHttp()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("should not call HTTP"));
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.HasActivePersonLegalHoldAsync("tenant-a", Guid.Empty);

        result.Should().BeTrue();
        handler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task ReturnsTrue_WhenWorkflowServiceUrlMissing()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("should not call HTTP"));
        var client = CreateClient(handler, workflowUrl: null, apiKey: "key");

        var result = await client.HasActivePersonLegalHoldAsync("tenant-a", PersonId);

        result.Should().BeTrue();
        handler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task ReturnsTrue_WhenApiKeyMissing()
    {
        var handler = new RecordingHandler(_ => throw new InvalidOperationException("should not call HTTP"));
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "");

        var result = await client.HasActivePersonLegalHoldAsync("tenant-a", PersonId);

        result.Should().BeTrue();
        handler.RequestCount.Should().Be(0);
    }

    [Fact]
    public async Task ReturnsTrue_WhenHttpNonSuccess()
    {
        var handler = new RecordingHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.HasActivePersonLegalHoldAsync("tenant-a", PersonId);

        result.Should().BeTrue();
        handler.RequestCount.Should().Be(1);
    }

    [Fact]
    public async Task ReturnsTrue_WhenJsonParseFails()
    {
        var handler = new RecordingHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not-json", Encoding.UTF8, "application/json")
            });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.HasActivePersonLegalHoldAsync("tenant-a", PersonId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnsTrue_OnTransportException()
    {
        var handler = new RecordingHandler(_ => throw new HttpRequestException("boom"));
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.HasActivePersonLegalHoldAsync("tenant-a", PersonId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Propagates_OperationCanceledException()
    {
        var handler = new RecordingHandler(_ => throw new OperationCanceledException());
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var act = () => client.HasActivePersonLegalHoldAsync("tenant-a", PersonId);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Returns_HasActiveHold_OnHttp200(bool hasActiveHold)
    {
        var handler = new RecordingHandler(request =>
        {
            request.Headers.GetValues("X-Internal-Api-Key").Should().ContainSingle().Which.Should().Be("secret-key");
            request.RequestUri!.AbsolutePath.Should().Be($"/internal/legal-holds/person/{PersonId:D}");
            request.RequestUri.Query.Should().Contain("tenantId=tenant-a");
            request.RequestUri.Query.Should().Contain("asOfUtc=");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $"{{\"hasActiveHold\":{(hasActiveHold ? "true" : "false")}}}",
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test/", apiKey: "secret-key");

        var result = await client.HasActivePersonLegalHoldAsync("tenant-a", PersonId);

        result.Should().Be(hasActiveHold);
    }

    [Fact]
    public async Task ReturnsTrue_WhenHttp200_MissingHasActiveHold()
    {
        var handler = new RecordingHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        var result = await client.HasActivePersonLegalHoldAsync("tenant-a", PersonId);

        result.Should().BeTrue();
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
                Content = new StringContent("{\"hasActiveHold\":false}", Encoding.UTF8, "application/json")
            };
        });
        var client = CreateClient(handler, workflowUrl: "http://workflow.test", apiKey: "key");

        await client.HasActivePersonLegalHoldAsync(tenantGuid.ToString("D").ToUpperInvariant(), PersonId);

        sentUri!.Query.Should().Contain($"tenantId={Uri.EscapeDataString(expectedTenantId)}");
    }

    private static LegalHoldGateClient CreateClient(
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

        return new LegalHoldGateClient(
            new StubHttpClientFactory(handler),
            configuration,
            Microsoft.Extensions.Options.Options.Create(new InternalApiOptions { ExpectedApiKey = apiKey }),
            NullLogger<LegalHoldGateClient>.Instance);
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
