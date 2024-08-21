using System.Collections;
using Microsoft.AspNetCore.Authorization;

// ReSharper disable CollectionNeverUpdated.Global

namespace sip.Auth;

public class RoleNetworkAuthOptions
{
    /// <summary>
    /// If true, both roles and addresses are required to match
    /// </summary>
    public bool RequireBoth { get; set; }
    /// <summary>
    /// Instead of OR, perform AND operation over the list of roles. 
    /// </summary>
    public bool ReqireAllRoles { get; set; }
        
    public ICollection<string> Roles { get; set; } = new List<string>();
    public ICollection<IPAddr> RemoteAddresses { get; set; } = new List<IPAddr>();
}

public class NotRemoteIpRequirement(IPAddr disallowedIpOrNetworkAddress) : AuthorizationHandler<NotRemoteIpRequirement>,
                                                                           IAuthorizationRequirement
{
    public IPAddr DisallowedIpOrNetworkAddress { get; } = disallowedIpOrNetworkAddress;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NotRemoteIpRequirement requirement)
    {
        var ip = context.User.GetRemoteIp();
        if (ip is not null && !ip.CheckAgainst(DisallowedIpOrNetworkAddress))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        context.Fail();
        return Task.CompletedTask;
    }
}

public record RoleNetworkAuthRequirement
    (RoleNetworkAuthOptions Options, IOrganization? Organization = null) : IAuthorizationRequirement;
    
public class RoleNetworkAuthorizationHandler(ILogger<RoleNetworkAuthorizationHandler> logger) : AuthorizationHandler<RoleNetworkAuthRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   RoleNetworkAuthRequirement requirement)
    {
        var opts = requirement.Options;
        
        // Lets resolve roles
        var anyRoles = opts.Roles.Count > 0;
        var rolesSatisfied = false;
        
        // Do we required scoped role?
        if (anyRoles)
        {
            bool RolePredicate(string roleStr)
                => requirement.Organization is null
                    ? context.User.IsInRole(roleStr)
                    : context.User.IsInRoleOfScope(roleStr, requirement.Organization.Id);
            
            rolesSatisfied = opts.ReqireAllRoles ? 
                opts.Roles.All(RolePredicate) :
                opts.Roles.Any(RolePredicate);  
        }
            
        // Lets resolve network IP
        var allowedRemoteIps = opts.RemoteAddresses.ToArray();
        var anyIps = allowedRemoteIps.Any();
        var ipSatisfied = false;
        var userRemoteIp = context.User.GetRemoteIp();
        if (anyIps)
        {
            ipSatisfied = userRemoteIp is not null && userRemoteIp.CheckAgainst(allowedRemoteIps);
        }

        if (anyIps || anyRoles)
        {
            if ((anyIps && ipSatisfied) || (anyRoles && rolesSatisfied))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
