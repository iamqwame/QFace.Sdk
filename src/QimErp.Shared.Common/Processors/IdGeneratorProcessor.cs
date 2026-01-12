namespace QimErp.Shared.Common.Processors;
public static class IdGeneratorProcessor
{
    /// <summary>
    /// Generates an ID based on the provided format and code.
    /// </summary>
    /// <param name="idFormat">The ID format object containing the format string.</param>
    /// <param name="number">The sequential or unique number to be included in the ID.</param>
    /// <param name="numberOfPaddedZeros">The length of the padded number. Default is 6.</param>
    /// <returns>The generated ID as a string.</returns>
    public static string GenerateId(this IdFormat? idFormat, long number, int numberOfPaddedZeros = 6)
    {
        // Validate the format and number parameters
        if (idFormat == null || string.IsNullOrWhiteSpace(idFormat.Value))
            throw new DomainException("IdGenerator:Format", "Format cannot be null or empty.");

        if (number <= 0)
            throw new DomainException("IdGenerator:Number", "Number must be greater than 0.");

        if (numberOfPaddedZeros < 0)
            throw new DomainException("IdGenerator:NumberOfPaddedZeros", "NumberOfPaddedZeros can't be less than zero");

        var format = idFormat.Value;
        var code = number.ToString($"D{numberOfPaddedZeros}");
        var now = DateTime.UtcNow;
        format = format.Replace("{CODE}", code);

        // ***Improved handling of concatenated placeholders***
        var result = new StringBuilder();
        var parts = Regex.Split(format, @"(\{[^}]+\})"); // Split by placeholders

        foreach (var part in parts)
        {
            if (part.StartsWith("{") && part.EndsWith("}"))
            {
                var placeholder = part.Substring(1, part.Length - 2);
                if (placeholder == "CODE") continue;

                result.Append(now.ToString(placeholder, CultureInfo.InvariantCulture));

            }
            else
            {
                result.Append(part);
            }
        }

        return result.ToString();
    }

    public static string GetDefaultCode(this IdFormatType idFormatType, long no)
    {
        var format = idFormatType switch
        {
            IdFormatType.Customer => "CUS{CODE}{ddMMyy}",
            IdFormatType.Vendor => "CUS{CODE}{ddMMyy}",
            IdFormatType.Employee => "EMP{CODE}{ddMMyy}",
            IdFormatType.SalesInvoice => "INV{CODE}{ddMMyy}",
            IdFormatType.SalesOrder => "SO-CODE}{ddMMyy}",
            IdFormatType.SalesPayment => "PAY{CODE}{ddMMyy}",
            IdFormatType.BatchInvoicePayment => "INVBT{CODE}{ddMMyy}",
            IdFormatType.Product => "PROD{CODE}{ddMMyy}",
            IdFormatType.PurchaseInvoice => "PI{CODE}{ddMMyy}",
            IdFormatType.PurchaseOrder => "PO{CODE}{ddMMyy}",
            IdFormatType.PriceAdjustmentBatchNo => "PA{CODE}{ddMMyy}",
            IdFormatType.WaybillNo => "WAY{CODE}{dd}",
            IdFormatType.BatchStockTransfer => "ST{CODE}{dd}",
            _ => throw new DomainException("IdGenerator:IdFormatType", "Invalid IdFormatType.")
        };
        return IdFormat.Create(format).GenerateId(no, 5);
    }
}

public enum IdFormatType
{
    Customer,
    SalesInvoice,
    PurchaseInvoice,
    BatchInvoicePayment,
    BatchStockTransfer,
    WaybillNo,
    SalesOrder,
    SalesPayment,
    Product,
    Employee,
    Vendor,
    PurchaseOrder,
    PriceAdjustmentBatchNo
}

public class IdFormat
{
    private IdFormat(string value)
    {
        Value = value;
    }
    public string Value { get; private set; }

    /// <summary>
    /// Creates an IdFormat instance.
    /// </summary>
    /// <param name="value">The format string.</param>
    /// <returns>An instance of IdFormat.</returns>
    public static IdFormat Create(string value)
    {
        return new IdFormat(value);
    }
}





