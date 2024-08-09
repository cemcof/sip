namespace sip.Documents.Proposals;

public enum ProposalState
{
    WaitingForSubmission, 
    Submitted,
    WaitingForEvaluation,
    Evaluated
}

/// <summary>
/// Proposal is a document type that optionally links to concrete project and organization
/// and has several related properties and links to users.
/// </summary>
public class Proposal : Document
{
    public Guid? ExpectedEvaluatorId { get; set; }
    public AppUser? ExpectedEvaluator { get; set; }

    public Guid? EvaluatedById { get; set; }
    public AppUser? EvaluatedBy { get; set; }
    
    public DateTime DtEvaluated { get; set; }
    public DateTime DtSubmitted { get; set; }

    public virtual bool IsEvaluated => DtEvaluated != default;

    public ProposalState ProposalState { get; set; }
}