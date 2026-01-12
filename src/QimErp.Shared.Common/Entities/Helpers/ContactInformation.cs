namespace QimErp.Shared.Common.Entities.Helpers;

public class ContactInformation
{
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }

    public ContactInformation()
    {
        Email = "";
        PhoneNumber = "";
    }

    public ContactInformation(string email, string phone)
    {
        Email = email;
        PhoneNumber = phone;
    }

    public static ContactInformation Create(string? email, string? phone, string phoneNumberCode = "GH")
    {
        return new ContactInformation(email ?? "", phone.ToStandardPhoneNumber(phoneNumberCode));
    }

    public ContactInformation WithEmail(string email)
    {
        Email = email;
        return this;
    }

    public ContactInformation WithPhoneNumber(string phoneNumber, string phoneNumberCode = "GH")
    {
        PhoneNumber = phoneNumber.ToStandardPhoneNumber(phoneNumberCode);
        return this;
    }
}
