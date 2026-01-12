namespace QimErp.Shared.Common.Events;

/// <summary>
/// Domain event fired when a currency is created or updated
/// </summary>
public class CurrencyUpdatedEvent : DomainEvent
{
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public bool IsBaseCurrency { get; set; }
    public int DecimalPlaces { get; set; }

    public CurrencyUpdatedEvent()
    {
    }

    public CurrencyUpdatedEvent(
        Guid currencyId,
        string currencyCode,
        string currencyName,
        string currencySymbol,
        bool isBaseCurrency,
        int decimalPlaces,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        CurrencyId = currencyId;
        CurrencyCode = currencyCode;
        CurrencyName = currencyName;
        CurrencySymbol = currencySymbol;
        IsBaseCurrency = isBaseCurrency;
        DecimalPlaces = decimalPlaces;
    }

    public static CurrencyUpdatedEvent Create(
        Guid currencyId,
        string currencyCode,
        string currencyName,
        string currencySymbol,
        bool isBaseCurrency,
        int decimalPlaces,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new CurrencyUpdatedEvent(currencyId, currencyCode, currencyName, 
            currencySymbol, isBaseCurrency, decimalPlaces, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a currency is deleted or deactivated
/// </summary>
public class CurrencyDeletedEvent : DomainEvent
{
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;

    public CurrencyDeletedEvent()
    {
    }

    public CurrencyDeletedEvent(
        Guid currencyId,
        string currencyCode,
        string currencyName,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        CurrencyId = currencyId;
        CurrencyCode = currencyCode;
        CurrencyName = currencyName;
    }

    public static CurrencyDeletedEvent Create(
        Guid currencyId,
        string currencyCode,
        string currencyName,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new CurrencyDeletedEvent(currencyId, currencyCode, currencyName, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a chart of account is created or updated
/// Note: AccountType and NormalBalance are stored as int to avoid cross-module enum dependencies
/// </summary>
public class ChartOfAccountUpdatedEvent : DomainEvent
{
    public Guid AccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AccountType { get; set; } // GlAccountType as int
    public int NormalBalance { get; set; } // NormalBalance as int
    public bool IsPostingAccount { get; set; }
    public bool IsContraAccount { get; set; }

    public ChartOfAccountUpdatedEvent()
    {
    }

    public ChartOfAccountUpdatedEvent(
        Guid accountId,
        string code,
        string name,
        int accountType,
        int normalBalance,
        bool isPostingAccount,
        bool isContraAccount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        AccountId = accountId;
        Code = code;
        Name = name;
        AccountType = accountType;
        NormalBalance = normalBalance;
        IsPostingAccount = isPostingAccount;
        IsContraAccount = isContraAccount;
    }

    public static ChartOfAccountUpdatedEvent Create(
        Guid accountId,
        string code,
        string name,
        int accountType,
        int normalBalance,
        bool isPostingAccount,
        bool isContraAccount,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ChartOfAccountUpdatedEvent(accountId, code, name, accountType, 
            normalBalance, isPostingAccount, isContraAccount, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a chart of account is deleted or deactivated
/// </summary>
public class ChartOfAccountDeletedEvent : DomainEvent
{
    public Guid AccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ChartOfAccountDeletedEvent()
    {
    }

    public ChartOfAccountDeletedEvent(
        Guid accountId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        AccountId = accountId;
        Code = code;
        Name = name;
    }

    public static ChartOfAccountDeletedEvent Create(
        Guid accountId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ChartOfAccountDeletedEvent(accountId, code, name, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a journal entry is created by another module (e.g., Cash Management)
/// GL Consumer will consume this and create the JournalEntry in GL
/// </summary>
public class JournalEntryCreatedEvent : DomainEvent
{
    public DateTime EntryDate { get; set; }
    public int EntryType { get; set; } // JournalEntryType as int
    public Guid FiscalPeriodId { get; set; }
    public string FiscalPeriodCode { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; } = 1.0m;
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string SourceModule { get; set; } = string.Empty;
    public Guid? SourceDocumentId { get; set; }
    public List<JournalEntryLineData> Lines { get; set; } = [];

    public JournalEntryCreatedEvent()
    {
    }

    public JournalEntryCreatedEvent(
        DateTime entryDate,
        int entryType,
        Guid fiscalPeriodId,
        string fiscalPeriodCode,
        string currencyCode,
        string description,
        string sourceModule,
        List<JournalEntryLineData> lines,
        string tenantId,
        decimal exchangeRate = 1.0m,
        string? referenceNumber = null,
        Guid? sourceDocumentId = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        EntryDate = entryDate;
        EntryType = entryType;
        FiscalPeriodId = fiscalPeriodId;
        FiscalPeriodCode = fiscalPeriodCode;
        CurrencyCode = currencyCode;
        ExchangeRate = exchangeRate;
        Description = description;
        ReferenceNumber = referenceNumber;
        SourceModule = sourceModule;
        SourceDocumentId = sourceDocumentId;
        Lines = lines;
    }

    public static JournalEntryCreatedEvent Create(
        DateTime entryDate,
        int entryType,
        Guid fiscalPeriodId,
        string fiscalPeriodCode,
        string currencyCode,
        string description,
        string sourceModule,
        List<JournalEntryLineData> lines,
        string tenantId,
        decimal exchangeRate = 1.0m,
        string? referenceNumber = null,
        Guid? sourceDocumentId = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new JournalEntryCreatedEvent(entryDate, entryType, fiscalPeriodId, fiscalPeriodCode,
            currencyCode, description, sourceModule, lines, tenantId, exchangeRate, referenceNumber,
            sourceDocumentId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Journal entry line data for JournalEntryCreatedEvent
/// </summary>
public class JournalEntryLineData
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public int TaxTransactionType { get; set; } // TaxTransactionType as int
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
    public Guid? TaxAccountId { get; set; }
    public string? TaxAccountCode { get; set; }
}

/// <summary>
/// Domain event fired when a cost center is created or updated
/// </summary>
public class CostCenterUpdatedEvent : DomainEvent
{
    public Guid CostCenterId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCostCenterId { get; set; }

    public CostCenterUpdatedEvent()
    {
    }

    public CostCenterUpdatedEvent(
        Guid costCenterId,
        string code,
        string name,
        string? description,
        Guid? parentCostCenterId,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        CostCenterId = costCenterId;
        Code = code;
        Name = name;
        Description = description;
        ParentCostCenterId = parentCostCenterId;
    }

    public static CostCenterUpdatedEvent Create(
        Guid costCenterId,
        string code,
        string name,
        string? description,
        Guid? parentCostCenterId,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new CostCenterUpdatedEvent(costCenterId, code, name, description, parentCostCenterId, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a cost center is deleted or deactivated
/// </summary>
public class CostCenterDeletedEvent : DomainEvent
{
    public Guid CostCenterId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public CostCenterDeletedEvent()
    {
    }

    public CostCenterDeletedEvent(
        Guid costCenterId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        CostCenterId = costCenterId;
        Code = code;
        Name = name;
    }

    public static CostCenterDeletedEvent Create(
        Guid costCenterId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new CostCenterDeletedEvent(costCenterId, code, name, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a fiscal period is created or updated
/// </summary>
public class FiscalPeriodUpdatedEvent : DomainEvent
{
    public Guid FiscalPeriodId { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid FiscalYearId { get; set; }
    public string FiscalYearCode { get; set; } = string.Empty;

    public FiscalPeriodUpdatedEvent()
    {
    }

    public FiscalPeriodUpdatedEvent(
        Guid fiscalPeriodId,
        string periodCode,
        string periodName,
        DateTime startDate,
        DateTime endDate,
        Guid fiscalYearId,
        string fiscalYearCode,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        FiscalPeriodId = fiscalPeriodId;
        PeriodCode = periodCode;
        PeriodName = periodName;
        StartDate = startDate;
        EndDate = endDate;
        FiscalYearId = fiscalYearId;
        FiscalYearCode = fiscalYearCode;
    }

    public static FiscalPeriodUpdatedEvent Create(
        Guid fiscalPeriodId,
        string periodCode,
        string periodName,
        DateTime startDate,
        DateTime endDate,
        Guid fiscalYearId,
        string fiscalYearCode,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new FiscalPeriodUpdatedEvent(fiscalPeriodId, periodCode, periodName, startDate, endDate, fiscalYearId, fiscalYearCode, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a fiscal year is created or updated
/// </summary>
public class FiscalYearUpdatedEvent : DomainEvent
{
    public Guid FiscalYearId { get; set; }
    public string YearCode { get; set; } = string.Empty;
    public string YearName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public FiscalYearUpdatedEvent()
    {
    }

    public FiscalYearUpdatedEvent(
        Guid fiscalYearId,
        string yearCode,
        string yearName,
        DateTime startDate,
        DateTime endDate,
        bool isActive,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        FiscalYearId = fiscalYearId;
        YearCode = yearCode;
        YearName = yearName;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
    }

    public static FiscalYearUpdatedEvent Create(
        Guid fiscalYearId,
        string yearCode,
        string yearName,
        DateTime startDate,
        DateTime endDate,
        bool isActive,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new FiscalYearUpdatedEvent(fiscalYearId, yearCode, yearName, startDate, endDate, isActive, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Domain event fired when a journal entry is posted
/// Used by Budget Planning module to sync actuals
/// </summary>
public class JournalEntryPostedEvent : DomainEvent
{
    public Guid JournalEntryId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public Guid FiscalPeriodId { get; set; }
    public string FiscalPeriodCode { get; set; } = string.Empty;
    public List<JournalEntryLinePostedData> Lines { get; set; } = [];

    public JournalEntryPostedEvent()
    {
    }

    public JournalEntryPostedEvent(
        Guid journalEntryId,
        string entryNumber,
        DateTime entryDate,
        Guid fiscalPeriodId,
        string fiscalPeriodCode,
        List<JournalEntryLinePostedData> lines,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        JournalEntryId = journalEntryId;
        EntryNumber = entryNumber;
        EntryDate = entryDate;
        FiscalPeriodId = fiscalPeriodId;
        FiscalPeriodCode = fiscalPeriodCode;
        Lines = lines;
    }

    public static JournalEntryPostedEvent Create(
        Guid journalEntryId,
        string entryNumber,
        DateTime entryDate,
        Guid fiscalPeriodId,
        string fiscalPeriodCode,
        List<JournalEntryLinePostedData> lines,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new JournalEntryPostedEvent(journalEntryId, entryNumber, entryDate, fiscalPeriodId, fiscalPeriodCode, lines, tenantId, userEmail, triggeredBy, userName);
    }
}

/// <summary>
/// Journal entry line data for JournalEntryPostedEvent
/// </summary>
public class JournalEntryLinePostedData
{
    public Guid LineId { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
}
