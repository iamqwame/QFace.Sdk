namespace QimErp.Shared.Common.Security;

/// <summary>DATA-25 column classification. PII and Sensitive require encryption at rest plus blind-index lookup.</summary>
public enum DataClassification
{
    Public = 0,
    Internal = 1,
    Confidential = 2,
    Pii = 3,
    Sensitive = 4,
}
