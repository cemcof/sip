using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sip.Documents.Proposals;

namespace sip.CEITEC.DirectAccess.Proposals.Creation;

public class DCreationProposal : Proposal
{
    public DProposalFormModel DProposalFormModel { get; set; } = new();
}

public class DProposalFormModel;

public class DProposalEConf : IEntityTypeConfiguration<DCreationProposal> 
{
    public void Configure(EntityTypeBuilder<DCreationProposal> builder)
    {
        builder.Property(cp => cp.DProposalFormModel)
            .ToJsonConvertedProperty();
    }
}
