using HandlebarsDotNet;

namespace sip.Documents.Renderers.Handlebars;

public class HandlebarsDateStandardHelper : IHandlebarsInlineHelper
{
    public string Name => "date_standard";
    
    public void Execute(EncodedTextWriter output, Context context, Arguments arguments)
    {
        if (arguments[0] is DateTime dt)
        {
            output.WriteSafeString(dt.Date.StandardFormat());
        }
    }
}