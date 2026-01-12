namespace QimErp.Shared.Common.Events;

/// <summary>
/// Event published when a bank reconciliation is completed with adjustments
/// Used by GL module to create adjustment journal entries
/// </summary>
public class BankReconciliationCompletedEvent : DomainEvent
{
    public Guid ReconciliationId { get; set; }
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public decimal BankFees { get; set; }
    public decimal BankInterest { get; set; }
    public decimal AdjustmentAmount { get; set; }

    public BankReconciliationCompletedEvent()
    {
    }

    public BankReconciliationCompletedEvent(
        Guid reconciliationId,
        Guid bankAccountId,
        string bankAccountName,
        decimal bankFees,
        decimal bankInterest,
        decimal adjustmentAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        ReconciliationId = reconciliationId;
        BankAccountId = bankAccountId;
        BankAccountName = bankAccountName;
        BankFees = bankFees;
        BankInterest = bankInterest;
        AdjustmentAmount = adjustmentAmount;
    }

    public static BankReconciliationCompletedEvent Create(
        Guid reconciliationId,
        Guid bankAccountId,
        string bankAccountName,
        decimal bankFees,
        decimal bankInterest,
        decimal adjustmentAmount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new BankReconciliationCompletedEvent(reconciliationId, bankAccountId, bankAccountName, bankFees, bankInterest, adjustmentAmount, tenantId, userEmail, triggeredBy, userName);
    }
}
