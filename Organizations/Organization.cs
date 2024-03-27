using System.ComponentModel.DataAnnotations.Schema;
using sip.Utils;

namespace sip.Organizations;

public class Organization(string id) : 
    IOrganization, 
    ITreeItem<Organization>, 
    IEquatable<Organization>, 
    IStringFilter
{
    #region EQUALS_BOILERPLATE

    

        public bool Equals(Organization? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Organization other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool IsFilterMatch(string filter = "")
        {
            return StringUtils.IsFilterMatchAtLeastOneOf(filter, Name, Id, Abbreviation, Description);
        }

        public static bool operator ==(Organization? left, Organization? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Organization? left, Organization? right)
        {
            return !Equals(left, right);
        }
    #endregion

    [StringLength(128), Key] public string Id { get; } = id;
    public string LinkId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    
    // Tree structure - if all organizations are loaded at once, there is no need to perform join for each layer - ef core will automatically fill up the tree from loaded flat collection.
    public Organization? Parent { get; set; }
    IOrganization? ITreeItem<IOrganization>.Parent => Parent;


    public string? ParentId { get; set; }
    public ICollection<Organization> Children { get; set; } = new List<Organization>();
    ICollection<IOrganization> ITreeItem<IOrganization>.Children => Children.Cast<IOrganization>().ToList();

    public bool Is<TOrganizationRef>()
        => typeof(TOrganizationRef).Name == Id;
    
    public bool IsOrParent<TOrganizationRef>()
        => Tree<Organization>.EnumerateToRoot(this).Any(o => o.Id == typeof(TOrganizationRef).Name);
    
}