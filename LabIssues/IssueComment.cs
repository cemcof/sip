using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace sip.LabIssues;

public class IssueComment
{
    private Guid _id;
    
    public string IssueId { get; set; } = null!;
    public Issue Issue { get; set; } = null!;

    public Guid? AuthorId { get; set; }
    public AppUser? Author { get; set; }
    
    public string IpAddress { get; set; }

    public DateTime DtCreated { get; set; }

    public string Comment { get; set; } = string.Empty;

    public IssueComment(string ipAddress, string comment = "")
    {
        IpAddress = ipAddress;
        Comment = comment;
    }
}

// Entity configuration
public class IssueCommentEntityConfig : IEntityTypeConfiguration<IssueComment>
{
    public void Configure(EntityTypeBuilder<IssueComment> builder)
    {
        builder.HasKey("_id");
        
        builder.Property(ic => ic.IpAddress)
            .HasMaxLength(42);

        
    }
}

