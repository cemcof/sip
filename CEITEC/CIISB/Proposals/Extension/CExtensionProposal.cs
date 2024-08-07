using sip.Documents.Proposals;

namespace sip.CEITEC.CIISB.Proposals.Extension;

public enum ExtensionResult
{
    [Render(Title = "The project will not lead any publishable results")]
    NoResult,
    [Render(Title = "No publications available so far")]
    NoPublicationSoFar,
    [Render(Title = "The outcomes of the project are part of the following publications")]
    Publications
}


public class CExtensionProposal : Proposal
{
    public string Justification { get; set; } = string.Empty;
    public ExtensionResult ExtensionResult { get; set; }
}