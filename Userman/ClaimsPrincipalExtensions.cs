using sip.Auth;
using sip.Utils;

namespace sip.Userman;

public static class ClaimsPrincipalExtensions
{
    public static bool IsAuthenticated(this ClaimsPrincipal cp) =>
        cp.Identities.Any(i => i.IsAuthenticated);

    public static bool IsAppAuthenticated(this ClaimsPrincipal cp) =>
        cp.Identities.Any(i => i.AuthenticationType == IdentityConstants.ApplicationScheme);

    public static string? GetPhone(this ClaimsPrincipal cp) =>
        cp.FindFirstValue(ClaimTypes.MobilePhone);

    public static string? GetEmail(this ClaimsPrincipal cp) =>
        cp.FindFirstValue(ClaimTypes.Email);
    
    public static string? GetId(this ClaimsPrincipal cp) =>
        cp.FindFirstValue(ClaimTypes.NameIdentifier);
    
    public static string? GetFullcontact(this ClaimsPrincipal cp) =>
        cp.GetFullname() + $" <{cp.GetEmail()}>";

    public static IEnumerable<(string role, string org)> GetOrganizationScopedRoles(this ClaimsPrincipal cp)
    {
        foreach (var claim in cp.Claims.Where(c => c.Value.Contains("/")).Select(c => c.Value.Split('/')))
        {
            yield return (claim[0], claim[1]);
        }
    }
    
    public static bool IsInRoleOfScope(this ClaimsPrincipal cp, string role, string? scope)
    {
        var targetRole = (string.IsNullOrEmpty(scope)) ? role : role + "/" + scope;
        return cp.IsInRole(targetRole);
    }


    public static IUserInfo ToUserInfo(this ClaimsPrincipal cp)
    {
        var idStr = cp.GetId();
        if (string.IsNullOrWhiteSpace(idStr))
        {
            throw new UserNotAuthenticatedException();
        }

        if (!Guid.TryParse(idStr, out var id))
        {
            throw new InvalidOperationException("User ID does not have GUID format");
        }
            
        return new UserInfo(
            id,
            cp.GetEmail(),
            cp.GetFirstname(),
            cp.GetLastname()
        );
    }

    public static IUserClientInfo ToUserClientInfo(this ClaimsPrincipal cp)
    {
        var uinfo = cp.ToUserInfo();
        return new ClientInfo(Id: uinfo.Id, EmailAddress: uinfo.EmailAddress, Firstname: uinfo.Firstname, Lastname: uinfo.Lastname,
            IpAddress: cp.GetRemoteIp() ??
            throw new NotAvailableException("IpAddress of current claims principal is not available"));
    }
}