namespace QimErp.Shared.Common.Entities.Helpers;

public class ContactPerson
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Relationship { get; set; }
    public bool IsPrimaryContact { get; set; }
    public bool IsEmergencyContact { get; set; }

    public static ContactPerson Create(string name, string phone, string phoneNumberCode = "GH")
    {
        return new ContactPerson
        {
            Id = Guid.NewGuid(),
            Name = name,
            Phone = phone.ToStandardPhoneNumber(phoneNumberCode)
        };
    } 
    public static ContactPerson Create(string name)
    {
        return new ContactPerson
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }
    public ContactPerson WithEmail(string? email)
    {
        if (email.IsEmpty()) return this;

        Email = email;
        return this;
    }

    public ContactPerson IsNamed(string name)
    {
        Name = name;
        return this;
    }

    public ContactPerson WithPhone(string phone)
    {
        Phone = phone.ToStandardPhoneNumber();
        return this;
    }

    public ContactPerson WithRelationship(string relationship)
    {
        Relationship = relationship;
        return this;
    }

   

    public ContactPerson AsEmergencyContact(bool isEmergency)
    {
        IsEmergencyContact = isEmergency;
        return this;
    }


    public ContactPerson AsPrimaryContact(bool isPrimary)
    {
        IsPrimaryContact = isPrimary;
        return this;
    }
}
