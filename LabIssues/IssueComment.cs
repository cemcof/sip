namespace sip.LabIssues;

public class IssueComment
{
    public Guid Id { get; set; }
    
    public string IssueId { get; set; }
    public Issue Issue { get; set; }

    public AppUser? Author { get; set; }
    public string IpAddress { get; set; }

    public DateTime DtCreated { get; set; }
    
    public string Comment { get; set; }
}

// Entity configuration
