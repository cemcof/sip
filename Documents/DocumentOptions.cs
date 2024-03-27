namespace sip.Documents;

public class DocumentOptions
{
    [Required] public Type DocumentType { get; private set; } = null!;
    [Required] public string Name { get; set; } = null!;


    public void UseDocumentType<TDocumentType>() where TDocumentType : Document
    {
        DocumentType = typeof(TDocumentType);
    }
    
}