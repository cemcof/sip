namespace sip.CEITEC.CIISB;

public class CProjectContext : ProjectContext
{
    public CProject CProject => (CProject) Project;

    public string PeerReviewUrl =>
        $"/proposal/{(CProject.PeerReviewProposal ?? throw new InvalidOperationException()).Id}/submit";
    
    public CProjectContext(CProject project) : base(project)
    {
        ProjectLink = $"/projects/{project.Id}";
    }
    
    
}