# QimErp.Shared.Common Configuration Templates

These files are **reference templates** for consuming applications. Copy the relevant sections into your application's `appsettings.json` (and environment-specific overrides).

## Files

| File | Purpose |
|------|---------|
| `appsettings.json` | Base configuration (production defaults) |
| `appsettings.Development.json` | Development overrides (dev exchanges, dev frontend URL) |
| `appsettings.Staging.json` | Staging overrides |
| `appsettings.Production.json` | Explicit production values |
| `.env.example` | Environment variables (production defaults) |
| `.env.Development.example` | Development env overrides |
| `.env.Staging.example` | Staging env overrides |
| `.env.Production.example` | Production env overrides |

## Environment Variables

Copy `.env.example` to `.env` and override as needed. Use `__` (double underscore) for nested keys:

```
FrontendSettings__BaseUrl=https://custom.qimerp.com
System__DefaultSystemEmail=admin@mycompany.com
RabbitMq__NotificationsExchange=qimerp.core.notify.dev_exchange
RabbitMq__WorkflowApprovalRequiredExchange=qimerp.workflow.workflow_approval_required.dev_exchange
RabbitMq__WorkflowChangedExchange=qimerp.workflow.workflow_changed.dev_exchange
RabbitMq__WorkflowStatusChangedExchange=qimerp.workflow.workflow_status_changed.dev_exchange
RabbitMq__WorkflowCompletedExchange=qimerp.workflow.workflow_completed.dev_exchange
```
