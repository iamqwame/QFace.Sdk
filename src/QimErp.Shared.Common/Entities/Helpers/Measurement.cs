namespace QimErp.Shared.Common.Entities.Helpers;

public class Measurement
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public static Measurement GetDefault()
    {
        return Get("ea");
    }
    public static Measurement Get(string code)
    {
        var measurement = GetAll()
            .FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.CurrentCultureIgnoreCase));
        if (measurement == null)
        {
            throw new DomainException("MeasurementNotFound", $"Measurement with code '{code}' was not found.");
        }

        return measurement;
    }


    public static IEnumerable<Measurement> GetAll()
    {
        return new List<Measurement>
        {
            new()
            {
                Code = "M3",
                Description = "Cubic meter"
            },
            new()
            {
                Code = "PC",
                Description = "Piece"
            },
            new()
            {
                Code = "BTL",
                Description = "Bottle"
            },
            new()
            {
                Code = "PCT",
                Description = "Percentage"
            },
            new()
            {
                Code = "GAL",
                Description = "Gallon"
            },
            new()
            {
                Code = "CCM",
                Description = "Cubic centimeter"
            },
            new()
            {
                Code = "DZ",
                Description = "Dozen"
            },
            new()
            {
                Code = "G",
                Description = "Gram"
            },
            new()
            {
                Code = "PT",
                Description = "Pint"
            },
            new()
            {
                Code = "KT",
                Description = "Kiloton"
            },
            new()
            {
                Code = "EA",
                Description = "Each"
            },
            new()
            {
                Code = "CBM",
                Description = "Cubic meters"
            },
            new()
            {
                Code = "PAL",
                Description = "Pallet"
            },
            new()
            {
                Code = "HR",
                Description = "Hour"
            },
            new()
            {
                Code = "LB",
                Description = "US Pound"
            },
            new()
            {
                Code = "MG",
                Description = "Milligram"
            },
            new()
            {
                Code = "CAR",
                Description = "Carton"
            },
            new()
            {
                Code = "MM",
                Description = "Millimeter"
            },
            new()
            {
                Code = "FT3",
                Description = "Cubic foot"
            },
            new()
            {
                Code = "L",
                Description = "Liter"
            },
            new()
            {
                Code = "OZ",
                Description = "Ounces"
            },
            new()
            {
                Code = "CR",
                Description = "Crate"
            },
            new()
            {
                Code = "C",
                Description = "Celsius"
            },
            new()
            {
                Code = "BOX",
                Description = "Boxes"
            },
            new()
            {
                Code = "KGV",
                Description = "Kilogram/cubic meter"
            },
            new()
            {
                Code = "CDM",
                Description = "Cubic decimeter"
            },
            new()
            {
                Code = "CTN",
                Description = "Container"
            },
            new()
            {
                Code = "M2",
                Description = "Square meter"
            },
            new()
            {
                Code = "CS",
                Description = "Case"
            },
            new()
            {
                Code = "FT",
                Description = "Foot"
            },
            new()
            {
                Code = "KG",
                Description = "Kilogram"
            },
            new()
            {
                Code = "KM",
                Description = "Kilometer"
            },
            new()
            {
                Code = "IN",
                Description = "Inch"
            },
            new()
            {
                Code = "CAN",
                Description = "Canister"
            },
            new()
            {
                Code = "MO",
                Description = "Monthly"
            },
            new()
            {
                Code = "NON",
                Description = "Unspecified"
            },
            new()
            {
                Code = "M",
                Description = "Meter"
            },
            new()
            {
                Code = "DM",
                Description = "Decimeter"
            },
            new()
            {
                Code = "ML",
                Description = "Milliliter"
            },
            new()
            {
                Code = "CM_2",
                Description = "Square centimeter (CM\u00b2)"
            },
            new()
            {
                Code = "PK",
                Description = "Pack"
            },
            new()
            {
                Code = "CM",
                Description = "Centimeter"
            }
        };
    }


}