namespace QimErp.Shared.Common.Entities.Helpers;

public class Address
{
    public string? Street1 { get; set; }
    public string? Street2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    
    public Address(){}

    public static Address Create(string? street1 = "")
    {
        return new Address
        {
            Street1 = street1 ?? ""
        };
    }

    public Address SetStreet2(string? street2)
    {
        Street2 = street2 ?? "";
        return this;
    }

    public Address InCity(string? city)
    {
        City = city ?? "";
        return this;
    }

    public Address LocatedInState(string? state)
    {
        State = state ?? "";
        return this;
    }

    public Address HavingZip(string? zip)
    {
        Zip = zip ?? "";
        return this;
    }

    public Address WithPostalCode(string? postalCode)
    {
        PostalCode = postalCode ?? "";
        return this;
    }

    public Address InCountry(string? country)
    {
        Country = country ?? "";
        return this;
    }

    public Address InStreet(string? street)
    {
        Street1 = street ?? "";
        return this;
    }

    public Address InRegion(string?  region)
    {
        PostalCode = region ?? "";
        return this;
    }
}