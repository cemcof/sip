namespace sip.Projects;

public class ProjectMember
{
    public Guid Id { get; set; }

    public string MemberType { get; set; } = string.Empty;

    public AppUser MemberUser { get; set; } = null!;
    public Guid MemberUserId { get; set; }

    public Project Project { get; set; } = null!;
    public string ProjectId { get; set; } = null!;

    public string? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}

public class MemberRef;
public class OwnerMember : MemberRef;