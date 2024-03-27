namespace sip.Documents;

public interface IDocumentFactory<TDocument> where  TDocument : Document
{
    Task<TDocument> CreateDocumentAsync();
}