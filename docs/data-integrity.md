# QimErp.Shared.DataIntegrity (DATA-06)

Nightly loose-link integrity checks. A loose link is a reference column without a foreign key that
points at a table in the same database (including local-copy tables). Links are discovered from each
registered DbContext's EF Core model; every link is checked once a night, per tenant, for orphans,
tenant mismatches and soft-deleted targets. Local-copy tables with a `SourceVersion` column are also
checked for unversioned rows.

The package does not reference `QimErp.Shared.Common` or Npgsql. It depends on
`Microsoft.EntityFrameworkCore.Relational` 9.0.0, `QFace.Sdk.Temporal` 1.0.10 and
`OpenTelemetry.Api.ProviderBuilderExtensions` 1.9.0.

## Registration

```csharp
using QimErp.Shared.DataIntegrity;

builder.Services.AddDataIntegrityChecks(configuration, "payroll",
    c => c.AddContext<PayrollApplicationDbContext>(PayrollDataIntegrityLinks.Configure));

var app = builder.Build();
if (app.RunDataIntegrityLinkReportIfRequested(args)) return;
```

- `moduleKey` must match `^[a-z][a-z0-9]*(-[a-z0-9]+)*$` (max 40 characters). Calling
  `AddDataIntegrityChecks` twice throws `InvalidOperationException`.
- When `Temporal:Address` is empty nothing Temporal-related is registered; the printer still works.
- When Temporal is configured the call registers:
  - a worker on task queue `qimerp-{moduleKey}-data-integrity` plus `Temporal:TaskQueueSuffix`
    (one activity at a time, two workflow tasks);
  - a schedule registrar that creates or updates schedule
    `qimerp-{moduleKey}-data-integrity-nightly{suffix}` (cron UTC, jitter, `Overlap=Skip`,
    catch-up window 2 h). The schedule starts workflow `DataIntegrityWorkflow`, which runs
    activity `RunIntegrityChecks:DataIntegrity`.
- `DataIntegrity:Enabled=false` registers no worker and pauses the schedule.
- The meters `QimErp.Shared.DataIntegrity` and `QimErp.Shared.Common.Outbox` are added to the host's
  OpenTelemetry MeterProvider. They are exported only when `OTEL_EXPORTER_OTLP_ENDPOINT` is set.

Register a context in one host only. Hosts that hold a second handle to another service's database
must not register that context.

## Options (`DataIntegrity` section)

| Key | Default | Range |
|---|---|---|
| Enabled | true | |
| Cron | `0 2 * * *` | non-empty |
| JitterMinutes | 15 | 0–120 |
| StatementTimeoutSeconds | 120 | 5–3600 |
| LockTimeoutSeconds | 5 | 1–60 |
| BatchThresholdRows | 250000 | ≥ 1000 |
| BatchSize | 100000 | 1000–1000000 |
| SampleSize | 10 | 1–50 |
| MaxTenantsLoggedPerCheck | 50 | 1–1000 |
| MaxRunMinutes | 120 | 10–600 |
| ScheduleStartupDelaySeconds | 30 | 0–600 |
| PrintLinks | false | |

## Overrides

```csharp
public static class PayrollDataIntegrityLinks
{
    public static void Configure(DataIntegrityLinkOptions links) => links
        .Map("Projects.TypeId", "ProjectType")
        .Exclude("AssignmentHistoryRecords.AssignmentId", "polymorphic")
        .Alias("User", "UserIdentity");
}
```

- `Map(tableColumn, targetEntity)` forces a target. The key is `Table.Column`, or
  `schema.Table.Column` when the table has a schema (the two-part form is accepted when the table
  name is unique). The target is the CLR short name, or the full name when the short name is
  ambiguous.
- `Exclude(tableColumn, reason)` skips a column. Reason convention:
  `cross-service: <Module>.<Entity>`, `polymorphic`, `external`, `business-key`.
- `Alias(idStem, targetEntity)` applies to every stem lookup in the context.
- Every key must name a table column in the model and every target must be an entity with a table
  and a single-column primary key. Otherwise `DataIntegrityConfigurationException` lists all
  problems: the printer exits with code 1 and the activity fails without retrying. The host still
  starts.

## Discovery rules

Targets are non-owned entity types with a table (no view, SQL query or function mapping), a
single-column primary key and a TPH or default mapping strategy. Derived TPH types use the root
table and key.

Candidate columns are walked on every eligible non-owned entity and its owned navigations
(table-split and separate-table owned types). A column is a candidate when its name ends with `Id`
(longer than two characters), or when it is the `Id` of an owned type whose CLR name ends with
`ValueObject`. Primary-key, foreign-key and shadow properties, arrays, and types other than
uuid/text/integer are excluded with a reason, as are JSON-mapped owned types, `EntityReference`
owned types, keyless and view-mapped types, and SDK infrastructure entities.

Name candidates then pass through fixed exclusions (infrastructure, audit, blob, external,
workflow-engine and polymorphic names) and are resolved in order: `Map` override, value object,
parent, exact or suffix match (with plural and alias lookups), role alias to `Employee`, and
ends-with. Ends-with matches are low confidence: they are reported but **not checked** until the
owner confirms them with `Map`. A match that points back at its own table without a qualifier
(`Parent`, `Source`, `Previous`, ...) is excluded as a self-named key. A uuid column against an
integer key, or the reverse, is excluded as type-incompatible.

Check names are `{moduleKey}[.{contextKey}].{table_snake}.{column_snake}_{kind}` with kind
`orphan`, `tenant_mismatch` (both sides have a string `TenantId`) or `deleted_target` (the target
has a `DataStatus` enum with a `Deleted` member). Local copies get
`{moduleKey}.{table_snake}.source_version_unversioned`. The context key is added only when the host
registers more than one context.

## Printing the inventory

```bash
dotnet run --project <Host> -- --data-integrity-links > docs/data-integrity/<moduleKey>.md
dotnet run --project <Host> -- --data-integrity-links --sql
```

`DataIntegrity:PrintLinks=true` has the same effect as the flag. The printer builds the host, reads
each registered DbContext's model and writes Markdown to stdout. It does not open a database
connection or contact Temporal. The output is deterministic; commit it and regenerate it when the
model changes. It lists checked links, local-copy checks, unindexed checked links (DATA-26),
low-confidence matches, unverifiable and unresolved columns, ambiguous columns and exclusions.

## Execution

- Only the Npgsql provider is executed; other providers are skipped with
  `reason=unsupported_provider`.
- Every batch runs in its own `READ ONLY` transaction with `statement_timeout` and `lock_timeout`
  set through `set_config`. Tables with more than `BatchThresholdRows` estimated rows (or never
  analysed) are scanned in primary-key ranges of `BatchSize`.
- The SQL reads root and target keys, ownership key columns, the link column, `TenantId`,
  `DataStatus`, `IsGlobal` and `SourceVersion` only. Identifiers come from the model and are quoted
  by the provider; all values are parameters.
- The checks read across tenants on purpose. A tenant-scoped run cannot see other tenants' rows, so
  it would misreport mismatches as orphans, and an unscoped EF query sees only global rows.
- Timeouts and missing tables or columns mark one check `TimedOut` or `Failed` and the run
  continues. Lost connections fail the activity, which retries from the last heartbeat. When the run
  exceeds `MaxRunMinutes` the remaining checks are skipped. When every executed check fails the
  activity fails without retrying.

## Signals

Logs (the signal that works without an OTLP collector):

- `data_integrity_found check tenantId count sampleIds runId` (Warning, top
  `MaxTenantsLoggedPerCheck` tenants by count; `partial=true` on timed-out or failed checks)
- `data_integrity_found_more check additionalTenants additionalCount runId`
- `data_integrity_check check status count durationMs runId`
- `data_integrity_check_failed check status reason runId`
- `data_integrity_run module context checks withFindings failed timedOut skipped totalFindings durationSeconds runId`
- `data_integrity_discovered module context links unindexed unresolved ambiguous excluded` (once per
  context after the schedule is reconciled)

Metrics (meter `QimErp.Shared.DataIntegrity`): `data_integrity_found{check}`,
`data_integrity_check_failures{check,reason}`, `data_integrity_run_duration_seconds{module}`,
`data_integrity_last_run_unixtime{module}`. Tenant ids are never metric tags. Sample ids are root
primary keys only; reference values and row contents are never logged.

## Extension points

- `IIntegrityResultSink` receives every outcome. A future IntegrityRuns table is one more sink:
  `IntegrityRunInfo` and `IntegrityFinding` carry RunId, RunAt, TenantId, Check, Count and SampleIds.
- `DataIntegrityBuilder.AddCheck<TCheck>()` registers a custom `IDataIntegrityCheck`, run after the
  discovered links with kind `Custom`.

## Relationship to `AddIntegrityCheckRunner`

The Shared.Common timer runner and the outbox backstop check are unchanged. Loose-link checks never
run through it.
