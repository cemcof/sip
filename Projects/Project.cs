using System.ComponentModel.DataAnnotations.Schema;
using sip.Documents;
using sip.Documents.Proposals;
using sip.Messaging;
using sip.Organizations;
using sip.Projects.Statuses;
using sip.Userman;
using sip.Utils;

namespace sip.Projects;

public class  Project : ITreeItem<Project>, IStringIdentified, IEquatable<Project>
{
    #region EQUALS_BOILERPLATE
        public bool Equals(Project? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Project other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Project? left, Project? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Project? left, Project? right)
        {
            return !Equals(left, right);
        }
    #endregion
    
    [MaxLength(128)] public string Id { get; set; } = null!;
    
    [MaxLength(256)] public string Acronym { get; set; } = string.Empty;
    [MaxLength(1024)] public string Title { get; set; } = string.Empty;

    public Project? Parent { get; set; } 
    public string? ParentId { get; set; }
    public ICollection<Project> Children { get; set; } = new List<Project>();

    public bool Closed { get; set; }
    
    /// <summary>
    /// Flags the project as disabled, such project will not be included in any workflows or list of projects.
    /// </summary>
    public bool Disabled { get; set; }

    public DateTime DtExpiration { get; set; }
    public DateTime DtCreated { get; set; }

    public List<Status> ProjectStatuses { get; set; } = new();
    public List<ProjectMember> ProjectMembers { get; set; } = new();
    public List<Document> ProjectDocuments { get; set; } = new();

    public List<ProjectOrganizationMessage> ProjectOrganizationMessages { get; set; } = new();
    
    [NotMapped] public IEnumerable<Proposal> Proposals => ProjectDocuments.OfType<Proposal>();
    

    [NotMapped] public IEnumerable<AppUser> AdditionalMembers =>
        ProjectMembers.Where(m => m.MemberType == nameof(AdditionalMembers)).Select(m => m.MemberUser);

    [NotMapped] public IEnumerable<Status> Statuses => ProjectStatuses.Where(ps => ps.Active);
    [NotMapped] public IEnumerable<Organization> Organizations => Statuses.Select(s => s.Organization).Distinct();

    [NotMapped] public IEnumerable<string> OrganizationsAbbreviations => Organizations.Select(o => o.Abbreviation);
    [NotMapped] public IEnumerable<Organization> LeafOrganizations => Organizations.Where(o => o.Children.Count == 0);

    // TODO - these are virtual - should respect Statuses
    // [NotMapped] public virtual Organization BaseOrganization { get; set; }

    public AffiliationDetails AffiliationDetails { get; set; } = null!;

    public bool InStatusAny<TStatus, TOrg>()
    {
        var orgName = typeof(TOrg).Name;
        foreach (var astatus in Statuses.Where(s => s.StatusInfoId == typeof(TStatus).Name))
        {
            var toroot = Tree<Organization>.EnumerateToRoot(astatus.Organization);
            if (toroot.Any(o => o.Id == orgName)) return true;
        }

        return false;
    }
    
    public bool InStatusAny<TStatus>(Organization org)
    {
        foreach (var astatus in Statuses.Where(s => s.StatusInfoId == typeof(TStatus).Name))
        {
            var toroot = Tree<Organization>.EnumerateToRoot(astatus.Organization);
            if (toroot.Any(o => org == o)) return true;
        }

        return false;
    }
    
    public bool InStatusAny<TStatus>(IEnumerable<Organization> orgs)
        => orgs.Any(InStatusAny<TStatus>);

    public bool InStatusAll<TStatus>()
    {
        return Statuses.All(s => s.StatusInfoId == typeof(TStatus).Name);
    }
    
    public bool InStatusAll<TStatus>(IEnumerable<Organization> organizations)
    {
        var statuses = Statuses.ToList();
        return organizations.All(o =>
            statuses.Any(s => s.OrganizationId == o.Id && s.StatusInfoId == typeof(TStatus).Name));
    }
    
    public IEnumerable<Organization> GetOrgsInStatus<TStatusRef>()
        where TStatusRef : StatusDefinition
    {
        var statusId = nameof(TStatusRef);
        return Statuses.Where(s => s.StatusInfoId == statusId).Select(s => s.Organization).Distinct();
    }
    
    public IEnumerable<ProjectMember> GetMembersForUser(AppUser user)
    {
        return ProjectMembers.Where(pm => pm.MemberUser.Equals(user));
    }

    public bool AnyMember<TMember>(Organization organization)
        where TMember : MemberRef
        => ProjectMembers.Any(pm => pm.MemberType == typeof(TMember).Name && pm.OrganizationId == organization.Id);

    public bool IsMember<TMember>(AppUser user, Organization organization)
        where TMember : MemberRef
        => ProjectMembers.Any(pm => pm.MemberUserId == user.Id && pm.MemberType == typeof(TMember).Name && pm.OrganizationId == organization.Id);
}



[Owned]
public class AffiliationDetails
{
    [MaxLength(1024)] public string Name { get; set; } = string.Empty;
    [MaxLength(256)] public string Type { get; set; } = string.Empty;
    [MaxLength(256)] public string Address { get; set; } = string.Empty;
    [MaxLength(256)] public string Country { get; set; } = string.Empty;
}