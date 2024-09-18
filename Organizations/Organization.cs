using System.Text.Json.Serialization;

namespace sip.Organizations;

public class Organization(string id, string linkId, string name, string abbreviation) : 
    IOrganization, 
    ITreeItem<Organization>, 
    IStringFilter,
    IIdentified<string>
{
    public string Id { get; } = id;
    public string LinkId { get; set; } = linkId;
    public string Name { get; set; } = name;
    public string Abbreviation { get; set; } = abbreviation;
    
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    
    // Tree structure - if all organizations are loaded at once, there is no need to perform join for each layer - ef core will automatically fill up the tree from loaded flat collection.
    public Organization? Parent { get; set; }
    [JsonIgnore] IOrganization? ITreeItem<IOrganization>.Parent => Parent;

    
    public string? ParentId { get; set; }
    [JsonIgnore] public ICollection<Organization> Children { get; set; } = new List<Organization>();
    [JsonIgnore] ICollection<IOrganization> ITreeItem<IOrganization>.Children => Children.Cast<IOrganization>().ToList();

    public bool Is<TOrganizationRef>()
        => typeof(TOrganizationRef).Name == Id;
    
    public bool IsOrParent<TOrganizationRef>()
        => Tree<Organization>.EnumerateToRoot(this).Any(o => o.Id == typeof(TOrganizationRef).Name);

    public override string ToString() 
        => $"{Name} ({Id})";

    public bool IsFilterMatch(string? filter = null)
        => StringUtils.IsFilterMatchAtLeastOneOf(filter, Name, Id, Abbreviation, Description);
}