using Microsoft.Extensions.Primitives;

namespace sip.Auth;

/// <summary>
/// Constants related to network authentication.
/// </summary>
public static class NetworkAddressAuth 
{
    public const string LOCAL_IP_CLAIMTYPE = "LOCAL_IP";
    public const string REMOTE_IP_CLAIMTYPE = "REMOTE_IP";
    public const string LOCAL_PORT_CLAIMTYPE = "LOCAL_PORT";
    public const string REMOTE_PORT_CLAIMTYPE = "REMOTE_PORT";
    public const string REMOTE_IP_FORWARDED_FOR_CLAIMTYPE = "REMOTE_IP_FORWARDED_FOR";
    public const string NETWORK_ADDRESS_AUTHENTICATION = "NETWORK_ADDRESS_AUTHENTICATION";
}

/// <summary>
/// This middleware adds a new <see cref="ClaimsIdentity"/> to the <see cref="ClaimsPrincipal"/>
/// New identity contains claims about network connection, such as remote ip address, local ip address and ports. 
/// The <see cref="ClaimTypes"/> can be found as constants in <see cref="NetworkAddressAuth"/> 
/// </summary>
public class NetworkAddressAuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
            
        var networkIdentity = new ClaimsIdentity(NetworkAddressAuth.NETWORK_ADDRESS_AUTHENTICATION);
        
        // IPs
        if (context.Connection.LocalIpAddress is not null)
            networkIdentity.AddClaim(new Claim(NetworkAddressAuth.LOCAL_IP_CLAIMTYPE, context.Connection.LocalIpAddress.ToString()));

        if (context.Connection.RemoteIpAddress is not null)
            networkIdentity.AddClaim(new Claim(NetworkAddressAuth.REMOTE_IP_CLAIMTYPE, context.Connection.RemoteIpAddress.ToString()));
        
        // From X-Forwarded-For header
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var forwardedForStr = forwardedFor.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedForStr))
                networkIdentity.AddClaim(new Claim(NetworkAddressAuth.REMOTE_IP_FORWARDED_FOR_CLAIMTYPE, forwardedForStr));
        }
            
        // Ports
        networkIdentity.AddClaim(new Claim(NetworkAddressAuth.LOCAL_PORT_CLAIMTYPE, context.Connection.LocalPort.ToString()));
        networkIdentity.AddClaim(new Claim(NetworkAddressAuth.REMOTE_PORT_CLAIMTYPE, context.Connection.RemotePort.ToString()));
        
        if (!context.User.Identities.Any(i => i.AuthenticationType == NetworkAddressAuth.NETWORK_ADDRESS_AUTHENTICATION))
        {
            context.User.AddIdentity(networkIdentity);
        }

        // Call the next delegate/middleware in the pipeline
        await next(context);
    }
}