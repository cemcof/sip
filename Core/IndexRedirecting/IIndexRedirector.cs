namespace sip.Core.IndexRedirecting;

public interface IIndexRedirector
{
    ValueTask<IndexRedirectionResult?> DecideTargetAsync(
        ClaimsPrincipal claimsPrincipal, 
        IOrganization? organization = null);
}