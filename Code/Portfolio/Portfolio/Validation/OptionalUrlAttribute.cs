using System.ComponentModel.DataAnnotations;

namespace Portfolio.Validation;
public class OptionalUrlAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        var stringValue = value as string;

        if (string.IsNullOrWhiteSpace(stringValue))
            return true; // nothing provided — valid, nothing to check

        return Uri.TryCreate(stringValue, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public override string FormatErrorMessage(string name) =>
        $"The {name} field must be a valid URL.";
}