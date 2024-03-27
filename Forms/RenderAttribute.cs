namespace sip.Forms;

public enum RenderingStrategy
{
    Default,
    Inline,
    Block
}

public class RenderAttribute : Attribute
{
    public string? Title { get; set; }
    public string? GroupStart { get; set; }
    
    public string? GroupClass { get; set; }
    public string? Unit { get; set; }
    public bool NoTitle { get; set; } = false;

    // For rendering forms
    public string? Tip { get; set; }
    public string? NoteBefore { get; set; }
    public string? NoteIn { get; set; }
    public string? NoteAfter { get; set; }

    public RenderingStrategy RenderingStrategy { get; set; } = RenderingStrategy.Block;
    public Sizing Sizing { get; set; }

    public bool Ignore { get; set; }

    public bool AsNotOption { get; set; } = false;
}