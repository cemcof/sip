using sip.Documents.Proposals;

namespace sip.CEITEC.CIISB.Proposals.TechnicalFeasibility;

public enum TechFeasibilityResult
{
    Accepted = 1,
    Rejected = 0,
    AcceptedUpon = 2
}

public class TechnicalFeasiblility : Proposal
{

    public TechFeasibilityResult Result { get; set; }
    public string Comments { get; set; } = string.Empty;
}