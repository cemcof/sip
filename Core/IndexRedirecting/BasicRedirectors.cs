namespace sip.Core.IndexRedirecting;

public class ToLoginIfNoUserIdentityRedirector : IIndexRedirector
{
    public ValueTask<IndexRedirectionResult?> DecideTargetAsync(ClaimsPrincipal claimsPrincipal, IOrganization? organization = null)
    {
        if (!claimsPrincipal.IsAppAuthenticated())
        {
            // If not, send him to login 
            return ValueTask.FromResult<IndexRedirectionResult?>(new("/login", true));
        }

        return ValueTask.FromResult<IndexRedirectionResult?>(null);
    }
}

public class ToAvailableOrganizationIfNoneRedirector(IOrganizationProvider organizationProvider) : IIndexRedirector
{
    public ValueTask<IndexRedirectionResult?> DecideTargetAsync(ClaimsPrincipal claimsPrincipal, IOrganization? organization = null)
    {
        if (organization is null)
        {
            // Try to find organization in roles
            var targetOrg = claimsPrincipal.GetOrganizationScopedRoles().FirstOrDefault();
            
            // If there is any, redirect to that organization
            if (targetOrg != default)
            {
                var org = organizationProvider.GetFromString(targetOrg.org);
                var result = new IndexRedirectionResult($"/{org.LinkId}", false);
                return ValueTask.FromResult<IndexRedirectionResult?>(result);
            }
        }

        return ValueTask.FromResult<IndexRedirectionResult?>(null);
    }
}