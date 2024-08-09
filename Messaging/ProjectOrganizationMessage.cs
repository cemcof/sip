namespace sip.Messaging;

public class ProjectOrganizationMessage : Message
{
    public string ProjectId { get; set; } = null!;
    public Project Project { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;
    public Organization Organization { get; set; } = null!;

}