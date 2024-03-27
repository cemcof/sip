namespace sip.Organizations;

public class OrganizationOptions
{
    [Required] public Organization OrganizationDetails { get; set; } = null!;

    public void SetParent<TParentOrg>() where TParentOrg : OrganizationDefinition
    {
        OrganizationDetails.ParentId = Activator.CreateInstance<TParentOrg>().Name;
    }
}