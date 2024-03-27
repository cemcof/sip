using HandlebarsDotNet;

namespace sip.Documents.Renderers.Handlebars;

public interface IHandlebarsService
{   
    /// <summary>
    /// Render given string text by applying given context. 
    /// </summary>
    /// <param name="template"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    string Render(string template, object context);
}

/// <summary>
/// Wrapper over default <see cref="IHandlebars"/> interface to fit application better (especially support DI and streamline configuration)
/// </summary>
public class HandlebarsService : IHandlebarsService, IDocRenderer
{
    private readonly ILogger<HandlebarsService> _logger;
    private readonly IHandlebars _handlebars;

    public HandlebarsService(IEnumerable<IHandlebarsInlineHelper> inlineHelpers, ILogger<HandlebarsService> logger)
    {
        _logger = logger;
        _handlebars = HandlebarsDotNet.Handlebars.Create();
        _logger.LogDebug("Initializing handlebars...");
        
        // Register inline helpers 
        foreach (var inlineHelper in inlineHelpers)
        {
            _logger.LogDebug("Registered handlebars helper: {}", inlineHelper.Name);
            _handlebars.RegisterHelper(inlineHelper.Name, inlineHelper.Execute);
        }
    }


    public string Render(string template, object context)
    {
        // TODO - implement caching or just compile every time?
        var tmplt = _handlebars.Compile(template);
        return tmplt(context);
    }

    public async Task<MimePart> Render(MimePart source, object context)
    {
        if (!SupportsType(source.ContentType))
            throw new FormatException(
                $"{nameof(HandlebarsService)} does not support rendering {source.ContentType} source");
        
        // For now, we need to use strings in order to user handlebars, since it does not seem to support streams
        var str = await source.Content.MimeContentToString();
        var resultStr = Render(str, context);
        var resultMs = new MemoryStream();
        await new StreamWriter(resultMs).WriteAsync(resultStr);
        var resultMp = new MimePart(source.ContentType)
        {
            Content = new MimeContent(resultMs)
        };
        
        return resultMp;
    }

    public bool SupportsType(ContentType contentType) => contentType.IsTextual();
}