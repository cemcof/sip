using HandlebarsDotNet;
using sip.Core;

namespace sip.Documents.Renderers.Handlebars;

public class HandlebarsUrlBuilderHelper(ILogger<HandlebarsUrlBuilderHelper> logger, IOptions<AppOptions> options)
    : IHandlebarsInlineHelper
{
    public string Name => "link_builder";

    public void Execute(EncodedTextWriter output, Context context, Arguments arguments)
    {
        logger.LogDebug("Executing url builder helper with context type {} and {} arguments", context.Value.GetType().FullName, arguments.Count());
        var hrefBuilder = new UriBuilder(options.Value.UrlBase);
        
        foreach (var argument in arguments)
        {
            hrefBuilder.Path = Path.Combine(hrefBuilder.Path, argument.ToString() ?? "");
            logger.LogDebug("Found URL argument: {} of type {} new href is {}", argument, argument.GetType().Name, hrefBuilder);
        }

        var href = hrefBuilder.ToString();
        var urlstring = $"<a href=\"{href}\">{href}</a>";

        output.WriteSafeString(urlstring);
        logger.LogDebug("Generated new url: {}", urlstring);
    }
    
}