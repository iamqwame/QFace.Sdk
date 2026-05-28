using FluentAssertions;
using QimErp.Shared.Common.Services.Workflow;
using QimErp.Shared.Common.Services.Workflow.Temporal;
using QimErp.Shared.Common.Workflow.Enums;
using Xunit;

namespace QimErp.Shared.Common.Tests.Workflow;

public class WorkflowLifecycleContractsTests
{
    private sealed class TestWorkflowEntity : IWorkflowEnabled, IWorkflowSubjectContextProvider, IWorkflowDeferredActivation
    {
        public string EntityType => "LeaveRequest";
        public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.InProgress;
        public Guid? CurrentWorkflowHistoryId { get; set; }
        public string? WorkflowCode { get; set; }
        public string? WorkflowComments { get; set; }
        public DateTime? WorkflowInitiatedAt { get; set; }
        public string? WorkflowInitiatedByEmail { get; set; }
        public string? WorkflowInitiatedByEmployeeId { get; set; }
        public string? WorkflowInitiatedByName { get; set; }
        public string? WorkflowCompletedByEmail { get; set; }
        public string? WorkflowCompletedByEmployeeId { get; set; }
        public string? WorkflowCompletedByName { get; set; }
        public string? WorkflowRejectionReason { get; set; }
        public DateTime? WorkflowCompletedAt { get; set; }
        public bool IsWorkflowEnabled => true;
        public bool IsActive => WorkflowStatus == WorkflowStatus.Approved;
        public bool IsPendingApproval => WorkflowStatus == WorkflowStatus.InProgress;
        public bool IsRejected => WorkflowStatus == WorkflowStatus.Rejected;
        public bool IsWorkflowComplete => WorkflowStatus is WorkflowStatus.Approved or WorkflowStatus.Rejected;
        public bool CanBeEdited() => !IsPendingApproval;
        public bool CanBeDeleted() => !IsPendingApproval;
        public string? GetWorkflowSubjectContextType() => "Employee";
        public string? GetWorkflowSubjectContextId() => "11111111-1111-1111-1111-111111111111";
        public bool IsEffective => WorkflowEffectiveStatus.IsEffective(this);
    }

    [Theory]
    [InlineData(WorkflowStatus.Approved, true)]
    [InlineData(WorkflowStatus.NotStarted, true)]
    [InlineData(WorkflowStatus.InProgress, false)]
    [InlineData(WorkflowStatus.Rejected, false)]
    public void WorkflowEffectiveStatus_IsEffective_reflects_workflow_status(
        WorkflowStatus status, bool expected)
    {
        var entity = new TestWorkflowEntity { WorkflowStatus = status };
        WorkflowEffectiveStatus.IsEffective(entity).Should().Be(expected);
    }

    [Fact]
    public void WorkflowSubjectContext_uses_provider_context_for_approver_resolution()
    {
        var entity = new TestWorkflowEntity();
        var context = WorkflowSubjectContext.FromEntity(entity);

        context.ContextType.Should().Be("Employee");
        context.ContextId.Should().Be("11111111-1111-1111-1111-111111111111");
        context.ResolveApproverSubjectId("child-id", "LeaveRequest").Should().Be(context.ContextId);
    }

    [Fact]
    public void WorkflowSubjectContext_falls_back_to_entity_id_when_no_provider()
    {
        var entity = new PlainWorkflowEntity { Id = Guid.Parse("22222222-2222-2222-2222-222222222222") };
        var context = WorkflowSubjectContext.FromEntity(entity);

        context.ResolveApproverSubjectId(entity.Id.ToString(), entity.EntityType)
            .Should().Be(entity.Id.ToString());
    }

    [Fact]
    public void WorkflowEntityApprovalHandlerRegistry_resolves_by_entity_type()
    {
        var registry = new WorkflowEntityApprovalHandlerRegistry(
        [
            new StubHandler("EmployeeOrganizationalUnit"),
            new StubHandler("LeaveRequest")
        ]);

        registry.GetHandler("employeeorganizationalunit")!.EntityType.Should().Be("EmployeeOrganizationalUnit");
        registry.GetHandler("Unknown").Should().BeNull();
    }

    [Fact]
    public void WorkflowNotificationRecipientResolver_extracts_initiator_and_literals()
    {
        var input = new ApprovalWorkflowInput
        {
            InitiatedBy = "requester@example.com",
            InitiatedByName = "Requester"
        };

        WorkflowNotificationRecipientResolver.IsInitiatorToken("Initiator").Should().BeTrue();
        WorkflowNotificationRecipientResolver.ExtractLiteralEmails(["Initiator", "hr@example.com"])
            .Should().ContainSingle("hr@example.com");
        WorkflowNotificationRecipientResolver.ResolveInitiator(input)!.Email
            .Should().Be("requester@example.com");
    }

    private sealed class PlainWorkflowEntity : IWorkflowEnabled
    {
        public Guid Id { get; init; } = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public string EntityType => "Vendor";
        public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.NotStarted;
        public Guid? CurrentWorkflowHistoryId { get; set; }
        public string? WorkflowCode { get; set; }
        public string? WorkflowComments { get; set; }
        public DateTime? WorkflowInitiatedAt { get; set; }
        public string? WorkflowInitiatedByEmail { get; set; }
        public string? WorkflowInitiatedByEmployeeId { get; set; }
        public string? WorkflowInitiatedByName { get; set; }
        public string? WorkflowCompletedByEmail { get; set; }
        public string? WorkflowCompletedByEmployeeId { get; set; }
        public string? WorkflowCompletedByName { get; set; }
        public string? WorkflowRejectionReason { get; set; }
        public DateTime? WorkflowCompletedAt { get; set; }
        public bool IsWorkflowEnabled => true;
        public bool IsActive => WorkflowStatus == WorkflowStatus.Approved;
        public bool IsPendingApproval => WorkflowStatus == WorkflowStatus.InProgress;
        public bool IsRejected => WorkflowStatus == WorkflowStatus.Rejected;
        public bool IsWorkflowComplete => WorkflowStatus is WorkflowStatus.Approved or WorkflowStatus.Rejected;
        public bool CanBeEdited() => !IsPendingApproval;
        public bool CanBeDeleted() => !IsPendingApproval;
    }

    private sealed class StubHandler(string entityType) : IWorkflowEntityApprovalHandler
    {
        public string EntityType { get; } = entityType;
        public Task OnAdvanceAsync(ApprovalWorkflowInput input, QimErp.Shared.Common.Workflow.Entities.WorkflowStep approvedStep, QimErp.Shared.Common.Workflow.Entities.WorkflowStep nextStep, ApprovalSignal signal, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task OnFinalizeAsync(ApprovalWorkflowInput input, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task OnRejectAsync(ApprovalWorkflowInput input, QimErp.Shared.Common.Workflow.Entities.WorkflowStep rejectedAtStep, ApprovalSignal signal, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task OnTimeoutAsync(ApprovalWorkflowInput input, QimErp.Shared.Common.Workflow.Entities.WorkflowStep timedOutStep, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
