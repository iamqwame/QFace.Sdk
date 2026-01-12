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
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Email { get; set; }
    public string? Picture { get; set; }
    

    public static EmployeeValueObject Create(Guid id, string code, string name, string email, string picture)
    {
        return new EmployeeValueObject(id, code, name, email, picture);
    }
}