namespace QimErp.Shared.Common.Contracts;
public class NoteRequest
{
    public string? Internal { get; set; }
    public string? External { get; set; }
}
public class AddressRequest
{
    public string? Street1 { get; set; }
    public string? Street2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string Line1 { get; set; }

    public static AddressRequest GetSample()
    {
        return new AddressRequest
        {
            Street1 = "123 Main St",
            Street2 = "Apt 4B",
            City = "Springfield",
            State = "IL",
            Zip = "62704",
            PostalCode = "123456",
            Country = "USA"
        };
    }

}


public class AddressResponse
{
    public string Street1 { get; set; } = "";
    public string Street2 { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Country { get; set; } = "";
}