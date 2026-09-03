using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Filters;
using QimErp.Shared.Common.Options;
using Xunit;

namespace QimErp.Shared.Common.Tests.Filters;

public class InternalApiAuthFilterTests
{
    private const string HeaderName = "X-Internal-Api-Key";

    [Fact]
    public async Task InvokeAsync_RejectsRequest_WhenExpectedKeyNotConfigured()
    {
        var filter = CreateFilter(expectedKey: "");
        var context = CreateContext("/internal/journal-entries/post", providedKey: "anything");
        var nextCalled = false;

        var result = await filter.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        nextCalled.Should().BeFalse();
        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task InvokeAsync_RejectsRequest_WhenHeaderMissing()
    {
        var filter = CreateFilter(expectedKey: "expected-key");
        var context = CreateContext("/internal/journal-entries/post", providedKey: null);
        var nextCalled = false;

        var result = await filter.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        nextCalled.Should().BeFalse();
        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Theory]
    [InlineData("wrong-key")]
    [InlineData("expected-ke")]
    [InlineData("expected-keyy")]
    [InlineData("EXPECTED-KEY")]
    public async Task InvokeAsync_RejectsRequest_WhenKeyDoesNotMatch(string providedKey)
    {
        var filter = CreateFilter(expectedKey: "expected-key");
        var context = CreateContext("/internal/journal-entries/post", providedKey);
        var nextCalled = false;

        var result = await filter.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        nextCalled.Should().BeFalse();
        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task InvokeAsync_RejectsRequest_OutsideInternalPath_WhenKeyMissing()
    {
        var filter = CreateFilter(expectedKey: "expected-key");
        var context = CreateContext("/api/gl/journal-entries", providedKey: null);
        var nextCalled = false;

        var result = await filter.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        nextCalled.Should().BeFalse();
        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task InvokeAsync_InvokesNext_WhenKeyMatches()
    {
        var filter = CreateFilter(expectedKey: "expected-key");
        var context = CreateContext("/internal/journal-entries/post", providedKey: "expected-key");
        var nextCalled = false;

        var result = await filter.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        nextCalled.Should().BeTrue();
        result.Should().NotBeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task InvokeAsync_InvokesNext_OutsideInternalPath_WhenKeyMatches()
    {
        var filter = CreateFilter(expectedKey: "expected-key");
        var context = CreateContext("/api/gl/journal-entries", providedKey: "expected-key");
        var nextCalled = false;

        await filter.InvokeAsync(context, _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        nextCalled.Should().BeTrue();
    }

    private static InternalApiAuthFilter CreateFilter(string expectedKey) =>
        new(Microsoft.Extensions.Options.Options.Create(new InternalApiOptions { ExpectedApiKey = expectedKey }));

    private static EndpointFilterInvocationContext CreateContext(string path, string? providedKey)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = path;
        if (providedKey is not null)
            httpContext.Request.Headers[HeaderName] = providedKey;

        return EndpointFilterInvocationContext.Create(httpContext);
    }
}
