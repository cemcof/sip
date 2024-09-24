using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace sip.Documents;

public class FileMetadata : IIdentified<Guid>
{
    public Guid Id { get; set; }

    [JsonIgnore] public List<FileInDocument> FileInDocuments { get; set; } = new();

    public string FileName { get; set; } = string.Empty;

    [NotMapped]
    public string FileNameNormalized
    {
        get
        {
            MimeKit.MimeTypes.TryGetExtension(ContentType.MimeType, out var ext);
            var fname = FileName.ToLower().Replace(" ", "_");
            return (!fname.EndsWith(ext)) ? fname.TrimEnd('.') + "." + ext : fname;
        }
    }

    public int Length { get; set; }
    public DateTime DtCreated { get; set; }
    public DateTime DtModified { get; set; }
    public ContentType ContentType { get; set; } = null!;
    public Guid FileDataId { get; set; }
    [MaybeNull] public FileData FileData { get; set; } = null!;

    public FileMetadata(string fileName, string contentType)
    {
        FileName = fileName;
        ContentType = ContentType.Parse(contentType);
    }
    
    public FileMetadata() { }
}

public class FileMetadataEntityTypeConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {
        builder.Property(f => f.ContentType)
            .HasConversion(v => v.ToMimeTypeString(), s => ContentType.Parse(s));

        builder.HasOne(f => f.FileData)
            .WithMany()
            .HasForeignKey(fm => fm.FileDataId);
    }
}