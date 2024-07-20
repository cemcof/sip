using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using sip.Auth;
using sip.Organizations;

namespace sip.Core;

public class MainGuiOptions
{
    public string? BrandText { get; set; }
    public string? PanelTheme { get; set; } = "default";
    public PathString? LogoPath { get; set; }

    public Type? DefaultLayoutComponent { get; private set; } = typeof(MainLayout);
    public MainGuiOptions SetDefaultLayout<TLayout>() where TLayout : LayoutComponentBase
    {
        DefaultLayoutComponent = typeof(TLayout);
        return this;
    }
    
    public MainGuiOptions NoDefaultLayout()
    {
        DefaultLayoutComponent = null;
        return this;
    }

    public Type UnauthorizedComponent { get; private set; } = typeof(Unauthorized);
    public MainGuiOptions SetAnauthorizedComponent<TComponent>() where TComponent : ComponentBase
    {
        UnauthorizedComponent = typeof(TComponent);
        return this;
    }

    public Type NotFoundComponent { get; private set; } = typeof(NotFound);
    public MainGuiOptions SetNotFoundComponent<TComponent>() where TComponent : ComponentBase
    {
        NotFoundComponent = typeof(TComponent);
        return this;
    }


    public Dictionary<string, PanelItemOptions> Modules { get; set; } = new();
    
    /// <summary>
    /// It will not be possible to login when accessing from these networks / IPs
    /// Useful for internal computers where login is not desired.
    /// </summary>
    public string[] NoSignInNetworks { get; set; } = [];

}

public class PanelItemOptions
{
    public string? DisplayText { get; set; } = string.Empty;
    public string LinkHref { get; set; } = string.Empty;

    public RoleNetworkAuthOptions? RoleNetworkAuthorization { get; set; }
    
    public UserRoleAuthOptions? UserRoleAuthorization { get; set; }
    public string? CssIcon { get; set; }

    public bool RequiresActiveOrganization { get; set; }
    public bool Render => !string.IsNullOrEmpty(DisplayText) || !string.IsNullOrEmpty(CssIcon);

    public IEnumerable<IAuthorizationRequirement> GetAuthorizationRequirements(IOrganization? org = null)
    {
        if (UserRoleAuthorization is not null)
        {
            yield return new AppUserRolesRequirement(UserRoleAuthorization.Roles, org);
        }

        if (RoleNetworkAuthorization is not null)
        {
            yield return new RoleNetworkAuthRequirement(RoleNetworkAuthorization, org);
        }
    }
}