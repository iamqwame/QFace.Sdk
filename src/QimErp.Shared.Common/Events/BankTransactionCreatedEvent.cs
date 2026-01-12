namespace QimErp.Shared.Common.Events;

/// <summary>
/// Event published when a bank transaction is created in Cash Management
/// Used by GL module for auto-posting
/// </summary>
public class BankTransactionCreatedEvent : DomainEvent
{
    public Guid TransactionId { get; set; }
    public Guid BankAccountId { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }

    public BankTransactionCreatedEvent()
    {
    }

    public BankTransactionCreatedEvent(
        Guid transactionId,
        Guid bankAccountId,
        string bankAccountName,
        string transactionType,
        decimal amount,
        DateTime transactionDate,
        string tenantId,
        string? description = null,
        Guid? costCenterId = null,
        string? costCenterCode = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        TransactionId = transactionId;
        BankAccountId = bankAccountId;
        BankAccountName = bankAccountName;
        TransactionType = transactionType;
        Amount = amount;
        TransactionDate = transactionDate;
        Description = description;
        CostCenterId = costCenterId;
        CostCenterCode = costCenterCode;
    }

    public static BankTransactionCreatedEvent Create(
        Guid transactionId,
        Guid bankAccountId,
        string bankAccountName,
        string transactionType,
        decimal amount,
        DateTime transactionDate,
        string tenantId,
        string? description = null,
        Guid? costCenterId = null,
        string? costCenterCode = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new BankTransactionCreatedEvent(transactionId, bankAccountId, bankAccountName, transactionType, amount, transactionDate, tenantId, description, costCenterId, costCenterCode, userEmail, triggeredBy, userName);
    }
}
