using Microsoft.AspNetCore.Authorization;

namespace sip.Auth.Verification.Contact;

public class ContactVerifiedRequirement : IAuthorizationRequirement;

public class ContactVerifierService(AppUserManager userManager) : AuthorizationHandler<ContactVerifiedRequirement>, IVerificator
{
    public const string VERIFIED_CLAIM_NAME = "VERIFIED";
    public const string VERIFIED_CLAIM_VALUE = "CONTACT";
    
    public Task<bool> IsVerifiedAsync(ClaimsPrincipal user)
    {
        var verf = user.HasClaim(VERIFIED_CLAIM_NAME, VERIFIED_CLAIM_VALUE);
        return Task.FromResult(verf);
    }

    public Type GetVerificationComponent() => typeof(ContactVerifier);

    public async Task SetVerifiedAsync(ClaimsPrincipal user)
    {
        var appUser = await userManager.FindByCpAsync(user);
        if (appUser is null)
            throw new InvalidOperationException("Trying to verify unknown user.");
        
        await userManager.AddClaimsAsync(appUser, [new Claim(VERIFIED_CLAIM_NAME, VERIFIED_CLAIM_VALUE)]);
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ContactVerifiedRequirement requirement)
    {
        if (!context.User.IsAppAuthenticated())
        {
            // There is no user logged in, so no need for verification 
            context.Succeed(requirement);
            return;
        }
        
        if (await IsVerifiedAsync(context.User))
            context.Succeed(requirement);
        else   
            context.Fail();
    }
}