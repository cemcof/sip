using System.ComponentModel.DataAnnotations.Schema;

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
    public Project? Project   { get; set; }

    /// <summary>
    /// A name of the document entity that further identifies what type this document is.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public List<FileInDocument> FilesInDocuments { get; set; } = new();

    [NotMapped]
    public IEnumerable<FileInDocument> ActivePrimaryFiles =>
        FilesInDocuments.Where(fid => fid.Active && fid.DocumentFileType == DocumentFileType.Primary);

    [NotMapped]
    public IEnumerable<FileMetadata> Attachments => FilesInDocuments
        .Where(f => f.DocumentFileType == DocumentFileType.Attachment).Select(f => f.FileMetadata);
}