namespace sip.Projects;

public class ProjectContext(Project project)
{
    public Organization? Organization { get; set; }
    public AppUser? User { get; set; }

    public Project Project { get; set; } = project;


    public string NewProposalLink { get; set; } = string.Empty;
    public string ProjectLink { get; set; } = string.Empty;
    public string ExtensionLink { get; set; } = string.Empty;

}