using Microsoft.AspNetCore.Authorization;

namespace sip.Auth;

public class NotIpBlacklistedRequirement(
    string blacklistConfSectionName,
    IOrganization? organization = null
    ) :  IAuthorizationRequirement
{
    public string BlacklistConfSectionName { get; } = blacklistConfSectionName;
    public IOrganization? Organization { get; } = organization;
}

public class NotIpBlacklistedHandler(IConfiguration configuration) : AuthorizationHandler<NotIpBlacklistedRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NotIpBlacklistedRequirement requirement)
    {
        var confSection =  (requirement.Organization is not null) ?
            configuration.GetSection($"{requirement.Organization.Id}:{requirement.BlacklistConfSectionName}") :
            configuration.GetSection(requirement.BlacklistConfSectionName);
        
        var ip = context.User.GetRemoteIp();
        if (ip is null)
        {
            context.Fail(new AuthorizationFailureReason(this, "No identity with remote IP found"));
            return Task.CompletedTask;
        }
        
        var blackListed = new List<IPAddr>();
        confSection.Bind(blackListed);
        var blacklisted = ip.CheckAgainst(blackListed.ToArray());
        if (blacklisted)
        {
            context.Fail(new AuthorizationFailureReason(this, "IP is blacklisted"));
            return Task.CompletedTask;
        }
        
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}