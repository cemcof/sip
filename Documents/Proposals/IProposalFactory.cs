namespace sip.Documents.Proposals;

public interface IProposalFactory<TProposal> where TProposal : Proposal 
{
    Task<TProposal> CreateProposalAsync();
}