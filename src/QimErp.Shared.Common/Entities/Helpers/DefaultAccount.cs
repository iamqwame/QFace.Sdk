namespace QimErp.Shared.Common.Entities.Helpers;
public static class DefaultAccount
{
    private static readonly Dictionary<AccountType, Account> Accounts = new()
    {
        {
            AccountType.AccountReceivable,
            new Account("53eb796b-2968-45aa-9e6d-282a2b7e8188", "10120", "Accounts Receivable")
        },
        { AccountType.Sales, new Account("3cb05c17-beb5-46f4-a902-c085f5a33e32", "40100", "Sales") },
        { AccountType.Inventory, new Account("8d0b7010-532c-4efb-bc88-41c40df52d3b", "10800", "Inventory") },
        { AccountType.COGS, new Account("2903fd7d-1341-4339-9612-fc1162cf80ee", "50300", "Cost of Goods Sold") },
        {
            AccountType.InvAdjustment,
            new Account("5c906e4e-31e1-4fcd-bece-228e726e5e64", "50500", "Purchase Price Variance")
        },
        {
            AccountType.ProductAssmCost,
            new Account("2317d1d8-4fd6-4ec4-89dc-8f7d5b118819", "10900", "Stocks of Work in Progress")
        },
        {
            AccountType.PettyCash,
            new Account("05dc34a4-33c9-4d6b-b569-965bae375197", "10113", "Cash in Hand A/C")
        },
        {
            AccountType.GeneralFund,
            new Account("8565ec5a-3f21-41e5-87bd-ae9ce5a606de", "10111", "Regular Checking Account")
        },
        {
            AccountType.AdvanceAccount,
            new Account("d4303b23-b497-4581-a423-5a20e5c5e6d2", "20120", "Customer Advances")
        },
        {
            AccountType.SaleDiscount,
            new Account("e265d996-d35c-4b0f-bdc7-0a4f238e1fda", "40400", "Sales Discounts")
        },
        {
            AccountType.PurchaseAccount,
            new Account("035f980d-8196-4cb8-8774-3719fd97ffbe", "50200", "Purchase A/C")
        },
        {
            AccountType.AccountPayable,
            new Account("c64c7451-c436-4f19-9b1e-03921cd8204e", "20110", "Accounts Payable")
        },
        {
            AccountType.PurchaseDiscount,
            new Account("2a948f5a-4dae-4f94-b7a1-bfe656b10a06", "50400", "Purchase Discounts")
        },
        {
            AccountType.GoodsReceiptNoteClearing,
            new Account("b6311583-c48e-4fb0-a048-6c143e2bbce6", "10810", "Goods Received Clearing Account")
        },
        {
            AccountType.ShippingCharge,
            new Account("90712337-2ca2-4b01-be6a-e159f8cbdabe", "40500", "Shipping and Handling")
        },
        { AccountType.SalesTax, new Account("63e9c58f-f4a2-4732-8459-25c62b716d52", "20300", "Sales Tax") },
        { AccountType.PurchaseTax, new Account("85d36679-08f8-4d41-95bd-3cc7cf08c8b0", "50700", "Purchase Tax") },
        { AccountType.CustomerAdvance, new Account("f8c3d9a1-2b45-4c7d-89f1-1a2b3c4d5e6f", "20120", "Customer Advances") },


    };

    public static Account Get(AccountType accountType) => Accounts[accountType];
}

public enum AccountType
{
    AccountReceivable,
    Sales,
    Inventory,
    COGS,
    InvAdjustment,
    ProductAssmCost,
    PettyCash,
    GeneralFund,
    AdvanceAccount,
    SaleDiscount,
    PurchaseAccount,
    AccountPayable,
    PurchaseDiscount,
    GoodsReceiptNoteClearing,
    ShippingCharge,
    SalesTax,
    PurchaseTax,
    CustomerAdvance,
    VendorAdvance
}

public class Account
{
    public string? Id { get; }
    public string Code { get; }
    public string? Name { get; }
    public string? FullName => $"{Code}-{Name}";

    public Account(string? id, string code, string? name)
    {
        Id = id;
        Code = code;
        Name = name;
    }
}