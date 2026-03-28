using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Temporal workflow interface for all notification delivery.
/// Implemented by <c>NotificationWorkflow</c> in QimErp.Platform.Notifications.WebApi.
///
/// Callers should never reference this interface directly — use
/// <see cref="QimErp.Shared.Common.Services.Notifications.INotificationWorkflowStarter"/>
/// which provides named convenience methods and hides the task queue.
/// </summary>
public interface INotificationWorkflow
{
    Task RunAsync(UnifiedMessageModel model);
}
