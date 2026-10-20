namespace QimErp.Shared.Common.Security;

/// <summary>DATA-25: marks a persisted property with its data classification.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class DataClassificationAttribute(DataClassification classification) : Attribute
{
    public DataClassification Classification { get; } = classification;
}
