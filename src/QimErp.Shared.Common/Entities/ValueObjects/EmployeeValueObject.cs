namespace QimErp.Shared.Common.Entities.ValueObjects;

public class EmployeeValueObject
{
    public EmployeeValueObject(Guid id, string code, string name,
        string email, string picture)
    {
        Id = id;
        Code = code;
        Name = name;
        Email = email;
        Picture = picture;
    }
    public EmployeeValueObject()
    {

    }

    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Picture { get; set; }


    public string? JobTitle { get; set; }
    public string? JobTitleCode { get; set; }
    public string? OrganizationalUnit { get; set; }
    public string? OrganizationalUnitCode { get; set; }
    public string? Station { get; set; }
    public string? StationCode { get; set; }
    public string? Rank { get; set; }
    public string? RankCode { get; set; }

    /// <summary>Grade name (e.g. "Senior Engineer"). Populated only when the source
    /// entity carries grade data (Payroll Employee). Null on plain EmployeeBase snapshots.</summary>
    public string? Grade { get; set; }
    public string? GradeCode { get; set; }
    public int?    GradeLevel { get; set; }
    public int?    Notch { get; set; }


    public static EmployeeValueObject Create(Guid id, string code, string name, string email, string picture)
    {
        return new EmployeeValueObject(id, code, name, email, picture);
    }
}
