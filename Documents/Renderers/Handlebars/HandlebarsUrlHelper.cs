using HandlebarsDotNet;
using sip.Core;

namespace sip.Documents.Renderers.Handlebars;

public class HandlebarsUrlHelper(ILogger<HandlebarsUrlHelper> logger, IOptions<AppOptions> options)
    : IHandlebarsInlineHelper
{
    public string Name => "link";

    public void Execute(EncodedTextWriter output, Context context, Arguments arguments)
    {
        logger.LogDebug("Executing url helper with context type {Ctxtype} and {Argcount} arguments", context.Value.GetType().FullName, arguments.Count());
        var urldetails = ParseUrlArguments(arguments);
        var urlstring = $"<a href=\"{urldetails.href}\">{urldetails.display}</a>";

        output.WriteSafeString(urlstring);
        logger.LogDebug("Generated new url: {Urlstring}", urlstring);
    }

    public (string href, string display) ParseUrlArguments(Arguments arguments)
    {
        var firstarg = arguments.First();
        var href = firstarg.ToString();
        
        // Sanity check
        if (string.IsNullOrEmpty(href))
            throw new ArgumentException("Url is missing in the arguments of url helper.");
        
        // If url href is relative, absolutize it using url base from application options
        if (!href.StartsWith("http://") && !href.StartsWith("https://"))
        {
            href = new Uri(options.Value.UrlBase, href).ToString();
        }

        var display = href;
        
        // The second argument shoud be url description - what is shown to the user instead of raw link
        if (arguments.Length >= 2)
        {
            display = arguments[1].ToString() ?? "Link";
        }

        
        
        // The third argument is then name of the link, used in link summary
        // if (arguments.Length >= 3)
        // {
        //     result.Name = arguments[2].ToString();
        // }
        return (href, display);
    }
}