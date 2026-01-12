namespace QimErp.Shared.Common.Services;

public interface IImportService
{
    Task<Import> StartImportAsync(
        string importType,
        string? fileName,
        long? fileSize,
        string? contentType,
        string tenantId,
        string userId,
        string userEmail,
        string? userName = null,
        CancellationToken cancellationToken = default);

    Task UpdateTotalRowsAsync(Guid importId, int totalRows, CancellationToken cancellationToken = default);

    Task UpdateProgressAsync(
        Guid importId,
        int processedRows,
        int successfulImports,
        int failedImports,
        CancellationToken cancellationToken = default);

    Task CompleteImportAsync(
        Guid importId,
        int totalRows,
        int successfulImports,
        int failedImports,
        CancellationToken cancellationToken = default);

    Task FailImportAsync(Guid importId, string errorMessage, CancellationToken cancellationToken = default);

    Task StartBatchSavingAsync(Guid importId, int totalBatches, CancellationToken cancellationToken = default);

    Task UpdateBatchSaveProgressAsync(Guid importId, int batchesSaved, int batchesFailed, CancellationToken cancellationToken = default);

    Task<Import?> GetImportAsync(Guid importId, CancellationToken cancellationToken = default);

    Task<List<Import>> GetImportsAsync(
        string? importType = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<int> GetImportsCountAsync(
        string? importType = null,
        string? status = null,
        CancellationToken cancellationToken = default);
}

