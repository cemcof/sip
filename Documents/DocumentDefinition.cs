using sip.Utils;

namespace sip.Documents;

public class DocumentDefinition<TDocumentType> : INamedSetup<DocumentOptions> where TDocumentType : Document
{
    public void Setup(DocumentOptions opts)
    {
        opts.Name = Name;
        opts.UseDocumentType<TDocumentType>();
    }

    public virtual string Name => GetType().Name;
}