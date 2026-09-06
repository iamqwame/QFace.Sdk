using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Middlewares;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Middlewares;

public sealed class CompanyContextMiddlewareTests
{
    private const string CompanyA = "company-a";
    private const string CompanyB = "company-b";
    private const string CompanyC = "company-c";

    private sealed class Result
    {
        public CompanyScope? Observed { get; set; }
        public CompanyScope? AfterPipeline { get; set; }
        public int StatusCode { get; set; }
        public string Body { get; set; } = string.Empty;
    }

    private static async Task<Result> RunAsync(
        (string Type, string Value)[] claims,
        (string Name, string Value)[]? headers = null,
        bool forceInactive = false,
        Exception? pipelineThrows = null)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            claims.Select(c => new Claim(c.Type, c.Value)), "test"));

        foreach (var (name, value) in headers ?? [])
            context.Request.Headers[name] = value;

        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var companyContext = new CompanyContext();
        var result = new Result();

        var middleware = new CompanyContextMiddleware(_ =>
        {
            result.Observed = companyContext.Current;
            return pipelineThrows is null ? Task.CompletedTask : Task.FromException(pipelineThrows);
        });

        var currentUser = new UserContextService(
            new HttpContextAccessor { HttpContext = context },
            NullLogger<UserContextService>.Instance);

        var options = Microsoft.Extensions.Options.Options.Create(new SystemOptions
        {
            Company = new CompanyOptions { ForceInactive = forceInactive }
        });

        var invoke = middleware.InvokeAsync(
            context, companyContext, currentUser, options, NullLogger<CompanyContextMiddleware>.Instance);

        if (pipelineThrows is not null)
            await Assert.ThrowsAsync(pipelineThrows.GetType(), () => invoke);
        else
            await invoke;

        result.AfterPipeline = companyContext.Current;
        result.StatusCode = context.Response.StatusCode;
        responseBody.Position = 0;
        result.Body = await new StreamReader(responseBody).ReadToEndAsync();
        return result;
    }

    // ── Resolution table ──────────────────────────────────────────────────────

    [Fact(DisplayName = "companyScope claim absent → CompanyScope.Inactive")]
    public async Task No_claim_is_inactive()
    {
        var result = await RunAsync([("tenantId", "t1")]);

        result.Observed!.MultiCompanyEnabled.Should().BeFalse();
        result.Observed.FilterActive.Should().BeFalse();
        result.Observed.ActiveCompanyId.Should().BeNull();
        result.Observed.AllowedCompanyIds.Should().Equal(string.Empty);
    }

    [Fact(DisplayName = "companyScope=all, no headers → AllCompanies(default), FilterActive=false")]
    public async Task Scope_all_without_headers()
    {
        var result = await RunAsync([("companyScope", "all"), ("defaultCompanyId", CompanyA)]);

        result.Observed!.FilterActive.Should().BeFalse();
        result.Observed.MultiCompanyEnabled.Should().BeTrue();
        result.Observed.ActiveCompanyId.Should().Be(CompanyA);
        result.Observed.AllowedCompanyIds.Should().Equal(string.Empty);
    }

    [Fact(DisplayName = "companyScope=all, X-Company-Scope: all → AllCompanies(default)")]
    public async Task Scope_all_with_all_header()
    {
        var result = await RunAsync(
            [("companyScope", "all"), ("defaultCompanyId", CompanyA)],
            [("X-Company-Scope", "all")]);

        result.Observed!.FilterActive.Should().BeFalse();
        result.Observed.ActiveCompanyId.Should().Be(CompanyA);
    }

    [Fact(DisplayName = "companyScope=all, X-Company-Id: X → ForCompanies([X], X)")]
    public async Task Scope_all_narrowed_by_header_id()
    {
        var result = await RunAsync(
            [("companyScope", "all"), ("defaultCompanyId", CompanyA)],
            [("X-Company-Id", CompanyB)]);

        result.Observed!.FilterActive.Should().BeTrue();
        result.Observed.ActiveCompanyId.Should().Be(CompanyB);
        result.Observed.AllowedCompanyIds.Should().BeEquivalentTo([string.Empty, CompanyB]);
    }

    [Fact(DisplayName = "companyScope=list, no headers, default in list → ForCompanies([A,B], default)")]
    public async Task Scope_list_uses_default_when_in_list()
    {
        var result = await RunAsync(
            [("companyScope", "list"), ("companyIds", $"{CompanyA},{CompanyB}"), ("defaultCompanyId", CompanyB)]);

        result.Observed!.FilterActive.Should().BeTrue();
        result.Observed.ActiveCompanyId.Should().Be(CompanyB);
        result.Observed.AllowedCompanyIds.Should().BeEquivalentTo([string.Empty, CompanyA, CompanyB]);
    }

    [Fact(DisplayName = "companyScope=list, default NOT in list → active is null, list unchanged")]
    public async Task Scope_list_ignores_default_outside_list()
    {
        var result = await RunAsync(
            [("companyScope", "list"), ("companyIds", $"{CompanyA},{CompanyB}"), ("defaultCompanyId", CompanyC)]);

        result.Observed!.FilterActive.Should().BeTrue();
        result.Observed.ActiveCompanyId.Should().BeNull();
        result.Observed.AllowedCompanyIds.Should().BeEquivalentTo([string.Empty, CompanyA, CompanyB]);
    }

    [Fact(DisplayName = "companyScope=list, X-Company-Scope: all → the claim list, never wider")]
    public async Task Scope_list_with_all_header_stays_at_claim_list()
    {
        var result = await RunAsync(
            [("companyScope", "list"), ("companyIds", $"{CompanyA},{CompanyB}"), ("defaultCompanyId", CompanyA)],
            [("X-Company-Scope", "all")]);

        result.Observed!.FilterActive.Should().BeTrue("an 'all' header must not disable the company filter");
        result.Observed.ActiveCompanyId.Should().BeNull();
        result.Observed.AllowedCompanyIds.Should().BeEquivalentTo([string.Empty, CompanyA, CompanyB]);
    }

    [Fact(DisplayName = "companyScope=list, X-Company-Id inside the claim → ForCompanies([A], A)")]
    public async Task Scope_list_narrowed_by_header_id()
    {
        var result = await RunAsync(
            [("companyScope", "list"), ("companyIds", $"{CompanyA},{CompanyB}")],
            [("X-Company-Id", CompanyA)]);

        result.Observed!.FilterActive.Should().BeTrue();
        result.Observed.ActiveCompanyId.Should().Be(CompanyA);
        result.Observed.AllowedCompanyIds.Should().BeEquivalentTo([string.Empty, CompanyA]);
    }

    [Fact(DisplayName = "companyScope=list, X-Company-Id outside the claim → 403 company_scope_denied")]
    public async Task Scope_list_rejects_out_of_claim_header()
    {
        var result = await RunAsync(
            [("companyScope", "list"), ("companyIds", $"{CompanyA},{CompanyB}")],
            [("X-Company-Id", CompanyC)]);

        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        result.Observed.Should().BeNull("the pipeline must not run for a denied company scope");

        using var doc = JsonDocument.Parse(result.Body);
        doc.RootElement.GetProperty("error").GetString().Should().Be("company_scope_denied");
        doc.RootElement.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "companyScope=list with an empty companyIds claim → fail-closed to tenant-shared only")]
    public async Task Empty_list_claim_fails_closed()
    {
        var result = await RunAsync([("companyScope", "list"), ("companyIds", "")]);

        result.Observed!.FilterActive.Should().BeTrue();
        result.Observed.ActiveCompanyId.Should().BeNull();
        result.Observed.AllowedCompanyIds.Should().Equal(string.Empty);
        result.Observed.RealCompanyIds.Should().BeEmpty();
    }

    // ── Kill switch + unwind ──────────────────────────────────────────────────

    [Fact(DisplayName = "SystemOptions.Company.ForceInactive overrides claims and headers")]
    public async Task Force_inactive_overrides_everything()
    {
        var result = await RunAsync(
            [("companyScope", "list"), ("companyIds", $"{CompanyA},{CompanyB}"), ("defaultCompanyId", CompanyA)],
            [("X-Company-Id", CompanyA)],
            forceInactive: true);

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.Observed!.MultiCompanyEnabled.Should().BeFalse();
        result.Observed.FilterActive.Should().BeFalse();
        result.Observed.ActiveCompanyId.Should().BeNull();
    }

    [Fact(DisplayName = "Scope is cleared after the request completes")]
    public async Task Clears_scope_after_request()
    {
        var result = await RunAsync(
            [("companyScope", "list"), ("companyIds", CompanyA), ("defaultCompanyId", CompanyA)]);

        result.Observed!.ActiveCompanyId.Should().Be(CompanyA);
        result.AfterPipeline!.Should().BeSameAs(CompanyScope.Inactive,
            "the middleware must unwind its AsyncLocal so it cannot leak into continuations");
    }

    [Fact(DisplayName = "Scope is cleared in finally even when the pipeline throws")]
    public async Task Clears_scope_when_pipeline_throws()
    {
        var result = await RunAsync(
            [("companyScope", "list"), ("companyIds", CompanyA), ("defaultCompanyId", CompanyA)],
            pipelineThrows: new InvalidOperationException("boom"));

        result.Observed!.ActiveCompanyId.Should().Be(CompanyA);
        result.AfterPipeline!.Should().BeSameAs(CompanyScope.Inactive);
    }
}
