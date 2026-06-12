using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Workflow.Configurations;

namespace QimErp.Shared.Common.Database;

public abstract class ApplicationDbContext<TContext>(
    DbContextOptions<TContext> options,
    ITenantContext tenantContext)
    : DbContext(options), IWorkflowAwareContext, ITenantQueryFilterContext
    where TContext : DbContext
{
    protected readonly ITenantContext _tenantContext = tenantContext;

    /// <summary>
    /// Current tenant id for the EF global query filter, read fresh from <see cref="ITenantContext"/>
    /// (an AsyncLocal) on every access.
    ///
    /// The query filter references THIS property — an instance member of the DbContext — rather than
    /// the <see cref="ITenantContext"/> service directly. That distinction is load-bearing: EF Core
    /// parameterizes member access rooted on the DbContext (emitting an <c>@__ef_filter__</c> parameter
    /// re-evaluated per query, so one compiled plan is shared safely across tenants). When the filter
    /// instead closed over the injected <c>tenantContext</c> service, EF funcletized <c>TenantId</c> into
    /// a baked-in SQL constant. That constant poisoned the compiled-query cache: whichever tenant first
    /// compiled a given query shape had its id reused for every other tenant hitting that shape until the
    /// process restarted — the cause of the post-onboarding empty-read anomaly.
    /// </summary>
    public string? CurrentTenantId => _tenantContext.TenantId;

    public DbSet<AppSetting> AppSettings { get; set; }
    public DbSet<Import> Imports { get; set; }
    public DbSet<EntityWorkflowStep> EntityWorkflowSteps { get; set; }
    public DbSet<EntityCodeConfig> EntityCodeConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyGlobalFilters(this);

        modelBuilder.ApplyConfiguration(new AppSettingConfiguration());
        modelBuilder.ApplyConfiguration(new ImportConfiguration());
        modelBuilder.ApplyConfiguration(new EntityWorkflowStepConfiguration());
        modelBuilder.ApplyConfiguration(new EntityCodeConfigConfiguration());
    }
}
