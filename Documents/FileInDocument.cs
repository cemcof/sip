using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sip.Documents;

public enum DocumentFileType
{
    Primary,
    Alternative,
    Attachment,
    Archived
}

public class FileInDocument
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }
    [JsonIgnore]
    public Document Document { get; set; } = null!;
    
    public Guid FileMetadataId { get; set; }
    public FileMetadata FileMetadata { get; set; } = null!;
    public DocumentFileType DocumentFileType { get; set; }
    

    [NotMapped] public bool Active => DocumentFileType is DocumentFileType.Attachment or DocumentFileType.Primary;
}