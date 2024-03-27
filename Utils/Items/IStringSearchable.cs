namespace sip.Utils.Items;

public interface IStringSearchable
{
    string? SearchText { get; set; }
}

public class StringSearchFilter : IStringSearchable
{
    public string? SearchText { get; set; }
}