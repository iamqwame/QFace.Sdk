using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.ExceptionHandlers;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Tests.Database;
using Xunit;

namespace QimErp.Shared.Common.Tests.MultiTenancy;

public sealed class EmployeeVisibilityTests : IDisposable
{
    private const string Tenant = "019e31ec-empv-0000-0000-000000000001";
    private const string CompanyA = "company-a";
    private const string CompanyB = "company-b";

    private sealed class EmployeeDbContext(DbContextOptions<EmployeeDbContext> options, ITenantContext tenantContext)
        : ApplicationDbContext<EmployeeDbContext>(options, tenantContext)
    {
        public DbSet<TestEmployee> Employees => Set<TestEmployee>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new TestEmployeeConfiguration());
        }
    }

    private sealed class CapturingLogger : ILogger<AuditEntitySaveChangesInterceptor>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NoopScope();
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));

        private sealed class NoopScope : IDisposable
        {
            public void Dispose() { }
        }
    }

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope _scope;

        public EmployeeDbContext Db { get; }
        public CapturingLogger Log { get; } = new();
        public ITenantContext TenantContext { get; }

        public Harness()
        {
            var services = new ServiceCollection();
            services.AddHttpContextAccessor();
            services.AddScoped<UserContextService>();
            services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<UserContextService>());
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddLogging();

            _root = services.BuildServiceProvider();
            _scope = _root.CreateScope();
            var sp = _scope.ServiceProvider;

            var userService = sp.GetRequiredService<UserContextService>();
            userService.SetContext(Tenant, "tester@qimerp.com");
            TenantContext = sp.GetRequiredService<ITenantContext>();
            TenantContext.SetTenant(Tenant);

            var interceptor = new AuditEntitySaveChangesInterceptor(userService, Log, sp);

            Db = new EmployeeDbContext(
                new DbContextOptionsBuilder<EmployeeDbContext>()
                    .UseInMemoryDatabase($"employee-{Guid.NewGuid()}")
                    .AddInterceptors(interceptor)
                    .Options,
                TenantContext);
        }

        public void Dispose()
        {
            Db.Dispose();
            _scope.Dispose();
            _root.Dispose();
        }
    }

    private static void SetScope(CompanyScope scope) => new CompanyContext().SetScope(scope);

    public void Dispose() => new CompanyContext().Clear();

    [Fact(DisplayName = "Employee homed in another company is invisible")]
    public async Task Employee_homed_in_another_company_is_invisible()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var employee = new TestEmployee("EMP-1", "Ama", "Owusu");
        harness.Db.Employees.Add(employee);
        await harness.Db.SaveChangesAsync();

        employee.CompanyId.Should().Be(CompanyA);

        SetScope(CompanyScope.ForCompanies([CompanyB], CompanyB));

        var visible = await harness.Db.Employees.ToListAsync();

        visible.Should().BeEmpty();
    }

    [Fact(DisplayName = "Employee visible across companies is readable from another company")]
    public async Task Employee_visible_across_companies_is_readable_from_another_company()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var employee = new TestEmployee("EMP-2", "Kofi", "Mensah");
        employee.WithVisibilityAcrossCompanies(true);
        harness.Db.Employees.Add(employee);
        await harness.Db.SaveChangesAsync();

        employee.CompanyId.Should().Be(CompanyA);

        SetScope(CompanyScope.ForCompanies([CompanyB], CompanyB));

        var visible = await harness.Db.Employees.ToListAsync();

        visible.Should().ContainSingle();
        visible[0].CompanyId.Should().Be(CompanyA);
    }

    [Fact(DisplayName = "Employee visible across companies never crosses a tenant boundary")]
    public async Task Employee_visible_across_companies_never_crosses_tenant_boundary()
    {
        const string OtherTenant = "019e31ec-empv-0000-0000-000000000002";

        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var employee = new TestEmployee("EMP-11", "Akosua", "Boateng");
        employee.WithVisibilityAcrossCompanies(true);
        harness.Db.Employees.Add(employee);
        await harness.Db.SaveChangesAsync();

        employee.CompanyId.Should().Be(CompanyA);

        harness.TenantContext.SetTenant(OtherTenant);

        var visible = await harness.Db.Employees.ToListAsync();

        visible.Should().BeEmpty();
    }

    [Fact(DisplayName = "Employee visible across companies is still not writable from another company")]
    public async Task Employee_visible_across_companies_is_not_writable_from_another_company()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var employee = new TestEmployee("EMP-7", "Kojo", "Appiah");
        employee.WithVisibilityAcrossCompanies(true);
        harness.Db.Employees.Add(employee);
        await harness.Db.SaveChangesAsync();

        SetScope(CompanyScope.ForCompanies([CompanyB], CompanyB));

        var visible = await harness.Db.Employees.SingleAsync();
        visible.UpdateBasicInfo("Kojo", "Mensah-Appiah");

        var act = async () => await harness.Db.SaveChangesAsync();

        await act.Should().ThrowAsync<CrossCompanyWriteException>();
    }

    [Fact(DisplayName = "Added employee with no company throws and names both concepts")]
    public async Task Added_employee_with_no_company_throws_and_names_both_concepts()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.AllCompanies(active: null));

        harness.Db.Employees.Add(new TestEmployee("EMP-3", "Yaw", "Boateng"));

        var act = async () => await harness.Db.SaveChangesAsync();

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Message.Should().Contain("WithCompanyId").And.Contain("WithVisibilityAcrossCompanies");
    }

    [Fact(DisplayName = "CompanyStampScope.EnterShared with an employee throws and names both concepts")]
    public async Task Added_employee_via_CompanyStampScope_EnterShared_throws()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.AllCompanies(active: null));

        harness.Db.Employees.Add(new TestEmployee("EMP-4", "Abena", "Asante"));

        var act = async () =>
        {
            using (CompanyStampScope.EnterShared())
            {
                await harness.Db.SaveChangesAsync();
            }
        };

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Message.Should().Contain("WithCompanyId").And.Contain("WithVisibilityAcrossCompanies");
    }

    [Fact(DisplayName = "BulkSeedScope with an employee throws instead of stamping blank")]
    public async Task Added_employee_via_BulkSeedScope_throws()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.AllCompanies(active: null));

        harness.Db.Employees.Add(new TestEmployee("EMP-5", "Kwame", "Adjei"));

        var act = async () =>
        {
            using (BulkSeedScope.Enter())
            {
                await harness.Db.SaveChangesAsync();
            }
        };

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Message.Should().Contain("WithCompanyId").And.Contain("WithVisibilityAcrossCompanies");

        harness.Log.Entries.Should().NotContain(e =>
            e.Message.Contains("BulkSeedScope active with no company write target"));
    }

    [Fact(DisplayName = "Employee with no company saves when multi-company is off")]
    public async Task Employee_with_no_company_saves_when_multi_company_is_off()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.Inactive);

        var employee = new TestEmployee("EMP-6", "Efua", "Darko");
        harness.Db.Employees.Add(employee);

        await harness.Db.SaveChangesAsync();

        employee.CompanyId.Should().BeEmpty();
    }

    [Fact(DisplayName = "Modified employee cannot be blanked to no company while multi-company is on")]
    public async Task Modified_employee_cannot_be_blanked_to_no_company_while_multi_company_is_on()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var employee = new TestEmployee("EMP-8", "Abena", "Osei");
        harness.Db.Employees.Add(employee);
        await harness.Db.SaveChangesAsync();
        employee.CompanyId.Should().Be(CompanyA);

        SetScope(CompanyScope.AllCompanies(active: null));
        employee.AsTenantShared();

        var act = async () => await harness.Db.SaveChangesAsync();

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Message.Should().Contain("WithCompanyId").And.Contain("WithVisibilityAcrossCompanies");
    }

    [Fact(DisplayName = "Modified employee already shared can be resaved without throwing")]
    public async Task Modified_employee_already_shared_can_be_resaved_without_throwing()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.Inactive);

        var employee = new TestEmployee("EMP-9", "Yaa", "Amponsah");
        harness.Db.Employees.Add(employee);
        await harness.Db.SaveChangesAsync();
        employee.CompanyId.Should().BeEmpty();

        SetScope(CompanyScope.AllCompanies(active: null));
        employee.UpdateBasicInfo("Yaa", "Amponsah-Mensah");

        var act = async () => await harness.Db.SaveChangesAsync();

        await act.Should().NotThrowAsync();
        employee.CompanyId.Should().BeEmpty();
    }

    [Fact(DisplayName = "Modified employee can be blanked to no company when multi-company is off")]
    public async Task Modified_employee_can_be_blanked_to_no_company_when_multi_company_is_off()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var employee = new TestEmployee("EMP-10", "Adjoa", "Frimpong");
        harness.Db.Employees.Add(employee);
        await harness.Db.SaveChangesAsync();
        employee.CompanyId.Should().Be(CompanyA);

        SetScope(CompanyScope.Inactive);
        employee.AsTenantShared();

        var act = async () => await harness.Db.SaveChangesAsync();

        await act.Should().NotThrowAsync();
        employee.CompanyId.Should().BeEmpty();
    }

    [Fact(DisplayName = "Modified employee cannot be blanked to a whitespace-only company while multi-company is on")]
    public async Task Modified_employee_cannot_be_blanked_to_whitespace_company_while_multi_company_is_on()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var employee = new TestEmployee("EMP-12", "Nana", "Yeboah");
        harness.Db.Employees.Add(employee);
        await harness.Db.SaveChangesAsync();
        employee.CompanyId.Should().Be(CompanyA);

        SetScope(CompanyScope.AllCompanies(active: null));
        employee.WithCompanyId("   ");

        var act = async () => await harness.Db.SaveChangesAsync();

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Message.Should().Contain("WithCompanyId").And.Contain("WithVisibilityAcrossCompanies");
    }

    [Fact(DisplayName = "Modified employee blanked to no company still throws inside BulkSeedScope")]
    public async Task Modified_employee_blanked_to_no_company_throws_inside_BulkSeedScope()
    {
        using var harness = new Harness();
        SetScope(CompanyScope.ForCompanies([CompanyA], CompanyA));

        var employee = new TestEmployee("EMP-13", "Esi", "Danso");
        harness.Db.Employees.Add(employee);
        await harness.Db.SaveChangesAsync();
        employee.CompanyId.Should().Be(CompanyA);

        employee.AsTenantShared();

        var act = async () =>
        {
            using (BulkSeedScope.Enter())
            {
                await harness.Db.SaveChangesAsync();
            }
        };

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
