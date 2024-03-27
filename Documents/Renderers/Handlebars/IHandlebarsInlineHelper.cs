using HandlebarsDotNet;

namespace sip.Documents.Renderers.Handlebars;

public interface IHandlebarsInlineHelper
{
    string Name { get; }
    void Execute(EncodedTextWriter output, Context context, Arguments arguments);
}