namespace QimErp.Shared.Common.Entities.Helpers;

public class TaxationGroupIndex
{
    public string Name { get; set; }

    public string Description { get; set; }

    public int Index { get; set; }

    public static TaxationGroupIndex Get(string id)
    {
        var taxationGroup = GetAll().FirstOrDefault(x => x?.Description == id);

        if (taxationGroup == null)
        {
            throw new DomainException("TaxationGroupNotFound", $"Taxation group with description '{id}' was not found.");
        }

        return taxationGroup;
    }


    private static IEnumerable<TaxationGroupIndex?> GetAll()
    {
        return new List<TaxationGroupIndex?>
        {
            new()
            {
                Name = "Sub-Total-1",
                Description = "Taxation",
                Index = 1,
            },
            new()
            {
                Name = "Sub-Total-2",
                Description = "VAT",
                Index = 2,
            },
        };
    }
}