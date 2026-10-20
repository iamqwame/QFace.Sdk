using FluentAssertions;
using QimErp.Shared.Common.Activities.Workflow;
using QimErp.Shared.Common.Workflow.Entities;
using Xunit;

namespace QimErp.Shared.Common.Tests.Workflow;

public class ProfilePictureFileIdDtoTests
{
    [Fact]
    public void ResolvedApprover_exposes_ProfilePictureFileId()
    {
        var fileId = Guid.CreateVersion7();
        var approver = new ResolvedApprover { ProfilePictureFileId = fileId };

        approver.ProfilePictureFileId.Should().Be(fileId);
        typeof(ResolvedApprover).GetProperty(nameof(ResolvedApprover.ProfilePictureFileId))
            .Should().NotBeNull();
        typeof(ResolvedApprover).GetProperty(nameof(ResolvedApprover.ProfilePictureUrl))
            .Should().NotBeNull();
    }

    [Fact]
    public void WorkflowActorChangedEvent_exposes_ProfilePictureFileId()
    {
        var fileId = Guid.CreateVersion7();
        var changed = new WorkflowActorChangedEvent { ProfilePictureFileId = fileId };

        changed.ProfilePictureFileId.Should().Be(fileId);
        typeof(WorkflowActorChangedEvent).GetProperty(nameof(WorkflowActorChangedEvent.ProfilePictureFileId))
            .Should().NotBeNull();
        typeof(WorkflowActorChangedEvent).GetProperty(nameof(WorkflowActorChangedEvent.ProfilePictureUrl))
            .Should().NotBeNull();
    }
}
