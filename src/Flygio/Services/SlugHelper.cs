using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Flygio.Services;

public static partial class SlugHelper
{
    public static string ToSlug(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var result = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        // Replace Swedish characters explicitly
        result = result.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        result = NonAlphaNum().Replace(result, "-");
        result = MultipleDashes().Replace(result, "-").Trim('-');
        return result;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphaNum();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleDashes();
}
