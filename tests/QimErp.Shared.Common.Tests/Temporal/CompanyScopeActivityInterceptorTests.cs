using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QFace.Sdk.Temporal.Interceptors;
using QimErp.Shared.Common.Services.MultiTenancy;
using Temporalio.Worker.Interceptors;
using Xunit;

namespace QimErp.Shared.Common.Tests.Temporal;

/// <summary>
/// Company-scope seeding in <see cref="TenantContextActivityInterceptor"/>.
/// </summary>
public sealed class CompanyScopeActivityInterceptorTests
{
    private const string TenantA = "tenant-aaa-111";
    private const string CompanyA = "company-a";
    private const string CompanyB = "company-b";

    private record InputWithCompany(string TenantId, string CompanyId);
    private record InputWithoutCompany(string TenantId);
    private record BatchItem(string TenantId, string CompanyId);

    private static async Task<CompanyScope?> RunAsync(object payload)
    {
        var companyContext = new CompanyContext();

        var services = new ServiceCollection();
        services.AddSingleton<ICompanyScopeSetter>(companyContext);
        services.AddSingleton<ITenantScopeSetter>(new TenantContext());
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var sp = services.BuildServiceProvider();

        var interceptor = new TenantContextActivityInterceptor(sp.GetRequiredService<IServiceScopeFactory>());

        CompanyScope? observed = null;
        var terminal = new TerminalInterceptor(() => observed = companyContext.Current);

        var wrapped = interceptor.InterceptActivity(terminal);
        await wrapped.ExecuteActivityAsync(MakeInput(payload));
        return observed;
    }

    [Fact(DisplayName = "Activity request carrying CompanyId seeds an active company scope")]
    public async Task Seeds_company_scope_from_request()
    {
        var scope = await RunAsync(new InputWithCompany(TenantA, CompanyA));

        scope!.FilterActive.Should().BeTrue();
        scope.MultiCompanyEnabled.Should().BeTrue();
        scope.ActiveCompanyId.Should().Be(CompanyA);
        scope.AllowedCompanyIds.Should().BeEquivalentTo([string.Empty, CompanyA]);
    }

    [Fact(DisplayName = "Activity request without CompanyId leaves the company filter inactive")]
    public async Task No_company_id_leaves_filter_inactive()
    {
        var scope = await RunAsync(new InputWithoutCompany(TenantA));

        scope!.FilterActive.Should().BeFalse();
        scope.MultiCompanyEnabled.Should().BeFalse(
            "an activity with no CompanyId must behave exactly as it did before multi-company");
        scope.ActiveCompanyId.Should().BeNull();
    }

    [Fact(DisplayName = "CRITICAL: a collection-valued activity arg does not inherit element[0]'s CompanyId")]
    public async Task Collection_arg_does_not_inherit_first_element_company()
    {
        var batch = new List<BatchItem>
        {
            new(TenantA, CompanyA),
            new(TenantA, CompanyB)
        };

        var scope = await RunAsync(batch);

        scope!.ActiveCompanyId.Should().BeNull(
            "a bulk batch is single-tenant by construction but NOT single-company — probing " +
            "element[0] would stamp the whole batch with one company's id");
        scope.FilterActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Company scope is cleared after the activity completes")]
    public async Task Clears_company_scope_after_activity()
    {
        var companyContext = new CompanyContext();

        var services = new ServiceCollection();
        services.AddSingleton<ICompanyScopeSetter>(companyContext);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var sp = services.BuildServiceProvider();

        var interceptor = new TenantContextActivityInterceptor(sp.GetRequiredService<IServiceScopeFactory>());
        var wrapped = interceptor.InterceptActivity(new TerminalInterceptor(() => { }));
        await wrapped.ExecuteActivityAsync(MakeInput(new InputWithCompany(TenantA, CompanyA)));

        companyContext.Current.Should().BeSameAs(CompanyScope.Inactive);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ExecuteActivityInput MakeInput(object payload)
    {
        var ctor = typeof(ExecuteActivityInput)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("No ExecuteActivityInput constructor found.");

        var ps = ctor.GetParameters();
        var args = new object?[ps.Length];
        for (var i = 0; i < ps.Length; i++)
        {
            var p = ps[i];
            if (p.ParameterType == typeof(object[]))
                args[i] = new[] { payload };
            else if (p.HasDefaultValue)
                args[i] = p.DefaultValue;
            else if (p.ParameterType.IsValueType)
                args[i] = Activator.CreateInstance(p.ParameterType);
            else
                args[i] = null;
        }

        return (ExecuteActivityInput)ctor.Invoke(args);
    }

    private sealed class TerminalInterceptor(Action onExecute) : ActivityInboundInterceptor(null!)
    {
        public override Task<object?> ExecuteActivityAsync(ExecuteActivityInput input)
        {
            onExecute();
            return Task.FromResult<object?>(null);
        }
    }
}
