using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services;

public abstract class ImportService<TContext> : IImportService
    where TContext : ApplicationDbContext<TContext>
{
    protected readonly TContext _context;
    protected readonly ILogger<ImportService<TContext>> _logger;
    protected readonly IDistributedCacheService _cacheService;
    protected const string CacheRegion = "hr";
    private const int ImportCacheTtlMinutes = 5;
    private static string ImportCacheKey(string tenantId, Guid importId) =>
        $"qface:qimerp:{tenantId}:hr:import_{importId}";

    protected ImportService(
        TContext context,
        ILogger<ImportService<TContext>> logger,
        IDistributedCacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    protected abstract DbSet<Import> Imports { get; }

    public async Task<Import> StartImportAsync(
        string importType,
        string? fileName,
        long? fileSize,
        string? contentType,
        string tenantId,
        string userId,
        string userEmail,
        string? userName = null,
        CancellationToken cancellationToken = default)
    {
        var import = Import.Create(importType, fileName, fileSize, contentType, tenantId, userId, userEmail, userName);
        
        await Imports.AddAsync(import, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created import record. ImportId: {ImportId}, ImportType: {ImportType}, TenantId: {TenantId}",
            import.Id, importType, tenantId);

        return import;
    }

    public async Task UpdateTotalRowsAsync(Guid importId, int totalRows, CancellationToken cancellationToken = default)
    {
        var import = await Imports.FindAsync([importId], cancellationToken);
        if (import == null)
        {
            _logger.LogWarning("Import {ImportId} not found for total rows update", importId);
            return;
        }

        if (import.Status == ImportStatus.NotStarted)
        {
            import.Start(totalRows);
        }
        else
        {
            import.UpdateTotalRows(totalRows);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCacheAsync(import.TenantId, importId);

        _logger.LogInformation("Updated total rows for ImportId: {ImportId}, TotalRows: {TotalRows}", importId, totalRows);
    }

    public async Task<Import?> UpdateProgressAsync(
        Guid importId,
        int processedRows,
        int successfulImports,
        int failedImports,
        CancellationToken cancellationToken = default)
    {
        var import = await Imports.FindAsync([importId], cancellationToken);
        if (import == null)
        {
            _logger.LogWarning("Import {ImportId} not found for progress update", importId);
            return null;
        }

        import.UpdateProgress(processedRows, successfulImports, failedImports);
        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCacheAsync(import.TenantId, importId);
        return import;
    }

    public async Task CompleteImportAsync(
        Guid importId,
        int totalRows,
        int successfulImports,
        int failedImports,
        CancellationToken cancellationToken = default)
    {
        var import = await Imports.FindAsync([importId], cancellationToken);
        if (import == null)
        {
            _logger.LogWarning("Import {ImportId} not found for completion", importId);
            return;
        }

        import.UpdateProgress(totalRows, successfulImports, failedImports);
        import.Complete();
        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCacheAsync(import.TenantId, importId);

        _logger.LogInformation("Import completed. ImportId: {ImportId}, Successful: {Successful}, Failed: {Failed}",
            importId, successfulImports, failedImports);
    }

    public async Task FailImportAsync(Guid importId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var import = await Imports.FindAsync([importId], cancellationToken);
        if (import == null)
        {
            _logger.LogWarning("Import {ImportId} not found for failure", importId);
            return;
        }

        import.Fail(errorMessage);
        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCacheAsync(import.TenantId, importId);

        _logger.LogError("Import failed. ImportId: {ImportId}, Error: {ErrorMessage}", importId, errorMessage);
    }

    public async Task StartBatchSavingAsync(Guid importId, int totalBatches, CancellationToken cancellationToken = default)
    {
        var import = await Imports.FindAsync([importId], cancellationToken);
        if (import == null)
        {
            _logger.LogWarning("Import {ImportId} not found for starting batch saving", importId);
            return;
        }

        import.StartBatchSaving(totalBatches);
        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCacheAsync(import.TenantId, importId);

        _logger.LogInformation("Started batch saving for ImportId: {ImportId}, TotalBatches: {TotalBatches}", importId, totalBatches);
    }

    public async Task UpdateBatchSaveProgressAsync(Guid importId, int batchesSaved, int batchesFailed, CancellationToken cancellationToken = default)
    {
        var import = await Imports.FindAsync([importId], cancellationToken);
        if (import == null)
        {
            _logger.LogWarning("Import {ImportId} not found for batch save progress update", importId);
            return;
        }

        import.UpdateBatchSaveProgress(batchesSaved, batchesFailed);
        
        // Check if all batches are complete
        if (import.BatchesSaved + import.BatchesFailed >= import.BatchesQueued && import.BatchesQueued > 0)
        {
            // All batches are done, complete the import
            // Note: We need the final counts from the import response, but since we're tracking batches,
            // we'll use the current values. The actual completion should be called from ImportProgressService
            // with the final response data. For now, we just update the status.
            _logger.LogInformation("All batches completed for ImportId: {ImportId}. BatchesSaved: {BatchesSaved}, BatchesFailed: {BatchesFailed}",
                importId, import.BatchesSaved, import.BatchesFailed);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCacheAsync(import.TenantId, importId);

        _logger.LogInformation("Updated batch save progress for ImportId: {ImportId}, BatchesSaved: {BatchesSaved}, BatchesFailed: {BatchesFailed}",
            importId, batchesSaved, batchesFailed);
    }

    public async Task<Import?> IncrementBatchSaveOutcomeAsync(Guid importId, bool success, CancellationToken cancellationToken = default)
    {
        var import = await Imports.FindAsync([importId], cancellationToken);
        if (import == null)
        {
            _logger.LogWarning("Import {ImportId} not found for batch save outcome increment", importId);
            return null;
        }

        import.IncrementBatchOutcome(success);

        if (import.BatchesSaved + import.BatchesFailed >= import.BatchesQueued && import.BatchesQueued > 0)
        {
            _logger.LogInformation("All batches completed for ImportId: {ImportId}. BatchesSaved: {BatchesSaved}, BatchesFailed: {BatchesFailed}",
                importId, import.BatchesSaved, import.BatchesFailed);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCacheAsync(import.TenantId, importId);
        return import;
    }

    /// <summary>
    /// Accumulates a downstream sync outcome (e.g. one EmployeeBulkSyncWorkflow chunk's IAM
    /// provisioning result) onto the import's running SyncSucceededCount/SyncFailedCount
    /// totals. A single import job can fan out to MULTIPLE bulk-sync workflow runs (chunked
    /// ~200 employees each, dispatched with a stagger), and those chunks can report their
    /// outcome concurrently/out of order. A naive load → mutate in memory → SaveChangesAsync
    /// here would race: two chunks reading the same starting count and each writing back
    /// "+1" would leave the counter at 1 instead of 2, silently dropping a chunk's
    /// contribution. Using EF's ExecuteUpdateAsync instead emits a single
    /// <c>UPDATE ... SET "SyncSucceededCount" = COALESCE("SyncSucceededCount", 0) + @p</c>
    /// SQL statement scoped to this one row — the increment is evaluated and applied
    /// atomically by the database itself, so concurrent chunk callbacks accumulate correctly
    /// with no read-modify-write window and no optimistic-concurrency retry loop needed.
    /// </summary>
    public async Task UpdateSyncOutcomeAsync(
        Guid importId,
        int succeededCount,
        int failedCount,
        CancellationToken cancellationToken = default)
    {
        // Fetched separately (not as part of the atomic update) purely to key the cache
        // invalidation below — TenantId is immutable once set, so there is no race here.
        var tenantId = await Imports
            .Where(i => i.Id == importId)
            .Select(i => i.TenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenantId == null)
        {
            _logger.LogWarning("Import {ImportId} not found for sync outcome update", importId);
            return;
        }

        var rowsAffected = await Imports
            .Where(i => i.Id == importId)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(i => i.SyncSucceededCount, i => (i.SyncSucceededCount ?? 0) + succeededCount)
                    .SetProperty(i => i.SyncFailedCount, i => (i.SyncFailedCount ?? 0) + failedCount)
                    .SetProperty(i => i.LastUpdatedAt, DateTime.UtcNow),
                cancellationToken);

        if (rowsAffected == 0)
        {
            _logger.LogWarning("Import {ImportId} not found for sync outcome update", importId);
            return;
        }

        await InvalidateCacheAsync(tenantId, importId);

        _logger.LogInformation(
            "Recorded sync outcome for ImportId: {ImportId}, ChunkSucceeded: {ChunkSucceeded}, ChunkFailed: {ChunkFailed}",
            importId, succeededCount, failedCount);
    }

    public async Task<Import?> GetImportAsync(Guid importId, CancellationToken cancellationToken = default)
    {
        try
        {
            var import = await Imports.FindAsync([importId], cancellationToken);
            if (import == null)
            {
                return null;
            }

            var cacheKey = ImportCacheKey(import.TenantId, importId);
            
            var cachedImport = await _cacheService.GetAsync<Import>(cacheKey, CacheRegion);
            if (cachedImport != null)
            {
                _logger.LogDebug("Import {ImportId} retrieved from cache", importId);
                return cachedImport;
            }

            await _cacheService.SetAsync(
                cacheKey,
                import,
                TimeSpan.FromMinutes(ImportCacheTtlMinutes),
                CacheRegion);
            
            _logger.LogDebug("Import {ImportId} cached for {Ttl} minutes", importId, ImportCacheTtlMinutes);
            
            return import;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get import {ImportId}. This might be due to database schema issues.", importId);
            return null;
        }
    }

    public async Task<List<Import>> GetImportsAsync(
        string? importType = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = Imports.AsQueryable();

        if (!string.IsNullOrWhiteSpace(importType))
        {
            query = query.Where(i => i.ImportType == importType);
        }

        if (!string.IsNullOrWhiteSpace(status) && 
            Enum.TryParse<ImportStatus>(status, true, out var statusEnum))
        {
            query = query.Where(i => i.Status == statusEnum);
        }

        return await query
            .OrderByDescending(i => i.Created)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetImportsCountAsync(
        string? importType = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = Imports.AsQueryable();

        if (!string.IsNullOrWhiteSpace(importType))
        {
            query = query.Where(i => i.ImportType == importType);
        }

        if (!string.IsNullOrWhiteSpace(status) && 
            Enum.TryParse<ImportStatus>(status, true, out var statusEnum))
        {
            query = query.Where(i => i.Status == statusEnum);
        }

        return await query.CountAsync(cancellationToken);
    }

    protected async Task InvalidateCacheAsync(string tenantId, Guid importId)
    {
        try
        {
            var cacheKey = ImportCacheKey(tenantId, importId);
            await _cacheService.RemoveAsync(cacheKey, CacheRegion);
            _logger.LogDebug("Invalidated cache for import {ImportId}", importId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for import {ImportId}", importId);
        }
    }
}

