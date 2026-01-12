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
