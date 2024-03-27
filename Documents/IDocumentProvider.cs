namespace sip.Documents;

public interface IDocumentProvider
{
    Task<Document?> GetDocumentAsync(Guid id);
}

public interface IDocumentProvider<TDocument> where TDocument : Document
{
    Task<TDocument?> GetDocumentAsync(Guid id);
}