# Multi-Company — Implementation Status

SDK **3.8.2** / QFace.Sdk.Temporal **1.0.10**. All ten repositories on `development`.

## Done

| Phase | What landed |
|---|---|
| 0 | 34 package references across 10 SDK versions consolidated to one, via per-solution `Directory.Packages.props` (CPM) |
| 1 | SQL gate passed — see `phase-1-sql-gate.md`. `UseQimErpNpgsql` pins collection parameterisation on all 59 call sites |
| 1.5 | Classification inventory — see `classification.md`. **Not signed off by a module owner** |
| 2 | `CompanyId` on `AuditableEntity`, model-finalizing filter convention, `ICompanyContext`, write stamping + guards, HTTP/Temporal transport, employee home company + visibility, settings override, per-company numbering |
| 3 | 23 migrations, one per context. `CompanyId` is `NOT NULL DEFAULT ''` |
| 4 | `Company`, `UserCompanyAccess`, `CompanyAccessMode`, `MultiCompanyEnabled`, token claims, CRUD endpoints, enablement-with-backfill workflow |
| 5 | `CompanyProvider` (cookie-backed), company headers on all four request paths, four-way cache purge on switch, sidebar switcher, top-bar name, `CompanySelect` |

Two permanent guards run in every module test project: no auditable entity may have a
null query filter, and every filter must reference `CompanyId` unless the entity is
tenant-wide. **All guards pass** — no module has an unscoped entity.

## Not done

- **Phase 1.5 sign-off.** Payroll and Accounting owners have not reviewed the classification.
  171 of 174 unique indexes were widened on a mechanical default, not a reviewed decision.
- **`KnowledgeChunk` scope was a judgement call**, not a product decision: is a tenant's RAG
  corpus shared across companies or isolated per company? Defaulted to isolated (widened).
- **Indexes ship inside `AddCompanyId`, not as separate `CONCURRENTLY` migrations.** Fine for
  development; a production rollout wants the index work split out with
  `CREATE INDEX CONCURRENTLY` + `suppressTransaction: true`, since EF wraps migrations in a
  transaction and these touch every auditable table.
- **No tenant has been enabled.** `MultiCompanyEnabled` is false everywhere, so behaviour is
  byte-identical to before this work. Nothing has been exercised against a live database.
- **`SystemOptions.Company.ForceInactive`** exists as a kill switch but has never been exercised.

## Before enabling the first tenant

1. Each module must register a `StampCompanyOnExistingRows` activity on its tenant-setup queue.
   The enablement workflow fans out through `ModuleSyncRegistry`; a module that has not
   registered one fails enablement and leaves the flag off — the safe outcome, but it means
   enablement cannot succeed until every installed module has implemented its side.
2. Resolve the downstream blockers in `classification.md`, in particular that
   `EmployeeChangedEvent` and the employee-sync activities carry no `CompanyId`, so synced
   employee rows are written with no company.
3. Audit `GetByCodeAsync(code)` call sites. Employee code uniqueness widened to
   `{TenantId, CompanyId, Code}`, so a lookup by code alone can now resolve another company's
   employee.
