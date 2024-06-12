namespace sip.Utils;

/// <summary>
/// Helper class for simple tagging technique.
/// Filtering tags are just strings in an array.
/// Tags in the array represent AND condition.
/// In the strings tags, multiple tags can be given, separated by comma. That is OR condition.
/// </summary>
public class TagFilter
{
    private readonly List<string[]> _tags;
    public TagFilter(IEnumerable<string> tags)
    {
        _tags = tags
            .Select(t => t.SplitWithTrim(",", ";"))
            .ToList();
    }
    
    public static implicit operator TagFilter(string[] tags) => new(tags);
    public static implicit operator TagFilter(List<string> tags) => new(tags);

    public bool Match(IEnumerable<string?> givenTags, StringComparer? comparer = null)
    {
        if (comparer is null) comparer = StringComparer.OrdinalIgnoreCase;
        // Ignore empty/null tags
        givenTags = givenTags.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
        var match = _tags.All(
            ft => ft.Any(t => givenTags.Contains(t, comparer))
        );
        return match;
    }

    public bool Match(params string?[] givenTags) => Match(givenTags.AsEnumerable());
    
    public override string ToString() =>
        "TagFilter {" +
            string.Join("; ", _tags.Select(t => string.Join(", ", t))) +
        "}";
}

