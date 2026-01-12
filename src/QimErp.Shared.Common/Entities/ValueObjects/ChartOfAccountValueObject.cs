namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Value object representing a Chart of Account for use in other modules.
/// Contains essential account information needed for journal entry creation and display.
/// </summary>
public class ChartOfAccountValueObject
{
    // Required for journal entry creation
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    // Required for business logic (determines debit/credit behavior)
    // Stored as int to avoid cross-module enum dependencies
    public int AccountType { get; set; } // GlAccountType as int (0=Asset, 1=Liability, 2=Equity, 3=Revenue, 4=Expense)
    public int NormalBalance { get; set; } // NormalBalance as int (0=Debit, 1=Credit)
    
    // Required for validation (can't post to non-posting accounts)
    public bool IsPostingAccount { get; set; }
    
    // Useful for business logic (reverses debit/credit behavior)
    public bool IsContraAccount { get; set; }
    
    // Optional - useful for display/grouping
    public string? AccountCategoryName { get; set; }
    
    // Constructors
    public ChartOfAccountValueObject() { }
    
    public ChartOfAccountValueObject(
        Guid id,
        string code,
        string name,
        int accountType,
        int normalBalance,
        bool isPostingAccount = true,
        bool isContraAccount = false,
        string? accountCategoryName = null)
    {
        Id = id;
        Code = code;
        Name = name;
        AccountType = accountType;
        NormalBalance = normalBalance;
        IsPostingAccount = isPostingAccount;
        IsContraAccount = isContraAccount;
        AccountCategoryName = accountCategoryName;
    }
}
