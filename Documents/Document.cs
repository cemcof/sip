using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace sip.Documents;

public class Document
{
    public Guid Id { get; set; }

    public string? OrganizationId { get; set; }

    /// <summary>
    /// ggg
    /// </summary>
    public Organization? Organization { get; set; }


    public string?  ProjectId { get; set; }
    [JsonIgnore] public Project? Project   { get; set; }

    /// <summary>
    /// A name of the document entity that further identifies what type this document is.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public List<FileInDocument> FilesInDocuments { get; set; } = new();

    [NotMapped, JsonIgnore]
    public IEnumerable<FileInDocument> ActivePrimaryFiles =>
        FilesInDocuments.Where(fid => fid.Active && fid.DocumentFileType == DocumentFileType.Primary);

    [NotMapped, JsonIgnore]
    public IEnumerable<FileMetadata> Attachments => FilesInDocuments
        .Where(f => f.DocumentFileType == DocumentFileType.Attachment).Select(f => f.FileMetadata);
}