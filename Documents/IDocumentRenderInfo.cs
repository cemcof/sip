using sip.Documents.Proposals;

namespace sip.Documents;

public interface IDocumentRenderInfoProvider<in TDocument>
    where TDocument : Document
{
    DocumentRenderInfo GetRenderInfo(TDocument document);
}