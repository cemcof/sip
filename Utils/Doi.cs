using System.Text.RegularExpressions;

namespace sip.Utils;

/// <summary>
/// Utility class for working with DOIs (digital object identifiers)
/// </summary>
public static class Doi
{
    private const string DOI_REGEX = @"10\.\d{4,9}\/[-\._;\(\)\/\:A-Z0-9]+";
    private const string DOI_URL_PREFIX = "https://doi.org/";

    public static IEnumerable<string> ExtractDoisFromText(string text)
    {
        var matches = Regex.Matches(text, DOI_REGEX, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
        return matches.Select(m => m.Value);
    }

    public static IEnumerable<(string doi, string url)> GetDoiUrls(IEnumerable<string> dois)
        => dois.Select(GetDoiUrl);

    public static (string doi, string url) GetDoiUrl(string doi)
        => (doi, DOI_URL_PREFIX + doi);
}