using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using QFace.Sdk.Temporal.Abstractions;
using QimErp.Shared.Common.Events;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.Notifications;
using Xunit;

namespace QimErp.Shared.Common.Tests.Notifications;

public class NotificationWorkflowStarterTests
{
    private const string TestTenantId = "tenant-test";

    [Fact]
    public async Task SendAsync_WithoutMessageId_ProducesDistinctWorkflowIds()
    {
        var capturing = new CapturingWorkflowStarter();
        var starter = CreateStarter(capturing);

        await starter.SendAsync(new UnifiedMessageModel
        {
            MessageType = "simple_email",
            ToEmail = "a@example.com",
            TenantId = TestTenantId,
        });
        await starter.SendAsync(new UnifiedMessageModel
        {
            MessageType = "simple_email",
            ToEmail = "b@example.com",
            TenantId = TestTenantId,
        });

        capturing.WorkflowIds.Should().HaveCount(2);
        capturing.WorkflowIds[0].Should().StartWith("notification-");
        capturing.WorkflowIds[1].Should().StartWith("notification-");
        capturing.WorkflowIds[0].Should().NotBe(capturing.WorkflowIds[1]);
        capturing.WorkflowIds[0].Should().NotBe("notification-");
        capturing.WorkflowIds[1].Should().NotBe("notification-");
    }

    [Fact]
    public async Task SendAsync_WithExplicitMessageId_PreservesWorkflowId()
    {
        var capturing = new CapturingWorkflowStarter();
        var starter = CreateStarter(capturing);
        const string messageId = "explicit-message-id";

        await starter.SendAsync(new UnifiedMessageModel
        {
            MessageType = "simple_email",
            ToEmail = "a@example.com",
            MessageId = messageId,
            TenantId = TestTenantId,
        });

        capturing.WorkflowIds.Should().ContainSingle().Which.Should().Be($"notification-{messageId}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendAsync_WithEmptyOrWhitespaceMessageId_AssignsNewId(string messageId)
    {
        var capturing = new CapturingWorkflowStarter();
        var starter = CreateStarter(capturing);
        var model = new UnifiedMessageModel
        {
            MessageType = "simple_email",
            ToEmail = "a@example.com",
            MessageId = messageId,
            TenantId = TestTenantId,
        };

        await starter.SendAsync(model);

        model.MessageId.Should().NotBeNullOrWhiteSpace();
        capturing.WorkflowIds.Should().ContainSingle().Which.Should().Be($"notification-{model.MessageId}");
    }

    [Fact]
    public async Task SendAsync_WithoutAmbientTenantAndWithoutModelTenantId_Throws()
    {
        var capturing = new CapturingWorkflowStarter();
        var starter = CreateStarter(capturing);

        var act = () => starter.SendAsync(new UnifiedMessageModel
        {
            MessageType = "simple_email",
            ToEmail = "a@example.com",
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*TenantId*");
        capturing.WorkflowIds.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendAsync_WithWhitespaceModelTenantIdAndNoAmbient_Throws(string tenantId)
    {
        var capturing = new CapturingWorkflowStarter();
        var starter = CreateStarter(capturing);

        var act = () => starter.SendAsync(new UnifiedMessageModel
        {
            MessageType = "simple_email",
            ToEmail = "a@example.com",
            TenantId = tenantId,
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*TenantId*");
        capturing.WorkflowIds.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAsync_WithModelTenantId_Proceeds()
    {
        var capturing = new CapturingWorkflowStarter();
        var starter = CreateStarter(capturing);
        var model = new UnifiedMessageModel
        {
            MessageType = "simple_email",
            ToEmail = "a@example.com",
            TenantId = TestTenantId,
        };

        await starter.SendAsync(model);

        model.TenantId.Should().Be(TestTenantId);
        capturing.WorkflowIds.Should().ContainSingle().Which.Should().StartWith("notification-");
    }

    [Fact]
    public async Task SendAsync_WithoutModelTenantId_StampsFromAmbientAndProceeds()
    {
        var capturing = new CapturingWorkflowStarter();
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(TestTenantId);
        var starter = CreateStarter(capturing, tenantContext);
        var model = new UnifiedMessageModel
        {
            MessageType = "simple_email",
            ToEmail = "a@example.com",
        };

        await starter.SendAsync(model);

        model.TenantId.Should().Be(TestTenantId);
        capturing.WorkflowIds.Should().ContainSingle().Which.Should().StartWith("notification-");
    }

    private static NotificationWorkflowStarter CreateStarter(
        IWorkflowStarter workflowStarter,
        ITenantContext? tenantContext = null)
    {
        var configuration = new ConfigurationBuilder().Build();
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(ITenantContext))).Returns(tenantContext);
        scope.Setup(s => s.ServiceProvider).Returns(sp.Object);
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        return new NotificationWorkflowStarter(workflowStarter, configuration, scopeFactory.Object);
    }

    private sealed class CapturingWorkflowStarter : IWorkflowStarter
    {
        public List<string> WorkflowIds { get; } = [];

        public Task<WorkflowStartResult> StartOrIgnoreAsync<TWorkflow>(
            string workflowId,
            string taskQueue,
            Expression<Func<TWorkflow, Task>> startExpression,
            CancellationToken cancellationToken = default)
        {
            WorkflowIds.Add(workflowId);
            return Task.FromResult(new WorkflowStartResult
            {
                WorkflowId = workflowId,
                RunId = "run",
            });
        }

        public Task<WorkflowStartResult> StartOrRaiseAsync<TWorkflow>(
            string workflowId,
            string taskQueue,
            Expression<Func<TWorkflow, Task>> startExpression,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<WorkflowStartResult> SignalWithStartAsync<TWorkflow>(
            string workflowId,
            string taskQueue,
            Expression<Func<TWorkflow, Task>> startExpression,
            Expression<Func<TWorkflow, Task>> signalExpression,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
