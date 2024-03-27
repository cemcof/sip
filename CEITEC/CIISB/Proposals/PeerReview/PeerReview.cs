using sip.Documents.Proposals;

namespace sip.CEITEC.CIISB.Proposals.PeerReview;

public enum PeerReviewResult
{
    Accepted = 1,
    Rejected = 0
}

public class PeerReview : Proposal
{

    public PeerReviewResult Result { get; set; }
    public string Comments { get; set; }
}