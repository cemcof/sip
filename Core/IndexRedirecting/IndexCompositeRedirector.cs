using sip.Organizations;

namespace sip.Core.IndexRedirecting;

public class IndexCompositeRedirector(IServiceProvider sp) : IIndexRedirector
{
    public async ValueTask<IndexRedirectionResult?> DecideTargetAsync(ClaimsPrincipal claimsPrincipal, IOrganization? organization = null)
    {
        foreach (var redirectDelegate in sp.GetRequiredService<IEnumerable<IIndexRedirector>>()
                     .Where(r => r is not DummyIndexRedirector)
                     .Where(r => r is not IndexCompositeRedirector))
        {
            var result = await redirectDelegate.DecideTargetAsync(claimsPrincipal, organization);
            if (result is not null)
                return result;
        }

        return null;
    }
}