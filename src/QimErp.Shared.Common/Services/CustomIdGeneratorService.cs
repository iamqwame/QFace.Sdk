using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Processors;

namespace QimErp.Shared.Common.Services;

/// <summary>
/// Service to generate custom formatted IDs for entities across all modules
/// </summary>
public class CustomIdGeneratorService<TContext>(
    TContext context,
    ILogger<CustomIdGeneratorService<TContext>> logger)
    where TContext : ApplicationDbContext<TContext>
{
    /// <summary>
    /// Gets the next sequential number for an entity type
    /// </summary>
    private async Task<long> GetNextNumberAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var dbSet = context.Set<T>();
            var count = await dbSet.CountAsync(cancellationToken);
            return count + 1;
        }
        catch
        {
            // If table doesn't exist, start at 1
            return 1;
        }
    }

    /// <summary>
    /// Generates a custom formatted code for any entity type
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="format">The format string (e.g., "ALL{CODE}{ddMMyy}")</param>
    /// <param name="padding">Number of digits for padding (default: 6)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted code string</returns>
    public async Task<string> GenerateCodeAsync<TEntity>(
        string format,
        int padding = 6,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        var number = await GetNextNumberAsync<TEntity>(cancellationToken);
        var idFormat = IdFormat.Create(format);
        return idFormat.GenerateId(number, padding);
    }

    /// <summary>
    /// Generates a custom formatted code for Allowance
    /// Format: ALL{CODE}{ddMMyy} e.g., ALL000001241225
    /// </summary>
    public async Task<string> GenerateAllowanceCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("ALL{CODE}{ddMMyy}", 6, cancellationToken);
    }

    /// <summary>
    /// Generates a custom formatted code for Deduction
    /// Format: DED{CODE}{ddMMyy} e.g., DED000001241225
    /// </summary>
    public async Task<string> GenerateDeductionCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("DED{CODE}{ddMMyy}", 6, cancellationToken);
    }

    /// <summary>
    /// Generates a custom formatted code for Bonus
    /// Format: BON{CODE}{ddMMyy} e.g., BON000001241225
    /// </summary>
    public async Task<string> GenerateBonusCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("BON{CODE}{ddMMyy}", 6, cancellationToken);
    }

    /// <summary>
    /// Generates a custom formatted code for PayrollRun
    /// Format: PR{CODE}{ddMMyy} e.g., PR000001241225
    /// </summary>
    public async Task<string> GeneratePayrollRunCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("PR{CODE}{ddMMyy}", 6, cancellationToken);
    }

    /// <summary>
    /// Generates a custom formatted code for EmployeeLoan
    /// Format: LOAN{CODE}{ddMMyy} e.g., LOAN000001241225
    /// </summary>
    public async Task<string> GenerateEmployeeLoanCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("LOAN{CODE}{ddMMyy}", 6, cancellationToken);
    }

    /// <summary>
    /// Generates a custom formatted code for SalaryAdvance
    /// Format: ADV{CODE}{ddMMyy} e.g., ADV000001241225
    /// </summary>
    public async Task<string> GenerateSalaryAdvanceCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("ADV{CODE}{ddMMyy}", 6, cancellationToken);
    }

    /// <summary>
    /// Generates a custom formatted code for ClaimRequest
    /// Format: CLM{CODE}{ddMMyy} e.g., CLM000001241225
    /// </summary>
    public async Task<string> GenerateClaimRequestCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("CLM{CODE}{ddMMyy}", 6, cancellationToken);
    }

    /// <summary>
    /// Generates a custom formatted code for Payment
    /// Format: PAY{CODE}{ddMMyy} e.g., PAY000001241225
    /// </summary>
    public async Task<string> GeneratePaymentCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("PAY{CODE}{ddMMyy}", 6, cancellationToken);
    }

    /// <summary>
    /// Generates a custom formatted code for Insurance
    /// Format: INS{CODE}{ddMMyy} e.g., INS000001241225
    /// </summary>
    public async Task<string> GenerateInsuranceCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("INS{CODE}{ddMMyy}", 6, cancellationToken);
    }

    /// <summary>
    /// Generates a custom formatted code for ProvidentFund
    /// Format: PF{CODE}{ddMMyy} e.g., PF000001241225
    /// </summary>
    public async Task<string> GenerateProvidentFundCodeAsync<TEntity>(
        CancellationToken cancellationToken = default) where TEntity : class
    {
        return await GenerateCodeAsync<TEntity>("PF{CODE}{ddMMyy}", 6, cancellationToken);
    }
}

