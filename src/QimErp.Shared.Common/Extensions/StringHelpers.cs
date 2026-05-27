using PhoneNumbers;

namespace QimErp.Shared.Common.Extensions;
public static class StringHelpers
{
    private static readonly PhoneNumberUtil PhoneNumberUtil = PhoneNumberUtil.GetInstance();
   
    /// <summary>
    /// Formats a phone number to the standard E.164 format (e.g., 233543459509).
    /// </summary>
    /// <param name="phone">The phone number to format.</param>
    /// <param name="region">The region code, default is "GH" (Ghana).</param>
    /// <returns>The formatted phone number in E.164 format, or null if invalid.</returns>
    public static string ToStandardPhoneNumber(this string? phone, string region = "GH")
    {
        if (phone.IsEmpty()) return string.Empty;
    
        try
        {
            var parsedPhone = PhoneNumberUtil.Parse(phone, region);
            if (PhoneNumberUtil.IsValidNumber(parsedPhone))
            {
                // Format to international E.164 without the "+" sign
                return PhoneNumberUtil.Format(parsedPhone, PhoneNumberFormat.E164).TrimStart('+');
            }
        }
        catch (NumberParseException)
        {
            // Handle invalid number parsing
        }
    
        return string.Empty;
    }
   

    public static IResult ToIResult<T>(this Result<T> result)
    {
        if (!result.IsFailure) return Results.Ok(result.ToResponse());

        if (result.Code == "404")
        {
            return Results.NotFound(result.Error);
        }

        if (result.Code == "409")
        {
            return Results.Json(result.ToResponse(), statusCode: StatusCodes.Status409Conflict);
        }

        if (result.Code == "401")
        {
            return Results.Json(result.ToResponse(), statusCode: StatusCodes.Status401Unauthorized);
        }

        if (result.Code == "403")
        {
            return Results.Json(result.ToResponse(), statusCode: StatusCodes.Status403Forbidden);
        }

        return Results.BadRequest(result.Error);
    }

    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }
}
