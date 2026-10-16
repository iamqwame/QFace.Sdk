# Durable outbox (DATA-09) — module migration

`AddDbContextWithOutbox*` now registers a **real same-transaction outbox** plus an Akka relay
(poller → consistent-hash router → dispatch workers), matching QFace Data Standard DATA-09 /
Appendix 1.

Previously the name was a misnomer: `AuditEntitySaveChangesInterceptor` published via MediatR /
started Temporal **after** `SaveChanges`, and `OutboxMessage` was unused. That gap is closed in
the SDK. Modules must stop `Tell` / Temporal-start after commit and add the table migration.

## What the SDK does

1. **`IOutbox.Add(...)`** inserts one `OutboxMessages` row per configured transport on the current
   `DbContext` (same unit of work as the business change).
2. **Domain events** raised with `entity.AddDomainEvent(...)` are pulled during `SavingChanges`
   and written to the outbox (not published post-commit).
3. **Workflow approval** (`InProgress` transition) enqueues `WorkflowApprovalRequired` for the
   **temporal** transport; the relay calls `IWorkflowTriggerBridge` after commit.
4. **Relay** (`OutboxRelayHostedService`) claims rows with a lease, dispatches by `EntityId`, and
   marks `ProcessedAt` only after the transport confirms (retries / dead-letter on failure).

## Forbidden (DATA-09)

```csharp
await db.SaveChangesAsync(ct);
actorService.Tell(new SyncEmployee(...));          // NO — in-memory mailbox, lost on crash
await temporal.StartWorkflowAsync(...);            // NO — gap before Temporal accepts start
```

## Required pattern

```csharp
employee.Rename(cmd.FirstName, cmd.LastName);
employee.AddDomainEvent(new EmployeeChangedEvent(...)); // or outbox.Add(...)

// Optional explicit API (same transaction):
outbox.Add(new EmployeeChangedEvent(...), employee.Id, employee.Version);

await db.SaveChangesAsync(ct);
// Do not Tell / start Temporal here — the relay delivers.
return Result.Success();
```

## EF migration (every module DbContext)

`ApplicationDbContext` maps `DbSet<OutboxMessage> OutboxMessages`. Add a migration in each
module that owns a database:

```bash
dotnet ef migrations add AddOutboxMessages --project <Module>.Infrastructure --startup-project <Module>.WebApi
dotnet ef database update
```

Table shape (Postgres):

| Column | Type |
| --- | --- |
| Id | uuid PK (UUIDv7) |
| TenantId | text |
| EventType | text |
| Transport | text (`temporal` / `inprocess` / `rabbitmq` / `kafka`) |
| EntityId | uuid |
| EntityVersion | bigint |
| Payload | jsonb |
| OccurredAt / NextAttemptAt | timestamptz |
| LockedUntil / LockedBy | timestamptz / text |
| Attempts | int |
| LastError | text |
| ProcessedAt / DeadAt | timestamptz |

Index: ready rows on `NextAttemptAt` where `ProcessedAt` and `DeadAt` are null.

## Configuration

```json
{
  "Outbox": {
    "RelayEnabled": true,
    "PollInterval": "00:00:01",
    "BatchSize": 100,
    "DispatchWorkerCount": 8,
    "MaxAttempts": 10,
    "DefaultTransports": [ "inprocess" ],
    "Routing": {
      "EmployeeChangedEvent": [ "temporal" ],
      "LeaveApproved": [ "temporal" ],
      "WorkflowApprovalRequired": [ "temporal" ]
    },
    "Temporal": {
      "EmployeeChangedEvent": {
        "WorkflowType": "EmployeeSyncWorkflow",
        "TaskQueue": "qimerp-corehr-employee-sync"
      }
    }
  }
}
```

Notes:

- **Default transport is `inprocess`** so existing MediatR handlers keep working via the relay
  until modules opt into Temporal. Level **S** should set `DefaultTransports: ["temporal"]`
  (or per-event `Routing`) once workers exist.
- **`WorkflowApprovalRequired`** defaults to Temporal even without config.
- Set `Outbox:RelayEnabled=false` in unit-test hosts that should not start Akka.
- Register event CLR types for in-process deserialization when the type name is ambiguous:
  `services.AddOutboxEventType<EmployeeChangedEvent>();`
- RabbitMQ / Kafka transports are stubs until a decision log approves a bus (DATA-09); rows
  routed there will retry and eventually dead-letter.

## Package version

Shipped in **QimErp.Shared.Common 3.8.33+**. Bump module PackageReferences, then remove
post-save `Tell` / Temporal starts in Features.

## Compliance checklist

- [ ] Migration applied for `OutboxMessages`
- [ ] No `Tell` / `StartWorkflow` / `SignalWithStart` immediately after `SaveChanges` for
      cross-module hand-off
- [ ] Side effects use `AddDomainEvent` or `IOutbox.Add` before save
- [ ] Temporal routes configured for S-level sync events
- [ ] Relay left enabled in WebApi / worker hosts
