using FluentAssertions;
using QimErp.Shared.Common.Services.Workflow;
using Xunit;

namespace QimErp.Shared.Common.Tests.Workflow;

public class WorkflowNotificationTemplatesTests
{
    [Theory]
    [InlineData("EmployeeLeaveRequest", "leave-approved")]
    [InlineData("LeaveRequest", "leave-approved")]
    [InlineData("SickLeave", "leave-approved")]
    [InlineData("MaternityLeave", "leave-approved")]
    [InlineData("Employee", "approval-approved")]
    [InlineData("EmployeeOrganizationalUnit", "approval-approved")]
    public void ApprovedForEntity_uses_leave_template_for_leave_entities(string entityType, string expected) =>
        WorkflowNotificationTemplates.ApprovedForEntity(entityType).Should().Be(expected);

    [Theory]
    [InlineData("EmployeeLeaveRequest", "leave-rejected")]
    [InlineData("LeaveRequest", "leave-rejected")]
    [InlineData("Employee", "approval-rejected")]
    public void RejectedForEntity_uses_leave_template_for_leave_entities(string entityType, string expected) =>
        WorkflowNotificationTemplates.RejectedForEntity(entityType).Should().Be(expected);

    [Fact]
    public void Temporal_template_codes_match_s3_kebab_case_files()
    {
        WorkflowNotificationTemplates.ApproverActionRequired.Should().Be("approval-request");
        WorkflowNotificationTemplates.WorkflowStarted.Should().Be("workflow-started");
        WorkflowNotificationTemplates.StepAdvanced.Should().Be("approval-stage-advanced");
        WorkflowNotificationTemplates.RejectionDefault.Should().Be("approval-rejected");
    }
}
