using Microsoft.AspNetCore.Components;
using InvalidOperationException = System.InvalidOperationException;

namespace sip.LabIssues;

public class IssueCreate
{
    public Organization Organization { get; }
    public DateTime DtCreated { get; }
    public IPAddr IpAddress { get; }
    
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AppUser InitiatedBy { get; set; }

    
    public IssueCreate() { }
    public IssueCreate(
        string ip, 
        Organization organization, 
        TimeProvider timeProvider,
        AppUser? creator = null)
    {
        // IpAddress = user.GetRemoteIp() ?? throw new InvalidOperationException("Missing ip in claims principal");
        DtCreated = timeProvider.DtUtcNow();
        Organization = organization;
    }
}