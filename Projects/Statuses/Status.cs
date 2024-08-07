using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace sip.Projects.Statuses;

public class Status
{
    public Guid Id { get; set; }

    public string StatusInfoId { get; set; } = null!;
    public StatusInfo StatusInfo { get; set; } = null!;

    public Status? EnteredFromStatus { get; set; }
    public Guid? EnteredFromStatusId { get; set; }
    public DateTime DtEntered { get; set; }

    public Status? LeftToStatus { get; set; }
    public Guid? LeftToStatusId { get; set; }

    public DateTime DtLeft { get; set; }

    public bool Active { get; set; }

    public Organization Organization { get; set; } = null!;
    public string OrganizationId { get; set; } = null!;

    public Project Project { get; set; } = null!;
    public string ProjectId { get; set; } = null!;
}

public class StatusEntityTypeDefinition : IEntityTypeConfiguration<Status>
{
    public void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.HasOne(s => s.LeftToStatus).WithMany().HasForeignKey(s => s.LeftToStatusId);
        builder.HasOne(s => s.EnteredFromStatus).WithMany().HasForeignKey(s => s.EnteredFromStatusId);
    }
}