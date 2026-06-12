namespace QimErp.Shared.Common.Constants;

/// <summary>
/// Canonical OU codes used as <c>WorkflowApprover.ValueId</c> (type = "department")
/// and in any service that needs to locate a specific organizational unit by code.
/// </summary>
public static class OrganizationalUnitCodes
{
    public const string Executive             = "EXEC";
    public const string HumanResources        = "HR";
    public const string People                = "PEOPLE";   // HR equivalent in Service/Corporate profiles
    public const string Finance               = "FINANCE";
    public const string Operations            = "OPS";
    public const string InformationTechnology = "IT";
    public const string Risk                  = "RISK";
    public const string Audit                 = "AUDIT";
    public const string Tax                   = "TAX";
    public const string BusinessDevelopment   = "BD";
    public const string Retail                = "RETAIL";
    public const string Corporate             = "CORPORATE";
    public const string Investment            = "INVESTMENT";
    public const string Treasury              = "TREASURY";
    public const string Advisory              = "ADVISORY";
    public const string Legal                 = "LEGAL";
    public const string Marketing             = "MARKETING";
    public const string SupplyChain           = "SUPPLY_CHAIN";
    public const string CustomerService       = "CUSTOMER_SERVICE";
    public const string ResearchAndDevelopment = "RD";
}
