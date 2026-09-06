# Phase 1 SQL Gate — Result: PASSED

Verified against **EF Core 9.0.0 / Npgsql.EntityFrameworkCore.PostgreSQL 9.0.2** (production versions).
Harness: `.scratch/CompanyFilterGate` — uses `ToQueryString()`, so it needs no database and no credentials.

## Generated SQL

```sql
SELECT i."Name" FROM "Item" AS i
WHERE NOT (i."Deleted")
  AND (i."IsGlobal" OR i."TenantId" = @__ef_filter__CurrentTenantId_0)
  AND (@__ef_filter__p_2 OR i."CompanyId" = ANY (@__ef_filter__AllowedCompanyIds_1))
ORDER BY i."Name"
```

| Check | Result |
|---|---|
| Collection `Contains` → single array parameter `= ANY(@p)` | PASS |
| No literal `IN ('a','b')` expansion | PASS |
| SQL body identical for 2 vs 5 companies | PASS — one cached plan regardless of company set |
| SQL body identical when `CompanyFilterActive = false` | PASS — **no `IModelCacheKeyFactory` change needed** |

The bool short-circuit parameterises as `@__ef_filter__p_2`, so "All Companies" and a narrow company
set produce the same plan. This is why the clause must never be *omitted* for single-company tenants:
omitting it changes the shape and would make the model per-tenant.

## Correction to the plan

The plan asserted that `IReadOnlyList<string>` "falls off the `PgAnyExpression` path" and that
`string[]` was therefore mandatory. **That is false on these versions** — `IReadOnlyList<string>`
produces byte-identical SQL, also array-parameterised, also shape-stable.

`string[]` is still what we ship, because it is in the locked plan and there is no reason to change
it. But the justification was wrong, so do not treat the array type as load-bearing: if a future
refactor needs `IReadOnlyList<string>`, it is safe. Re-run the gate on any EF or Npgsql upgrade.
