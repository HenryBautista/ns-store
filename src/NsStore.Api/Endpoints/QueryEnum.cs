using NsStore.Application.Common;

namespace NsStore.Api.Endpoints;

/// <summary>
/// Reads an enum out of a query string the same way the API writes one into JSON.
///
/// Minimal APIs bind enum parameters with the case-sensitive <c>Enum.TryParse(string, out T)</c>
/// overload, so <c>?status=credit</c> — the exact camelCase spelling this API emits in every
/// response, per the <c>JsonStringEnumConverter</c> in <c>Program.cs</c> — fails to bind and
/// surfaces as an unhandled <c>BadHttpRequestException</c>, i.e. a 500. Parsing the raw string
/// here keeps one spelling on the wire in both directions, and turns an unknown value into a 400
/// with an error code instead of a server error.
/// </summary>
internal static class QueryEnum
{
    /// <summary>Null when the parameter was omitted; throws when it carries a value we don't know.</summary>
    public static TEnum? Parse<TEnum>(string? value, string parameterName)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // TryParse also accepts numbers and undeclared combinations, so IsDefined has the last word.
        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new BadRequestException(
            $"'{value}' is not a valid value for '{parameterName}'. Expected one of: {string.Join(", ", Enum.GetNames<TEnum>())}.");
    }
}
