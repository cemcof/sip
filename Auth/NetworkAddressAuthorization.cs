using System.Collections;
using Microsoft.AspNetCore.Authorization;
using sip.Organizations;

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
    public ICollection<string> RemoteAddresses { get; set; } = new List<string>();
}

public class NotRemoteIpRequirement(string disallowedIpOrNetworkAddress) : AuthorizationHandler<NotRemoteIpRequirement>,
                                                                           IAuthorizationRequirement
{
    public string DisallowedIpOrNetworkAddress { get; } = disallowedIpOrNetworkAddress;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NotRemoteIpRequirement requirement)
    {
        var ip = context.User.GetRemoteIp();
        if (!string.IsNullOrWhiteSpace(ip) && !ip.CheckIp(DisallowedIpOrNetworkAddress))
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
    private readonly ILogger<RoleNetworkAuthorizationHandler> _logger = logger;

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
        var userRemoteIp = context.User.FindFirst(c => c.Type == NetworkAddressAuth.REMOTE_IP_CLAIMTYPE)?.Value;
        if (anyIps)
        {
            ipSatisfied = userRemoteIp is not null && userRemoteIp.CheckIp(allowedRemoteIps);
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

/// <summary>
/// Static helpers for working with IP addresses.
/// </summary>
public static class IpHelper
{
    public static bool CheckIp(this string sourceIp, params string[] targetIp)
    {
        return targetIp.Any(sourceIp.CheckIp);
    }

    private static bool CheckIp(this string sourceIp, string targetIp)
    {
        if (targetIp.Contains('/'))
        {
            // This means the sourceIp is specifiead as network address with prefix
            var ip = targetIp.Split('/')[0];
            var prefix = int.Parse(targetIp.Split('/')[1]);
            
            var sourceIpBits = new BitArray(IPAddress.Parse(sourceIp).GetAddressBytes().Reverse().ToArray());
            var targetIpBits = new BitArray(IPAddress.Parse(ip).GetAddressBytes().Reverse().ToArray());
            if (sourceIpBits.Length < prefix || sourceIpBits.Length != targetIpBits.Length)
            {
                return false;
            }

            // Obviously, bit shifts would be more elegant than this nested for garbage, but this solution
            // is universal enough to even work for IPv6 addresses
            for (int i = sourceIpBits.Length - 1; i >= sourceIpBits.Length - prefix; i--)
            {
                if (sourceIpBits[i] != targetIpBits[i])
                {
                    return false;
                }
            }

            return true;
        }
        
        // Just compare addresses
        return sourceIp.Equals(targetIp);
    }
}