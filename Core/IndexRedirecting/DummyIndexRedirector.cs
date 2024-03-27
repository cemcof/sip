using sip.Organizations;

namespace sip.Core.IndexRedirecting;

public class DummyIndexRedirector : IIndexRedirector
{
    public ValueTask<IndexRedirectionResult?> DecideTargetAsync(ClaimsPrincipal claimsPrincipal,
        IOrganization? organization = null)
        => ValueTask.FromResult<IndexRedirectionResult?>(null);
}