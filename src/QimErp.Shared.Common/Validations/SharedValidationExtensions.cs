using PhoneNumbers;

namespace QimErp.Shared.Common.Validations;

public static class SharedValidationExtensions
{
    private static readonly PhoneNumberUtil PhoneNumberUtil = PhoneNumberUtil.GetInstance();
    private static readonly Regex FullNameRegex =
        new("^[a-z ,.'-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex EmailRegex =
        new("^[\\w!#$%&’*+/=?`{|}~^-]+(?:\\.[\\w!#$%&’*+/=?`{|}~^-]+)*@(?:[a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);


    public static IRuleBuilderOptions<T, string> MustBeValidPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder, string region = "GH")
    {
        return ruleBuilder.Must(phone =>
        {
            try
            {
                var parsedPhone = PhoneNumberUtil.Parse(phone, region);
                return PhoneNumberUtil.IsValidNumber(parsedPhone);
            }
            catch (NumberParseException)
            {
                return false;
            }
        }).WithMessage((model, phone) => $"Invalid phone number: '{phone}'.");
    }

    // Same as MustBeValidPhoneNumber, but the region is resolved per-request (e.g. from a
    // sibling country-code field) instead of a fixed default — for flows like tenant
    // registration where the caller isn't assumed to be in a single country.
    public static IRuleBuilderOptions<T, string> MustBeValidPhoneNumberForRegion<T>(
        this IRuleBuilder<T, string> ruleBuilder, Func<T, string?> regionSelector)
    {
        return ruleBuilder.Must((model, phone) =>
        {
            var region = regionSelector(model);
            if (string.IsNullOrWhiteSpace(region)) region = "GH";
            try
            {
                var parsedPhone = PhoneNumberUtil.Parse(phone, region);
                return PhoneNumberUtil.IsValidNumber(parsedPhone);
            }
            catch (NumberParseException)
            {
                return false;
            }
        }).WithMessage((model, phone) => $"Invalid phone number: '{phone}'.");
    }

    private static readonly Regex BareGhanaMobileMoneyPhone = new("^0[2-9][0-9]{8}$", RegexOptions.Compiled);
    private static readonly Regex IntlGhanaMobileMoneyPhone = new("^\\+?233[2-9][0-9]{8}$", RegexOptions.Compiled);

    /// <summary>
    /// Normalises a Ghana mobile money number to the bare 10-digit local format
    /// (e.g. "0241234567"), accepting either that form or +233/233 international
    /// form. Returns null if the input matches neither — MoMo providers only
    /// recognise these two shapes, so this is intentionally stricter than
    /// <see cref="MustBeValidPhoneNumber{T}"/>.
    /// </summary>
    public static string? NormaliseGhanaMobileMoneyPhone(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = new string(input.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());

        if (BareGhanaMobileMoneyPhone.IsMatch(trimmed)) return trimmed;
        if (IntlGhanaMobileMoneyPhone.IsMatch(trimmed))
        {
            var afterCountryCode = trimmed.StartsWith("+233") ? trimmed[4..] : trimmed[3..];
            return "0" + afterCountryCode;
        }
        return null;
    }

    public static IRuleBuilderOptions<T, string> MustBeValidGhanaMobileMoneyNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(phone => NormaliseGhanaMobileMoneyPhone(phone) != null)
            .WithMessage((model, phone) => $"Invalid Ghana mobile money number: '{phone}'.");
    }

    // Validate Currency
    public static IRuleBuilderOptions<T, string> MustBeValidCurrency<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(code =>
        {
            try
            {
                Currency.Get(code);
                return true;
            }
            catch (DomainException)
            {
                return false;
            }
        }).WithMessage((model, code) => $"Invalid currency code: '{code}'.");
    }

    // Validate Email List
    public static IRuleBuilderOptions<T, string[]?> MustContainValidEmails<T>(this IRuleBuilder<T, string[]?> ruleBuilder)
    {
        return ruleBuilder.Must(emails => emails == null || emails.All(email => EmailRegex.IsMatch(email)))
            .WithMessage("All email addresses must be valid.");
    }

    // Validate Measurement Code
    public static IRuleBuilderOptions<T, string> MustBeValidMeasurement<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(code =>
        {
            try
            {
                Measurement.Get(code);
                return true;
            }
            catch (DomainException)
            {
                return false;
            }
        }).WithMessage((model, code) => $"Invalid measurement code: '{code}'.");
    }

    // Validate Full Name
    public static IRuleBuilderOptions<T, string> MustBeValidFullName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(name => FullNameRegex.IsMatch(name))
            .WithMessage((model, name) => $"Invalid full name format: '{name}'.");
    }

    // Validate Email Format
    public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(email => EmailRegex.IsMatch(email))
            .WithMessage((model, email) => $"Invalid email format: '{email}'.");
    }
}
