namespace QimErp.Shared.Common.Entities;

/// <summary>DATA-23 retention class. Values must stay aligned with Platform RetentionPolicy catalog.</summary>
public enum RetentionClass
{
    Transient = 0,
    Operational = 1,
    Statutory = 2,
    Permanent = 3,
}
