namespace QimErp.Shared.Common.Processors;

/// <summary>
/// Internal taxation constants used by TaxationProcessor and TaxationShared.
/// These are implementation details of the SDK's taxation engine; they are not
/// part of the public API surface.
/// </summary>
internal static class TaxationConstants
{
    internal static class Core
    {
        public const string DistributionBaseOnTheBase = "Base";
        public const string DistributionBaseOnThePercentageOfTheBase = "% of Tax";
    }

    internal static class IncludedInPrice
    {
        public const string Default = "None";
        public const string TaxIncluded = "TaxIncluded";
    }

    internal static class TaxType
    {
        public const string Default = "None";
        public const string Sales = "Sales";
        public const string Purchases = "Purchases";
    }

    internal static class TaxComputationMethod
    {
        public const string Fixed = "Fixed";
        public const string Percentage = "Percentage";
        public const string PercentageOfTaxIncluded = "PercentageOfTaxIncluded";
        public const string Group = "Group";
    }

    internal static class TaxScope
    {
        public const string Default = "None";
    }
}
