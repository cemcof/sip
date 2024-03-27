namespace sip.Organizations.Centers;

public interface INodeStatusProvider
{
    Task<Dictionary<string, CenterNodeStatus>> GetNodesStatusAsync(IOrganization organizationSubject);
}