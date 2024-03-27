namespace sip.Documents.Renderers;

public interface IDocRenderer
{
    Task<MimePart> Render(MimePart source, object context);
    bool SupportsType(ContentType contentType);
}