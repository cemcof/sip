using sip.Utils;

namespace sip.Userman;

public class RoleDefinition : INamedSetup<RoleOptions>
{
    public virtual string Name => GetType().Name;
    
    public virtual void Setup(RoleOptions options)
    {
        options.RoleDetails.Id = Name;
        options.RoleDetails.Name = Name.Humanize();
    }

    public static string GetName<TRole>() where TRole : RoleDefinition => Activator.CreateInstance<TRole>().Name;
}


// Some role refs related to user management
public class UserAdminRole : RoleDefinition
{
    public override void Setup(RoleOptions options)
    {
        base.Setup(options);
        options.RoleDetails.Description = "User with this role can manage other users within the system";
    }
}

public class ImpersonatorRole : RoleDefinition
{
    public override void Setup(RoleOptions options)
    {
        base.Setup(options);
        options.RoleDetails.Description =
            "User with this role can impersonate as other users within the system or use arbitrary claims. " +
            "This is essentially a god mode role and can be granted only directly in the database.";
    }
}
