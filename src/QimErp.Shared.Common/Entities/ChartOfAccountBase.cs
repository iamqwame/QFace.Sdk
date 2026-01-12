namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Base class for ChartOfAccount entities across all modules.
/// Contains common properties and methods shared by module-specific ChartOfAccount entities.
/// </summary>
public abstract class ChartOfAccountBase : GuidAuditableEntity
{
    public string Code { get; protected set; } = string.Empty;
    public string Name { get; protected set; } = string.Empty;
    public int AccountType { get; protected set; } // Stored as int (GlAccountType enum)
    public int NormalBalance { get; protected set; } // Stored as int (NormalBalance enum)
    public bool IsPostingAccount { get; protected set; }
    public Guid GlAccountId { get; protected set; } // Original GL account ID for reference

    // Computed Properties
    public bool IsActive => DataStatus == DataState.Active;

    protected ChartOfAccountBase() { }

    protected ChartOfAccountBase(
        Guid? id,
        string code,
        string name,
        int accountType,
        int normalBalance)
    {
        Id = id ?? CreateId();
        Code = code;
        Name = name;
        AccountType = accountType;
        NormalBalance = normalBalance;
        IsPostingAccount = true;
        AsActive();
    }

    /// <summary>
    /// Updates account information (code and name)
    /// </summary>
    public ChartOfAccountBase UpdateInfo(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Account code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name is required", nameof(name));

        Code = code;
        Name = name;
        return this;
    }

    /// <summary>
    /// Syncs account information from GL event
    /// </summary>
    public ChartOfAccountBase SyncFromGlEvent(string code, string name, int accountType, int normalBalance, bool isPostingAccount)
    {
        Code = code;
        Name = name;
        AccountType = accountType;
        NormalBalance = normalBalance;
        IsPostingAccount = isPostingAccount;
        return this;
    }

    /// <summary>
    /// Sets the GL account ID reference
    /// </summary>
    public ChartOfAccountBase WithGlAccountId(Guid glAccountId)
    {
        GlAccountId = glAccountId;
        return this;
    }

    /// <summary>
    /// Sets whether this is a posting account
    /// </summary>
    public ChartOfAccountBase WithPostingAccount(bool isPostingAccount)
    {
        IsPostingAccount = isPostingAccount;
        return this;
    }

    /// <summary>
    /// Activates the account
    /// </summary>
    public ChartOfAccountBase Activate()
    {
        AsActive();
        return this;
    }

    /// <summary>
    /// Deactivates the account
    /// </summary>
    public new ChartOfAccountBase Deactivate()
    {
        base.Deactivate();
        return this;
    }
}
