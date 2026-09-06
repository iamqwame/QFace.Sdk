using QimErp.Shared.Common.Extensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Tests.Workflow;
using QimErp.Shared.Common.Workflow.Configurations;
using Xunit;

namespace QimErp.Shared.Common.Tests.Database;

public sealed class IndexShapeTests
{
    private sealed class TestWorkflowEntityConfiguration : WorkflowEnabledEntityConfiguration<TestWorkflowEntity>;

    private sealed class WorkflowIndexDbContext(DbContextOptions<WorkflowIndexDbContext> options) : DbContext(options)
    {
        public DbSet<TestWorkflowEntity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestWorkflowEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantId).IsRequired();
                entity.Ignore(e => e.CustomFields);
                entity.Ignore(e => e.DomainEvents);
                entity.OwnsOne(e => e.Details);
            });
            modelBuilder.ApplyConfiguration(new TestWorkflowEntityConfiguration());
        }
    }

    private static ConventionTestDbContext CreateAuditableContext()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant("tenant-a");

        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseQimErpNpgsql("Host=localhost;Database=index_shape_tests;Username=none;Password=none")
            .Options;

        return new ConventionTestDbContext(options, tenantContext);
    }

    private static WorkflowIndexDbContext CreateWorkflowContext()
    {
        var options = new DbContextOptionsBuilder<WorkflowIndexDbContext>()
            .UseQimErpNpgsql("Host=localhost;Database=index_shape_tests_wf;Username=none;Password=none")
            .Options;

        return new WorkflowIndexDbContext(options);
    }

    private static bool HasIndex(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType, bool unique, params string[] propertyNames) =>
        entityType.GetIndexes().Any(i =>
            i.IsUnique == unique && i.Properties.Select(p => p.Name).SequenceEqual(propertyNames));

    [Fact(DisplayName = "AppSetting has a unique index on {TenantId, CompanyId, Key}, not the old {TenantId, Key}")]
    public void AppSetting_HasWidenedUniqueIndex()
    {
        using var context = CreateAuditableContext();
        var entityType = context.Model.FindEntityType(typeof(AppSetting))!;

        HasIndex(entityType, unique: true, "TenantId", "CompanyId", "Key").Should().BeTrue();
        HasIndex(entityType, unique: true, "TenantId", "Key").Should().BeFalse();
    }

    [Fact(DisplayName = "EntityCodeConfig has a unique index on {TenantId, CompanyId, EntityType} named accordingly")]
    public void EntityCodeConfig_HasWidenedUniqueIndex()
    {
        using var context = CreateAuditableContext();
        var entityType = context.Model.FindEntityType(typeof(EntityCodeConfig))!;

        var index = entityType.GetIndexes().SingleOrDefault(i =>
            i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "CompanyId", "EntityType" }));

        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be("IX_EntityCodeConfigs_TenantId_CompanyId_EntityType");

        HasIndex(entityType, unique: true, "TenantId", "EntityType").Should().BeFalse();
    }

    [Fact(DisplayName = "AuditableEntityConfiguration composite index is {TenantId, CompanyId, DataStatus}")]
    public void AuditableEntity_HasTenantCompanyDataStatusIndex()
    {
        using var context = CreateAuditableContext();
        var entityType = context.Model.FindEntityType(typeof(EntityCodeConfig))!;

        HasIndex(entityType, unique: false, "TenantId", "CompanyId", "DataStatus").Should().BeTrue();
        HasIndex(entityType, unique: false, "DataStatus", "Created").Should().BeFalse();
        HasIndex(entityType, unique: false, "DataStatus", "LastModified").Should().BeFalse();
    }

    [Fact(DisplayName = "WorkflowEnabledEntityConfiguration replaces the bare TenantId index with two tenant+company composites")]
    public void WorkflowEnabledEntity_HasTenantCompanyComposites()
    {
        using var context = CreateWorkflowContext();
        var entityType = context.Model.FindEntityType(typeof(TestWorkflowEntity))!;

        HasIndex(entityType, unique: false, "TenantId", "CompanyId", "DataStatus").Should().BeTrue();
        HasIndex(entityType, unique: false, "TenantId", "CompanyId", "WorkflowStatus").Should().BeTrue();
        HasIndex(entityType, unique: false, "TenantId").Should().BeFalse();

        // Untouched indexes from before this change must remain exactly as they were.
        HasIndex(entityType, unique: false, "DataStatus", "WorkflowStatus", "Created").Should().BeTrue();
        HasIndex(entityType, unique: false, "DataStatus", "WorkflowStatus", "LastModified").Should().BeTrue();
        HasIndex(entityType, unique: false, "WorkflowStatus", "Created").Should().BeTrue();
        HasIndex(entityType, unique: false, "WorkflowStatus", "LastModified").Should().BeTrue();
    }

    [Fact(DisplayName = "TenantPluginFlag keeps its {TenantId, PluginKey} unique index untouched")]
    public void TenantPluginFlag_KeepsOriginalUniqueIndex()
    {
        using var context = CreateAuditableContext();
        var entityType = context.Model.FindEntityType(typeof(TenantPluginFlag))!;

        HasIndex(entityType, unique: true, "TenantId", "PluginKey").Should().BeTrue();
    }
}
