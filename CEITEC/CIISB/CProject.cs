using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sip.CEITEC.CIISB.Proposals.Creation;
using sip.CEITEC.CIISB.Proposals.Extension;
using sip.CEITEC.CIISB.Proposals.PeerReview;
using sip.CEITEC.CIISB.Proposals.TechnicalFeasibility;

namespace sip.CEITEC.CIISB;

public enum CProjectType
{
    Auto, Internal, External
}

public class CProject : Project
{
    // CIISB projects have DOI publications
    public List<string> Publications { get; set; } = new();
    public CProjectType ProjectType { get; set; }

    public bool IsInternal => ActualProjectType is CProjectType.Internal;
    public bool IsExternal => ActualProjectType is CProjectType.External;
    
    [NotMapped] public CCreationProposal CreationProposal => Proposals.OfType<CCreationProposal>().First();
    [NotMapped] public CExtensionProposal? ExtensionProposal => Proposals.OfType<CExtensionProposal>().FirstOrDefault();
    [NotMapped] public PeerReview? PeerReviewProposal => Proposals.OfType<PeerReview>().FirstOrDefault();
    [NotMapped] public IEnumerable<TechnicalFeasiblility> TechnicalFeasiblilityProposal => Proposals.OfType<TechnicalFeasiblility>();
    [NotMapped] public AppUser Applicant => ProjectMembers.First(m => m.MemberType == nameof(ApplicantMember)).MemberUser;
    [NotMapped] public AppUser Principal => ProjectMembers.First(m => m.MemberType == nameof(PrincipalMember)).MemberUser;
    
    [NotMapped] public CProjectType ActualProjectType {
        get
        {
            if (ProjectType != CProjectType.Auto) return ProjectType;
            if (Regex.IsMatch(AffiliationDetails.Name, "(masaryk university|ceitec|institute of biotechnology av cr|biocev|, mu)",
                    RegexOptions.IgnoreCase))
            {
                return CProjectType.Internal;
            }
        
            var mailPostfix =  Applicant.Email?.Split("@").Last();
            if (!string.IsNullOrEmpty(mailPostfix) && Regex.IsMatch(mailPostfix, @"(muni\.|ceitec\.|ibt\.cas\.)"))
            {
                return CProjectType.Internal;
            }

            return CProjectType.External;
        }
    }

    [NotMapped]
    public bool PeerReviewRequired =>
        (ActualProjectType == CProjectType.External &&
         !Title.ToLower().Contains("covid") &&
         (ParentId == default));

    [NotMapped] public string ContactAddress => AffiliationDetails.Address + ", " + AffiliationDetails.Country;
    [NotMapped] public string InstitutaionAbbreviation
    {
        get
        {
            var orgname = AffiliationDetails.Name;
            var words = orgname.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2) return orgname.Substring(0, 2);
            var irrelevants = new[] {"and", "of", "from", "in", "at", "the", "a"};
            var relevantonly = words.Where(w => !irrelevants.Contains(w)).ToArray();
            var word1 = relevantonly.FirstOrDefault() ?? "institute";
            var word2 = (relevantonly.Length >= 2) ? relevantonly[1] : "institute";
            return (word1.Substring(0, 1) + word2.Substring(0, 1)).ToUpper();
        }
    }

    [NotMapped]
    public IEnumerable<string> CeitecOrgs =>
        Organizations.Where(o => o.IsOrParent<Ceitec>()).Select(c => c.Abbreviation);

    [NotMapped] public string Institution => AffiliationDetails.Name;

    public bool IsGtcRequired()
        => IsGtcRequired<Ceitec>() || IsGtcRequired<Biocev>();
    
    public bool IsGtcRequired<TResearchCenter>() where  TResearchCenter : OrganizationDefinition
    {
        if (typeof(TResearchCenter) == typeof(Ceitec))
        {
            return ((ActualProjectType is not CProjectType.Internal &&
                    Organizations.Any(o => o.IsOrParent<Ceitec>())) ||
                    (ActualProjectType is CProjectType.Internal) && IsBiocevApplicant &&
                    Organizations.Any(o => o.IsOrParent<Ceitec>()));
        }

        if (typeof(TResearchCenter) == typeof(Biocev))
        {
            return Organizations.Any(o => o.IsOrParent<Biocev>());
        }

        return false;
    }

    [NotMapped] public bool IsBiocevApplicant
    {
        get
        {
            if (AffiliationDetails.Name.ToLower().Contains("biocev")) return true;
            if (Applicant.Email != null && Applicant.Email.GetEmailDomain().ToLower().Contains("ibt.cas.")) return true;
            return false;
        }
    }
}


public class CProjectEntityDefinition : IEntityTypeConfiguration<CProject>
{
    public void Configure(EntityTypeBuilder<CProject> builder)
    {
        builder.Property(p => p.Publications)
            .ToStringListProperty();
        
        
    }
}
