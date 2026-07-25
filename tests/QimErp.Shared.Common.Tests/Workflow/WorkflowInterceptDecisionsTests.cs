using FluentAssertions;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services.Workflow;
using QimErp.Shared.Common.Workflow.Enums;
using QimErp.Shared.Common.Workflow.Entities;
using Xunit;

namespace QimErp.Shared.Common.Tests.Workflow;

public class WorkflowInterceptDecisionsTests
{
    [Fact]
    public void U1_ApprovedStatus_AllowsUpdateWorkflow()
    {
        var entity = new TestWorkflowEntity { WorkflowStatus = WorkflowStatus.Approved };
        WorkflowInterceptDecisions.ShouldInitiateWorkflowOnUpdate(entity).Should().BeTrue();
    }

    [Fact]
    public void U2_RejectedStatus_AllowsUpdateWorkflow()
    {
        var entity = new TestWorkflowEntity { WorkflowStatus = WorkflowStatus.Rejected };
        WorkflowInterceptDecisions.ShouldInitiateWorkflowOnUpdate(entity).Should().BeTrue();
    }

    [Fact]
    public void U3_InProgressStatus_BlocksUpdateWorkflow()
    {
        var entity = new TestWorkflowEntity { WorkflowStatus = WorkflowStatus.InProgress };
        WorkflowInterceptDecisions.ShouldInitiateWorkflowOnUpdate(entity).Should().BeFalse();
        WorkflowInterceptDecisions.IsUpdateWorkflowBlockedByStatus(entity).Should().BeTrue();
    }

    [Fact]
    public void U4_NotStartedStatus_AllowsFirstUpdateWorkflow()
    {
        var entity = new TestWorkflowEntity { WorkflowStatus = WorkflowStatus.NotStarted };
        WorkflowInterceptDecisions.ShouldInitiateWorkflowOnUpdate(entity).Should().BeTrue();
    }

    [Fact]
    public void U5_ReturnedStatus_AllowsResubmitWorkflow()
    {
        var entity = new TestWorkflowEntity { WorkflowStatus = WorkflowStatus.Returned };
        WorkflowInterceptDecisions.ShouldInitiateWorkflowOnUpdate(entity).Should().BeTrue();
    }

    [Fact]
    public void C1_CreateEnabledWithCode_StartsWorkflow()
    {
        var config = new EntityWorkflowConfig
        {
            EnableWorkflowForCreate = true,
            CreateWorkflowCode = "CREATE-WF"
        };
        var entity = new TestWorkflowEntity();

        WorkflowInterceptDecisions.ShouldStartCreateWorkflow(config, entity).Should().BeTrue();
    }

    [Fact]
    public void C2_CreateDisabled_DoesNotStartWorkflow()
    {
        var config = new EntityWorkflowConfig { EnableWorkflowForCreate = false, CreateWorkflowCode = "CREATE-WF" };
        WorkflowInterceptDecisions.ShouldStartCreateWorkflow(config, new TestWorkflowEntity()).Should().BeFalse();
    }

    [Fact]
    public void C3_CreateTriggerConditionsFail_DoesNotStartWorkflow()
    {
        var config = new EntityWorkflowConfig
        {
            EnableWorkflowForCreate = true,
            CreateWorkflowCode = "CREATE-WF",
            CreateTriggerConditions =
            [
                new WorkflowTriggerCondition
                {
                    Field = nameof(TestWorkflowEntity.Amount),
                    Operator = WorkflowOperators.GreaterThan,
                    Value = "5000"
                }
            ]
        };
        var entity = new TestWorkflowEntity { Amount = 100 };

        WorkflowInterceptDecisions.ShouldStartCreateWorkflow(config, entity).Should().BeFalse();
    }

    [Fact]
    public void C4_CreateTriggerConditionsPass_StartsWorkflow()
    {
        var config = new EntityWorkflowConfig
        {
            EnableWorkflowForCreate = true,
            CreateWorkflowCode = "CREATE-WF",
            CreateTriggerConditions =
            [
                new WorkflowTriggerCondition
                {
                    Field = nameof(TestWorkflowEntity.Amount),
                    Operator = WorkflowOperators.GreaterThan,
                    Value = "5000"
                }
            ]
        };
        var entity = new TestWorkflowEntity { Amount = 6000 };

        WorkflowInterceptDecisions.ShouldStartCreateWorkflow(config, entity).Should().BeTrue();
    }

    [Fact]
    public void R7_UpdateDisabled_ResolverReturnsNullEvenWithMatchingFields()
    {
        var config = new EntityWorkflowConfig
        {
            EnableWorkflowForUpdate = false,
            UpdateWorkflowRoutes =
            [
                new WorkflowOperationRoute
                {
                    Priority = 1,
                    WorkflowCode = "WF-A",
                    SignificantFields = ["Amount"]
                }
            ]
        };

        WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Amount"], StringComparer.OrdinalIgnoreCase),
            new TestWorkflowEntity()).Should().BeNull();
    }

    [Fact]
    public void R9_MultipleConditionsOnRoute_RequireAllToPass()
    {
        var config = new EntityWorkflowConfig
        {
            EnableWorkflowForUpdate = true,
            UpdateWorkflowRoutes =
            [
                new WorkflowOperationRoute
                {
                    Priority = 1,
                    WorkflowCode = "WF-A",
                    SignificantFields = ["Amount"],
                    Conditions =
                    [
                        new WorkflowTriggerCondition
                        {
                            Field = nameof(TestWorkflowEntity.Amount),
                            Operator = WorkflowOperators.GreaterThan,
                            Value = "1000"
                        },
                        new WorkflowTriggerCondition
                        {
                            Field = nameof(TestWorkflowEntity.Status),
                            Operator = WorkflowOperators.Equals,
                            Value = "Active"
                        }
                    ]
                }
            ]
        };

        var passEntity = new TestWorkflowEntity { Amount = 2000, Status = "Active" };
        var failEntity = new TestWorkflowEntity { Amount = 2000, Status = "Draft" };

        WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Amount"], StringComparer.OrdinalIgnoreCase),
            passEntity)!.WorkflowCode.Should().Be("WF-A");

        WorkflowRouteResolver.ResolveUpdateRoute(
            config,
            new HashSet<string>(["Amount"], StringComparer.OrdinalIgnoreCase),
            failEntity).Should().BeNull();
    }
}
