using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services.Workflow;
using QimErp.Shared.Common.Workflow.Enums;
using QimErp.Shared.Common.Workflow.Entities;
using Xunit;

namespace QimErp.Shared.Common.Tests.Workflow;

public class TestWorkflowEntity : WorkflowEnabledEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public TestOwnedDetails? Details { get; set; }

    public TestWorkflowEntity()
    {
        DisableWorkflowForSeeding();
        TenantId = "test-tenant";
    }
}

public class TestOwnedDetails
{
    public string Label { get; set; } = "";
}

public class TestWorkflowDbContext : DbContext
{
    public TestWorkflowDbContext(DbContextOptions<TestWorkflowDbContext> options) : base(options) { }

    public DbSet<TestWorkflowEntity> Entities => Set<TestWorkflowEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestWorkflowEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Ignore(e => e.CustomFields);
            entity.Ignore(e => e.DomainEvents);
            entity.OwnsOne(e => e.Details);
        });
    }
}

public class WorkflowFieldChangeDetectorTests
{
    [Fact]
    public void D1_ScalarModified_IncludesPropertyName()
    {
        var changed = DetectModified(e => e.Title = "Updated");
        changed.Should().Contain("Title");
    }

    [Fact]
    public void D2_WorkflowColumnsIgnored_AreExcluded()
    {
        var changed = DetectModified(e => e.WorkflowStatus = WorkflowStatus.InProgress);
        changed.Should().NotContain("WorkflowStatus");
        changed.Should().BeEmpty();
    }

    [Fact]
    public void D3_AddedEntity_IncludesNonNullScalars()
    {
        var changed = DetectAdded(new TestWorkflowEntity { Title = "New", Amount = 100 });
        changed.Should().Contain("Amount");
        changed.Should().Contain("Title");
    }

    [Fact]
    public void D4_OwnedNestedChange_IncludesParentKey()
    {
        var entity = new TestWorkflowEntity { Details = new TestOwnedDetails { Label = "A" } };
        var changed = DetectModified(entity, e => e.Details!.Label = "B");
        changed.Should().Contain("Details");
    }

    [Fact]
    public void D5_OnlyWorkflowStatusChange_DoesNotCountAsBusinessChange()
    {
        var changed = DetectModified(e =>
        {
            e.WorkflowStatus = WorkflowStatus.Approved;
            e.CurrentWorkflowHistoryId = Guid.NewGuid();
        });
        changed.Should().BeEmpty();
    }

    private static HashSet<string> DetectModified(Action<TestWorkflowEntity> mutate)
    {
        var entity = new TestWorkflowEntity { Title = "Original", Amount = 10 };
        return DetectModified(entity, mutate);
    }

    private static HashSet<string> DetectModified(TestWorkflowEntity entity, Action<TestWorkflowEntity> mutate)
    {
        using var context = CreateContext();
        context.Add(entity);
        context.SaveChanges();
        mutate(entity);
        context.Update(entity);
        var entry = context.Entry(entity);
        return WorkflowFieldChangeDetector.DetectChangedFields(entry);
    }

    private static HashSet<string> DetectAdded(TestWorkflowEntity entity)
    {
        using var context = CreateContext();
        context.Add(entity);
        var entry = context.Entry(entity);
        return WorkflowFieldChangeDetector.DetectChangedFields(entry);
    }

    private static TestWorkflowDbContext CreateContext()
    {
        var services = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder<TestWorkflowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseInternalServiceProvider(services)
            .Options;
        return new TestWorkflowDbContext(options);
    }
}

public class WorkflowRouteResolverTests
{
    [Fact]
    public void R1_SingleRouteMatch_ReturnsRouteCode()
    {
        var config = ConfigWithUpdateRoute("WF-A", ["Amount"]);
        var route = WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Amount"], StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity { Amount = 100 });

        route!.WorkflowCode.Should().Be("WF-A");
    }

    [Fact]
    public void R2_PriorityOrder_ReturnsFirstMatchingRoute()
    {
        var config = new EntityWorkflowConfig
        {
            EnableWorkflowForUpdate = true,
            UpdateWorkflowRoutes =
            [
                new WorkflowOperationRoute { Priority = 1, WorkflowCode = "WF-1", SignificantFields = ["Status"] },
                new WorkflowOperationRoute { Priority = 2, WorkflowCode = "WF-2", SignificantFields = ["Amount"] }
            ]
        };

        var route = WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Status", "Amount"], StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity());

        route!.WorkflowCode.Should().Be("WF-1");
    }

    [Fact]
    public void R3_SecondRouteWhenFirstMisses_ReturnsSecondRoute()
    {
        var config = new EntityWorkflowConfig
        {
            EnableWorkflowForUpdate = true,
            UpdateWorkflowRoutes =
            [
                new WorkflowOperationRoute { Priority = 1, WorkflowCode = "WF-1", SignificantFields = ["Status"] },
                new WorkflowOperationRoute { Priority = 2, WorkflowCode = "WF-2", SignificantFields = ["Amount"] }
            ]
        };

        var route = WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Amount"], StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity());

        route!.WorkflowCode.Should().Be("WF-2");
    }

    [Fact]
    public void R4_ConditionRequired_FailsWhenValueTooLow()
    {
        var config = ConfigWithUpdateRoute("WF-A", ["Amount"], conditionField: "Amount", op: WorkflowOperators.GreaterThan, value: "5000");
        var route = WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Amount"], StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity { Amount = 3000 });

        route.Should().BeNull();
    }

    [Fact]
    public void R5_ConditionPasses_ReturnsRoute()
    {
        var config = ConfigWithUpdateRoute("WF-A", ["Amount"], conditionField: "Amount", op: WorkflowOperators.GreaterThan, value: "5000");
        var route = WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Amount"], StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity { Amount = 6000 });

        route!.WorkflowCode.Should().Be("WF-A");
    }

    [Fact]
    public void R6_NoFieldOverlap_ReturnsNull()
    {
        var config = ConfigWithUpdateRoute("WF-A", ["Status"]);
        var route = WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Title"], StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity());

        route.Should().BeNull();
    }

    [Fact]
    public void R8_DeleteConditionsOnly_MatchesWithoutFields()
    {
        var config = new EntityWorkflowConfig
        {
            EnableWorkflowForDelete = true,
            DeleteWorkflowRoutes =
            [
                new WorkflowOperationRoute
                {
                    Priority = 1,
                    WorkflowCode = "DELETE-WF",
                    Conditions =
                    [
                        new WorkflowTriggerCondition
                        {
                            Field = "Amount",
                            Operator = WorkflowOperators.GreaterThan,
                            Value = "1000"
                        }
                    ]
                }
            ]
        };

        var route = WorkflowRouteResolver.ResolveDeleteRoute(
            config,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity { Amount = 2000 });

        route!.WorkflowCode.Should().Be("DELETE-WF");
    }

    [Fact]
    public void R10_EmptyRoutesList_ReturnsNull()
    {
        var config = new EntityWorkflowConfig { EnableWorkflowForUpdate = true, UpdateWorkflowRoutes = [] };
        var route = WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Amount"], StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity());

        route.Should().BeNull();
    }

    [Fact]
    public void D6_CaseInsensitiveFieldMatch_WorksInResolver()
    {
        var config = ConfigWithUpdateRoute("WF-A", ["title"]);
        var route = WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Title"], StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity { Title = "Changed" });

        route!.WorkflowCode.Should().Be("WF-A");
    }

    private static EntityWorkflowConfig ConfigWithUpdateRoute(
        string code,
        string[] fields,
        string? conditionField = null,
        WorkflowOperators op = WorkflowOperators.Equals,
        string value = "")
    {
        var route = new WorkflowOperationRoute
        {
            Priority = 1,
            WorkflowCode = code,
            SignificantFields = fields.ToList()
        };

        if (conditionField != null)
        {
            route.Conditions.Add(new WorkflowTriggerCondition
            {
                Field = conditionField,
                Operator = op,
                Value = value
            });
        }

        return new EntityWorkflowConfig
        {
            EnableWorkflowForUpdate = true,
            UpdateWorkflowRoutes = [route]
        };
    }
}

public class EntityWorkflowConfigLegacyMigratorTests
{
    [Fact]
    public void P5_LegacyJson_ConvertsToSingleUpdateAndDeleteRoutes()
    {
        const string json = """
            {
              "updateWorkflowCode": "legacy-update",
              "deleteWorkflowCode": "legacy-delete",
              "significantFieldsForUpdate": ["Amount", "Status"],
              "updateTriggerConditions": [{ "Field": "Amount", "Operator": "GreaterThan", "Value": "100" }]
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var config = EntityWorkflowConfigLegacyMigrator.Migrate(new EntityWorkflowConfig(), doc.RootElement);

        config.UpdateWorkflowRoutes.Should().ContainSingle();
        config.UpdateWorkflowRoutes[0].WorkflowCode.Should().Be("legacy-update");
        config.UpdateWorkflowRoutes[0].SignificantFields.Should().BeEquivalentTo(["Amount", "Status"]);
        config.DeleteWorkflowRoutes.Should().ContainSingle();
        config.DeleteWorkflowRoutes[0].WorkflowCode.Should().Be("legacy-delete");
    }
}
