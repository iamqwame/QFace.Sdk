namespace QimErp.Shared.Common.Entities.Helpers;

public class AccountProperty
{
    /// <summary>
    /// Gets the account code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Gets the account name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the unique account ID.
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="AccountProperty"/> with mandatory fields.
    /// </summary>
    /// <param name="accountId">The unique identifier for the account.</param>
    /// <param name="name">The name of the account.</param>
    /// <param name="code">The code for the account.</param>
    /// <returns>A new instance of <see cref="AccountProperty"/>.</returns>
    public static AccountProperty Create(string? accountId, string? name, string? code = "")
    {
        return new AccountProperty
        {
            AccountId = accountId,
            Name = name,
            Code = code
        };
    }

    /// <summary>
    /// Sets the account code and returns the updated instance.
    /// </summary>
    /// <param name="code">The code to set.</param>
    /// <returns>The updated <see cref="AccountProperty"/> instance.</returns>
    public AccountProperty WithCode(string code)
    {
        Code = code;
        return this;
    }

    /// <summary>
    /// Sets the account name and returns the updated instance.
    /// </summary>
    /// <param name="name">The name to set.</param>
    /// <returns>The updated <see cref="AccountProperty"/> instance.</returns>
    public AccountProperty WithName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    /// Sets the account ID and returns the updated instance.
    /// </summary>
    /// <param name="accountId">The account ID to set.</param>
    /// <returns>The updated <see cref="AccountProperty"/> instance.</returns>
    public AccountProperty WithAccountId(string accountId)
    {
        AccountId = accountId;
        return this;
    }
}


/// <summary>
/// Represents an account with a code, name, and unique account ID.
/// </summary>
public class AccountingSides
{
    public string? DebitAccountId { get; set; } = string.Empty;
    public string? DebitAccount { get; set; } = string.Empty;
    public string DebitNote { get; set; } = string.Empty;
    public string? CreditAccountId { get; set; } = string.Empty;
    public string? CreditAccount { get; set; } = string.Empty;
    public string CreditNote { get; set; } = string.Empty;

    public static AccountingSides Create()
    {
        return new AccountingSides();
    }
    public AccountingSides OnDebit(string? accountId, string? account)
    {
        DebitAccountId = accountId;
        DebitAccount = account;
        return this;
    }
    public AccountingSides OnCredit(string? accountId, string? account)
    {
        CreditAccountId = accountId;
        CreditAccount = account;
        return this;
    }

    public AccountingSides WithNote(string credit, string debit)
    {
        CreditNote = credit;
        DebitNote = debit;
        return this;
    }

}