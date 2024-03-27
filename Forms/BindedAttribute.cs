namespace sip.Forms;

public class BindedAttribute(string target, params string[] values) : Attribute
{
    private readonly string   _target = target;
    private readonly string[] _values = values;

    public bool Disable { get; set; } = true;
}