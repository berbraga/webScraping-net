using System.Text.RegularExpressions;

namespace WebScraping.Domain.Services;

public static partial class CopyrightYearExtractor
{
    private static readonly Regex FooterRegex = FooterPattern();
    private static readonly Regex TagRegex = TagPattern();
    private static readonly Regex RangeRegex = YearRangePattern();
    private static readonly Regex SingleYearRegex = SingleYearPattern();

    public static int? TryExtractOldestYear(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var footerMatch = FooterRegex.Match(html);
        if (footerMatch.Success)
        {
            var fromFooter = ExtractFromRegion(footerMatch.Groups[1].Value);
            if (fromFooter.HasValue)
            {
                return fromFooter;
            }
        }

        var start = (int)(html.Length * 0.8);
        if (start < 0 || start >= html.Length)
        {
            return ExtractFromRegion(html);
        }

        return ExtractFromRegion(html[start..]);
    }

    private static int? ExtractFromRegion(string region)
    {
        var text = TagRegex.Replace(region, " ");
        var years = new List<int>();

        text = RangeRegex.Replace(text, match =>
        {
            if (int.TryParse(match.Groups[1].Value, out var a)
                && int.TryParse(match.Groups[2].Value, out var b))
            {
                years.Add(Math.Min(a, b));
                years.Add(Math.Max(a, b));
            }

            return " ";
        });

        foreach (Match match in SingleYearRegex.Matches(text))
        {
            if (int.TryParse(match.Groups[1].Value, out var year))
            {
                years.Add(year);
            }
        }

        return years.Count == 0 ? null : years.Min();
    }

    [GeneratedRegex(@"<footer\b[^>]*>(.*?)</footer>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex FooterPattern();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagPattern();

    [GeneratedRegex(@"\b((?:19|20)\d{2})\s*[-–—]\s*((?:19|20)\d{2})\b")]
    private static partial Regex YearRangePattern();

    [GeneratedRegex(@"\b((?:19|20)\d{2})\b")]
    private static partial Regex SingleYearPattern();
}
