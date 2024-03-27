namespace sip.Forms;

public class SelectionAttribute(params string[] keyvalues) : Attribute
{
    public string[] Options { get; } = keyvalues;

    public bool ItemsCollapsible { get; set; }

    public IEnumerable<KeyValuePair<string, string>> GetKeyValues()
    {
        foreach (var option in Options)
        {
            var pieces = option.Split("=>");
            var key = pieces.First();
            var value = pieces.Length > 1 ? pieces[1] : key.Humanize();
            yield return new KeyValuePair<string, string>(key, value);
        }
    }
        
}