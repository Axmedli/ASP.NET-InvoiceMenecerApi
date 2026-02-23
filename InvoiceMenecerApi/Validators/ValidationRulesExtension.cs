using FluentValidation;
using System.Text.RegularExpressions;

namespace InvoiceMenecerApi.Validators;

public static class ValidationRulesExtension
{

    public static IRuleBuilder<T, string> Password<T>(
        this IRuleBuilder<T,
            string> ruleBuilder,
        bool mustContainLowerCase = true,
        bool mustContainUpperrCase = true,
        bool mustContainDigit = true
        )
    {
        return ruleBuilder
            .Must(password =>
            {
                if (mustContainLowerCase && !Regex.IsMatch(password, @"[a-z]"))
                    return false;
                if (mustContainUpperrCase && !Regex.IsMatch(password, @"[A-Z]"))
                    return false;
                if (mustContainDigit && !Regex.IsMatch(password, @"\d"))
                    return false;
                return true;
            }).WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one digit.");

    }
}

