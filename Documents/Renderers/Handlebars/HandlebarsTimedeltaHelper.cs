using HandlebarsDotNet;

namespace sip.Documents.Renderers.Handlebars;

public class HandlebarsTimedeltaHelper(ILogger<HandlebarsTimedeltaHelper> logger, TimeProvider timeProvider)
    : IHandlebarsInlineHelper
{
    public string Name => "timedelta";

    public void Execute(EncodedTextWriter output, Context context, Arguments arguments)
    {
        logger.LogDebug("Executing date helper with context type {} and {} arguments", context.Value.GetType().FullName, arguments.Count());

        var timedelta = arguments.Length switch
        {
            > 0 when arguments[0] is DateTime dt => dt.HumanizeTimedelta(compareAgainst: timeProvider.DtUtcNow()),
            > 0 when arguments[0] is TimeSpan ts => ts.Humanize(),
            _ => throw new ArgumentException("Invalid date helper handlebars argument. Must be of DateTime or TimeSpan type")
        };

        output.WriteSafeString(timedelta);
    }
}
