using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Workflow.Configurations;

namespace QimErp.Shared.Common.Database;

public abstract class ApplicationDbContext<TContext>(
    DbContextOptions<TContext> options,
    ITenantContext tenantContext)
    : DbContext(options), IWorkflowAwareContext
    where TContext : DbContext
{
    protected readonly ITenantContext _tenantContext = tenantContext;

    public DbSet<AppSetting> AppSettings { get; set; }
    public DbSet<Import> Imports { get; set; }
    public DbSet<EntityWorkflowStep> EntityWorkflowSteps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyGlobalFilters(_tenantContext);
        
        modelBuilder.ApplyConfiguration(new AppSettingConfiguration());
        modelBuilder.ApplyConfiguration(new ImportConfiguration());
        modelBuilder.ApplyConfiguration(new EntityWorkflowStepConfiguration());
    }
}
