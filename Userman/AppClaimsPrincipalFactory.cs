namespace sip.Userman;

public class AppClaimsPrincipalFactory(
        AppUserManager                  appUserManager,
        IOptions<ClaimsIdentityOptions> claimsIdentityOptions)
    : IUserClaimsPrincipalFactory<AppUser>
{
    public async Task<ClaimsPrincipal> CreateAsync(AppUser user)
    {
        // Basic info from user should be transformed to the claims
        var claimsToAdd = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Id.ToString()),
            new(claimsIdentityOptions.Value.SecurityStampClaimType, user.SecurityStamp ?? string.Empty)
        };
        
        // Extract claims from user contacts
        foreach (var userContact in user.Contacts)
        {
            if (!string.IsNullOrEmpty(userContact.Firstname)) claimsToAdd.Add(new(ClaimTypes.GivenName, userContact.Firstname));
            if (!string.IsNullOrEmpty(userContact.Lastname)) claimsToAdd.Add(new(ClaimTypes.Surname, userContact.Lastname));
            if (!string.IsNullOrEmpty(userContact.Email)) claimsToAdd.Add(new(ClaimTypes.Email, userContact.Email));
            if (!string.IsNullOrEmpty(userContact.Phone)) claimsToAdd.Add(new(ClaimTypes.MobilePhone, userContact.Phone));
        }
        
        
        
        // Now load roles 
        var roles = await appUserManager.GetRolesForUserAsync(user);
        claimsToAdd.AddRange(
            roles.Select(r => new Claim(ClaimTypes.Role, r.organization is null ? r.role.Id : $"{r.role.Id}/{r.organization.Id}"))
        );
        
        // Load user claims 
        var userClaims = await appUserManager.GetClaimsAsync(user);
        claimsToAdd.AddRange(userClaims.Select(c => new Claim(c.Type, c.Value)));
        
        var claimsIdentity =
            new ClaimsIdentity(claimsToAdd, IdentityConstants.ApplicationScheme); // TODO - consider using custom scheme for app auth

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        return claimsPrincipal;
    }
    
    public async Task<ClaimsPrincipal?> GenerateClaimsForUserId(string id, IEnumerable<Claim>? additionalClaims = null)
    {
        additionalClaims ??= Enumerable.Empty<Claim>();
        var user = await appUserManager.FindByIdAsync(id);
        if (user is null) return null;
        
        var cp = await CreateAsync(user);
        var ci = cp.Identities.First();
        ci.AddClaims(additionalClaims);

        return cp;
    }
}