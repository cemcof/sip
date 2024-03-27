using System.Collections;
using HandlebarsDotNet;

namespace sip.Documents.Renderers.Handlebars;

public class HandelbarsListHelpper : IHandlebarsInlineHelper
{
    public string Name => "list";
    
    public void Execute(EncodedTextWriter output, Context context, Arguments arguments)
    {
        if (arguments[0] is IEnumerable enume)
        {
            output.WriteSafeString(string.Join(", ", enume.Cast<object>().Select(o => o.ToString())));
        }
    }
}