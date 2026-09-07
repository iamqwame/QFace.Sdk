using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Workflows;
using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Generic module-side implementation of the "StampCompanyOnExistingRows" activity every module
/// registers on its tenant-setup task queue. Reflects the DbContext model rather than requiring a
/// per-module hand-written list, so a module adopting this needs only DI registration, not code.
/// </summary>
public sealed class CompanyStampActivities<TContext>(TContext context, ILogger<CompanyStampActivities<TContext>>? logger = null)
    where TContext : DbContext
{
    private static readonly MethodInfo StampEntityTypeMethod =
        typeof(CompanyStampActivities<TContext>).GetMethod(nameof(StampEntityTypeAsync), BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Activity("StampCompanyOnExistingRows")]
    public async Task<CompanyStampResult> StampCompanyOnExistingRowsAsync(CompanyStampRequest request)
    {
        var ct = ActivityExecutionContext.Current.CancellationToken;

        if (string.IsNullOrWhiteSpace(request.CompanyId))
        {
            return new CompanyStampResult { Success = false, ErrorMessage = "CompanyId is required to stamp existing rows." };
        }

        using var stamp = CompanyStampScope.Enter(request.CompanyId);

        var stamped = 0;
        var targets = context.Model.GetEntityTypes()
            .Where(t => t.ClrType is { IsAbstract: false }
                        && typeof(AuditableEntity).IsAssignableFrom(t.ClrType)
                        && !typeof(ITenantWideEntity).IsAssignableFrom(t.ClrType))
            .Select(t => t.ClrType)
            .Distinct()
            .ToList();

        foreach (var clrType in targets)
        {
            var task = (Task<int>)StampEntityTypeMethod
                .MakeGenericMethod(clrType)
                .Invoke(this, [request.TenantId, request.CompanyId, ct])!;
            stamped += await task;
        }

        await context.SaveChangesAsync(ct);

        logger?.LogInformation(
            "[StampCompanyOnExistingRows] tenant={TenantId} company={CompanyId} rows={Rows}",
            request.TenantId, request.CompanyId, stamped);

        return new CompanyStampResult { Success = true, RowsStamped = stamped };
    }

    private async Task<int> StampEntityTypeAsync<TEntity>(string tenantId, string companyId, CancellationToken ct)
        where TEntity : AuditableEntity
    {
        var rows = await context.Set<TEntity>()
            .Where(e => e.TenantId == tenantId && e.CompanyId == string.Empty)
            .ToListAsync(ct);

        foreach (var row in rows) row.CompanyId = companyId;
        return rows.Count;
    }
}
