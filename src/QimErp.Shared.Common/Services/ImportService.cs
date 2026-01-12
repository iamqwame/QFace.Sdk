using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services;

public abstract class ImportService<TContext> : IImportService
    where TContext : ApplicationDbContext<TContext>
{
    protected readonly TContext _context;
    protected readonly ILogger<ImportService<TContext>> _logger;
    protected readonly IDistributedCacheService _cacheService;
    protected const string CacheRegion = AppConstant.Cache.Regions.Hr;

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

    public async Task UpdateProgressAsync(
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
            return;
        }

        import.UpdateProgress(processedRows, successfulImports, failedImports);
        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateCacheAsync(import.TenantId, importId);
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

    public async Task<Import?> GetImportAsync(Guid importId, CancellationToken cancellationToken = default)
    {
        try
        {
            var import = await Imports.FindAsync([importId], cancellationToken);
            if (import == null)
            {
                return null;
            }

            var cacheKey = AppConstant.Cache.Keys.Import(import.TenantId, importId);
            
            var cachedImport = await _cacheService.GetAsync<Import>(cacheKey, CacheRegion);
            if (cachedImport != null)
            {
                _logger.LogDebug("Import {ImportId} retrieved from cache", importId);
                return cachedImport;
            }

            await _cacheService.SetAsync(
                cacheKey,
                import,
                TimeSpan.FromMinutes(AppConstant.Cache.Ttl.Import),
                CacheRegion);
            
            _logger.LogDebug("Import {ImportId} cached for {Ttl} minutes", importId, AppConstant.Cache.Ttl.Import);
            
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
            var cacheKey = AppConstant.Cache.Keys.Import(tenantId, importId);
            await _cacheService.RemoveAsync(cacheKey, CacheRegion);
            _logger.LogDebug("Invalidated cache for import {ImportId}", importId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for import {ImportId}", importId);
        }
    }
}

