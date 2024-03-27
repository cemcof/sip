namespace sip.Documents.Proposals;

public interface IDocumentSubmitter<in TDocument> where TDocument : Document
{
    Task SubmitDocumentAsync(TDocument document);
}

