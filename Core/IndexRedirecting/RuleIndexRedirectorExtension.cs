namespace sip.Core.IndexRedirecting;

public class RuleIndexRedirectionBuilder(IServiceCollection services)
{
    public RuleIndexRedirectionBuilder ToLoginIfNoUserIdentity()
    {
        services.AddSingleton<IIndexRedirector, ToLoginIfNoUserIdentityRedirector>();
        return this;
    }

    public RuleIndexRedirectionBuilder ToAvailableOrganizationIfNone()
    {
        services.AddSingleton<IIndexRedirector, ToAvailableOrganizationIfNoneRedirector>();
        return this;
    }

    public void Done()
    {
        services.AddSingleton<IIndexRedirector, IndexCompositeRedirector>();
    }
}

public static class RuleIndexRedirectorExtension
{
    public static RuleIndexRedirectionBuilder UseRuleIndexRedirection(this IServiceCollection services)
    {
        return new RuleIndexRedirectionBuilder(services);
    }
}