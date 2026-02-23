# QimErp.Shared.Common Configuration (Phase 1)

This document describes the configurable options for FrontendSettings, System identity, and RabbitMQ exchanges.

## Registration

Options are automatically registered when you call:

- `AddCoreServices(services, configuration, assembly)` — registers all Phase 1 options from configuration
- `AddDbContextWithOutbox<TContext>(services, connectionString, configuration?)` — always registers options; when `configuration` is provided, binds from config; when null, uses default values
- `AddDbContextWithOutboxConsumer<TContext>(services, connectionString, configuration?)` — always registers options; when `configuration` is provided, binds from config; when null, uses default values

You can also register manually:

```csharp
services.AddQimErpConfiguration(configuration);
```

## Configuration Schema

Add these sections to `appsettings.json` (or use environment variables). Reference templates are in [Config/](Config/).

```json
{
  "FrontendSettings": {
    "BaseUrl": "https://app.qimerp.com",
    "ActivationPath": "/auth/activate",
    "ResetPasswordPath": "/auth/reset-password",
    "LoginPath": "/auth/login"
  },
  "System": {
    "DefaultUserId": "system",
    "DefaultUserName": "System",
    "DefaultSystemEmail": "system@qimerp.com",
    "ConsumerSystemEmail": "system@consumer",
    "DefaultNextStepName": "Final Review",
    "DefaultRequesterName": "Requester",
    "DefaultApproverName": "Approver",
    "DefaultWorkflowCodeDisplayName": "Workflow Request"
  },
  "RabbitMq": {
    "Exchanges": {
      "Notify": "qimerp.core.notify.prod_exchange",
      "WorkflowApprovalRequired": "qimerp.workflow.workflow_approval_required.prod_exchange",
      "WorkflowChanged": "qimerp.workflow.workflow_changed.prod_exchange",
      "WorkflowStatusChanged": "qimerp.workflow.workflow_status_changed.prod_exchange",
      "WorkflowCompleted": "qimerp.workflow.workflow_completed.prod_exchange",
      "WorkflowApprovalRequest": "qimerp.workflow.workflow_approval_request.local_exchange"
    }
  }
}
```

## Environment Variables

Override any value using the `__` (double underscore) separator:

- `FrontendSettings__BaseUrl`
- `System__DefaultSystemEmail`
- `RabbitMq__Exchanges__Notify`
- `RabbitMq__Exchanges__WorkflowApprovalRequired`
- `RabbitMq__Exchanges__WorkflowChanged`
- `RabbitMq__Exchanges__WorkflowStatusChanged`
- `RabbitMq__Exchanges__WorkflowCompleted`
- `RabbitMq__Exchanges__WorkflowApprovalRequest`

## Environment-Specific Overrides

- **Production:** Use prod_exchange defaults.
- **Development/Staging:** Override in `appsettings.Development.json`:

  ```json
  {
    "RabbitMq": {
      "Exchanges": {
        "Notify": "qimerp.core.notify.dev_exchange",
        "WorkflowApprovalRequired": "qimerp.workflow.workflow_approval_required.dev_exchange",
        "WorkflowChanged": "qimerp.workflow.workflow_changed.dev_exchange",
        "WorkflowStatusChanged": "qimerp.workflow.workflow_status_changed.dev_exchange",
        "WorkflowCompleted": "qimerp.workflow.workflow_completed.dev_exchange",
        "WorkflowApprovalRequest": "qimerp.workflow.workflow_approval_request.local_exchange"
      }
    }
  }
  ```

## Migration from Previous Versions

1. **No breaking changes for default behavior** — If you omit these sections, property defaults are used.
2. **Consumer apps using `AddDbContextWithOutboxConsumer`** — Passing `configuration` is optional. When omitted, defaults apply; when provided, options bind from config:

   ```csharp
   // With configuration (custom values from appsettings)
   services.AddDbContextWithOutboxConsumer<MyContext>(connectionString, configuration);

   // Without configuration (default values)
   services.AddDbContextWithOutboxConsumer<MyContext>(connectionString);
   ```

3. **API apps using `AddDbContextWithOutbox`** — Similarly, `configuration` is optional:

   ```csharp
   services.AddDbContextWithOutbox<MyContext>(connectionString, configuration); // optional config
   ```

4. **Workflow-enabled apps** — Already covered when using `AddCoreServices`; no changes required.

## Troubleshooting

- **Options not registered / InvalidOperationException** — If you see errors resolving `IOptions<RabbitMqOptions>` or `IOptions<SystemOptions>`, ensure you are using a recent version that includes `AddQimErpConfigurationWithDefaults`. Both `AddDbContextWithOutbox` and `AddDbContextWithOutboxConsumer` now always register options (either from config or defaults). If using an older setup, pass `configuration` to `AddDbContextWithOutboxConsumer` or call `services.AddQimErpConfiguration(configuration)` manually.
