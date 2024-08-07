namespace sip.Organizations;

public class OrganizationDefinition : INamedSetup<OrganizationOptions>
{
    public virtual string Name => GetType().Name;

    public virtual void Setup(OrganizationOptions opts)
    {
        
    }

    public static string GetName<TOrganiztion>() where TOrganiztion : OrganizationDefinition =>
        Activator.CreateInstance<TOrganiztion>().Name;
}