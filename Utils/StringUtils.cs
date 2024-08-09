using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace sip.Utils;

public static class StringUtils
{
    public static string ToSafeFilename(string fileName, string? postfix = null)
    {
        var ext = Path.GetExtension(fileName);
        var fname = Path.GetFileNameWithoutExtension(fileName);
            
        var invalidChars = Path.GetInvalidFileNameChars();
        fname = string.Join("_", fname.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
            .TrimEnd('.');

        return (postfix is null) ? fname + ext : fname + "_" + postfix + ext;
    }
        
    public static byte[] HexToByteArray(this string str)
    {
        return Enumerable.Range(0, str.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(str.Substring(x, 2), 16))
            .ToArray();
    }

    public static string[] SplitWithTrim(this string str, params string[] separators)
        => str.Split(separators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    
    // public static byte[] HexToByteArray(this ReadOnlySpan<char> str)
    // {
    //     var chars = str;
    //     return Enumerable.Range(0, str.Length)
    //         .Where(x => x % 2 == 0)
    //         .Select(x => Convert.ToByte(chars.Slice(x, 2), 16))
    //         .ToArray();
    // }
    //     
    
    
    // DEL - this implementation is already in humanize lib
    // public static string LimitLength(this string str, int maxChars = 50, string suffix = "...", bool fromEnd = false)
    // {
    //     if (suffix.Length > maxChars)
    //     {
    //         throw new ArgumentException($"{nameof(suffix)} must not be longer than {nameof(maxChars)}");
    //     }
    //
    //     if (str.Length > maxChars + suffix.Length)
    //     {
    //         return fromEnd ? 
    //             suffix + str[(str.Length - maxChars + suffix.Length)..] : 
    //             str[..(maxChars - suffix.Length)] + suffix;
    //     }
    //
    //     return str;
    // }

    public static string WithPlaceholderExt(this string str, string placeholder = " - ")
    {
        return (string.IsNullOrWhiteSpace(str)) ? placeholder : str;
    }

    public static string WithPlaceholder(string? str, string placeholder = " - ")
    {
        return (str is null) ? "".WithPlaceholderExt(placeholder) : str.WithPlaceholderExt(placeholder);
    }
        
    public static bool IsFilterMatchAtLeastOneOf(string? filter, params string?[] targets)
    {
        // On empty search string we just assume a match (user is not searching)
        if (string.IsNullOrWhiteSpace(filter))
            return true;
        
        filter = filter.Trim();
        IEnumerable<string> tgts = targets.Where(t => !string.IsNullOrEmpty(t))!;
        var compareInfo = CultureInfo.CurrentCulture.CompareInfo;
        var options = CompareOptions.IgnoreCase | 
                      CompareOptions.IgnoreNonSpace;

        return tgts.Any(t => compareInfo.IndexOf(t.Trim(), filter, options) != -1);
    }
    public static bool IsFilterMatchAtLeastOneOf(string[] filters, params string[] targets)
    {
        return filters.All(f => IsFilterMatchAtLeastOneOf(f, targets));
    }
        
    public static string UnderscoreToUpperCamelCase(this String s)
    {
        if (string.IsNullOrEmpty(s)) return s;
            
        // return Regex.Replace(s1,).ToLower();
        var camelcase = Regex.Replace(s, "_[a-z]", (m) => m.ToString().TrimStart('_').ToUpper());
            
        // Make first letter uppercase
        var firstletter = char.ToUpper(camelcase[0]);
        return (s.Length >= 2) ? firstletter + camelcase[1..] : firstletter.ToString();
    }
        
    public static string TitleCaseToText(this String str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        if (str.Length == 1)
        {
            return str.ToUpper();
        }
            
        // AAA => AAA
        // Abb => Abb
        // ABc => A bc
        // AABcd => AA bcd

        var tmp = Regex.Replace(str, "[A-Z][a-z]", " $0" );
        tmp = tmp.Trim();
        // tmp = Regex.Replace(tmp, " [A-Z]")
        return tmp;
    }
        
        
    // !! TAKEN FROM - https://github.com/aaubry/YamlDotNet/blob/master/YamlDotNet/Serialization/Utilities/StringExtensions.cs !! 
    private static string ToCamelOrPascalCase(string str, Func<char, char> firstLetterTransform)
    {
        var text = Regex.Replace(str, "([_\\-])(?<char>[a-z])", match => match.Groups["char"].Value.ToUpperInvariant(), RegexOptions.IgnoreCase);
        return firstLetterTransform(text[0]) + text.Substring(1);
    }
        
    /// <summary>
    /// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
    /// camel case (thisIsATest). Camel case is the same as Pascal case, except the first letter
    /// is lowercase.
    /// </summary>
    /// <param name="str">String to convert</param>
    /// <returns>Converted string</returns>
    public static string ToCamelCase(this string str)
    {
        return ToCamelOrPascalCase(str, char.ToLowerInvariant);
    }

    /// <summary>
    /// Convert the string with underscores (this_is_a_test) or hyphens (this-is-a-test) to 
    /// pascal case (ThisIsATest). Pascal case is the same as camel case, except the first letter
    /// is uppercase.
    /// </summary>
    /// <param name="str">String to convert</param>
    /// <returns>Converted string</returns>
    public static string ToPascalCase(this string str)
    {
        return ToCamelOrPascalCase(str, char.ToUpperInvariant);
    }

    /// <summary>
    /// Convert the string from camelcase (thisIsATest) to a hyphenated (this-is-a-test) or 
    /// underscored (this_is_a_test) string
    /// </summary>
    /// <param name="str">String to convert</param>
    /// <param name="separator">Separator to use between segments</param>
    /// <returns>Converted string</returns>
    public static string FromCamelCase(this string str, string separator)
    {
        // Ensure first letter is always lowercase
        str = char.ToLower(str[0]) + str.Substring(1);

        str = Regex.Replace(str.ToCamelCase(), "(?<char>[A-Z])", match => separator + match.Groups["char"].Value.ToLowerInvariant());
        return str;
    }

    public static string HideTextPartially(this string str, int hideStartIndex, int hideStopIndex)
    {
        return str.Substring(0, hideStartIndex) + new string('*', hideStopIndex - hideStartIndex) +
               str.Substring(hideStopIndex);
    }

    public static string HideEmailPartially(this string str)
    {
        return str.HideTextPartially(1, str.IndexOf('@'));
    }

    public static string GetEmailDomain(this string str)
    {
        var spl = str.Split("@");
        return (spl.Length > 1) ? spl[1] : "";
    }
    
    public static bool IsEqualIgnoreWsAndDiacritics(this string str, string other)
    {
        var result = string.Compare(str, other, CultureInfo.CurrentCulture, 
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
        return result == 0;
    }

    public static string GetAsString(this HeaderList headerList)
    {
        using var memstream = new MemoryStream();
        headerList.WriteTo(memstream);
        memstream.Position = 0;
        return Encoding.UTF8.GetString(memstream.ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <param name="newlineSequence"></param>
    /// <returns></returns>
    public static string HtmlToText(this string str, string newlineSequence = "\n")
    {
        // Get rid of line breaks and merge multiple whitespaces
        str = str.ReplaceLineEndings("");
        str = Regex.Replace(str, @"\s\s+", " ");
        
        // Transfer breakline and paragraph tags to newlins
        str = str.Replace("<br/>", newlineSequence);
        str = str.Replace("<br>", newlineSequence);
        str = str.Replace("</p>", newlineSequence + newlineSequence);
        
        // Now strip all remaining tags
        str = Regex.Replace(str, "<[^>]*>", "");

        return str;
    }


    public static PropertyBuilder<List<string>> ToStringListProperty(this PropertyBuilder<List<string>> propertyBuilder)
    {
        propertyBuilder.HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?) null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?) null)!,
            new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode()))
            ));

        return propertyBuilder;
    } 
    
    public static PropertyBuilder<TProperty> ToJsonConvertedProperty<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
    {
        propertyBuilder.HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?) null),
            v => JsonSerializer.Deserialize<TProperty>(v, (JsonSerializerOptions?) null)!);
        
        // TODO- is value comparer needed?

        return propertyBuilder;
    } 
}