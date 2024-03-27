using HandlebarsDotNet;
using sip.Core;

namespace sip.Documents.Renderers.Handlebars;

public class HandlebarsDateHelper(ILogger<HandlebarsDateHelper> logger, IOptions<AppOptions> options)
    : IHandlebarsInlineHelper
{
    private readonly IOptions<AppOptions>          _options = options;

    public string Name => "date";

    public void Execute(EncodedTextWriter output, Context context, Arguments arguments)
    {
        logger.LogDebug("Executing date helper with context type {} and {} arguments", context.Value.GetType().FullName, arguments.Count());

        if (arguments.Length > 0 && arguments[0] is DateTime dt)
        {
            var dtFormatted = dt.ToString("dd.MM.yyyy");
            output.WriteSafeString(dtFormatted);
        }
        else throw new ArgumentException("Invalid date helper handlebars argument. Must be of DateTime type");
    }
    
}