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

    public bool Match(IEnumerable<string?> givenTags)
    {
        // Ignore empty/null tags
        givenTags = givenTags.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
        
        var match = _tags.All(
            ft => ft.Any(givenTags.Contains)
        );

        return match;
    }

    public bool Match(params string?[] givenTags) => Match(givenTags.AsEnumerable());
}

