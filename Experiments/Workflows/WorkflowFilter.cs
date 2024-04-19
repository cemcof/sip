namespace sip.Experiments.Workflows;

public record WorkflowFilter(
    IOrganization Organization,
    string[] Tags)
{

    public bool HasTags(params string[] tags)
    { 
        
    }
};