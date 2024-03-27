using System.Net;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using sip.Userman;

namespace sip.Auth;

public class LoginController(
        AppSignInManager         signInManager,
        AppUserManager           userManager,
        IOptions<AuthOptions>    authOptions,
        ILogger<LoginController> logger)
    : ControllerBase
{
    [HttpGet("/challenge/{scheme}")]
    public async Task<IActionResult> ChallengeExternalScheme(string scheme, string? returnUrl = "/")
    {
        if (returnUrl is null) returnUrl = "/";
        returnUrl = HttpUtility.UrlDecode(returnUrl);
        
        var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
        if (schemes.Any(s => s.Name == scheme))
        {
            // We need to wrap the return url, so that we always end up on SignExternalToInternal endpoint, which will 
            // sign in the application cookie using new info in external cookie
            // However, there is a special case - login mapping - if used, do not wrap to login_to_internal, since it is
            // not desirable for this use case - login mapper action will login the user after mapping and redirect him to the root page.
            if (!returnUrl.Contains("/loginmap"))
            {
                returnUrl = $"/login_to_internal?returnUrl={WebUtility.UrlEncode(returnUrl)}";
            }
            
            logger.LogDebug("Challenging scheme {}, RedirectUri is {}", scheme, returnUrl);
            var authProperties = signInManager.ConfigureExternalAuthenticationProperties(scheme, returnUrl);
            await HttpContext.ChallengeAsync(scheme, authProperties);
        }
        else
        {
            return BadRequest();
        }
        
        return new EmptyResult();
    }

    /// <summary>
    /// Attempts to map currently logged in external provider login info to the actual user in the system.
    ///
    /// User invoking this must have <see cref="AuthOptions.EXTERNALMAP_CLAIM_TYPE"/> that determines the target user and the target user must exist. 
    /// </summary>
    [HttpGet("/loginmap")]
    public async Task<IActionResult> MapLoginToUser(string returnUrl = "/")
    {
        returnUrl = HttpUtility.UrlDecode(returnUrl);
        
        // Check if we have claim to do this 
        var targetId = HttpContext.User.FindFirstValue(AuthOptions.EXTERNALMAP_CLAIM_TYPE);
        if (targetId is null)
        {
            logger.LogDebug("User mapping failed - {} claim is missing or is empty", 
                nameof(AuthOptions.EXTERNALMAP_CLAIM_TYPE));
            return BadRequest();
        }
        
        // Check if we have external provider authentication info
        var extinfo = await signInManager.GetExternalLoginInfoAsync();
        if (extinfo is null)
        {
            logger.LogDebug("User mapping failed - there is not external info in the cookie");
            return BadRequest();
        }
        
        // Check if target user exists
        var targetUser = await userManager.FindByIdAsync(targetId);
        if (targetUser is null)
        {
            logger.LogDebug("User mapping failed - target user {} does not exist", targetId);            
            return BadRequest();
        }
        
        
        // If this login already exists for a user, remove it so it can be mapped to the new user
        var existingUser = await userManager.FindByLoginAsync(extinfo.LoginProvider, extinfo.ProviderKey);
        if (existingUser is not null)
        {
            await userManager.RemoveLoginAsync(existingUser, extinfo.LoginProvider, extinfo.ProviderKey);
            logger.LogInformation("Removed login from user {}, {}", existingUser.Id, existingUser.Fullname);
        }
        
        // Save new login for the user 
        await userManager.AddLoginAsync(targetUser, extinfo);
        logger.LogInformation("Added login for user {}, {}", targetUser.Id, targetUser.Fullname);
        
        // Now sign in the new user 
        await signInManager.SignInAsync(targetUser, new AuthenticationProperties() { RedirectUri = returnUrl });
        
        return Redirect(returnUrl); // TODO - let redirection on signinmanager?
    }

    /// <summary>
    /// Just logouts the user out of identity cookies and sends him to logged out page.
    /// </summary>
    /// <returns></returns>
    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Redirect("/logged_out");
    }

    /// <summary>
    /// Attemps to log in user to the application.
    /// External login cookie must be provided in order to succeed.
    /// TODO - Add internal name/pwd loggin? Probably not. 
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet("/login_to_internal")]
    public async Task<IActionResult> SignExternalToInternal(string returnUrl = "/")
    {
        var origu = returnUrl;
        returnUrl = HttpUtility.UrlDecode(returnUrl);
        logger.LogDebug("Signing external to internal begun, return url is (decoded) {}, (original) {}", returnUrl, origu);
        
        var externalLoginInfo = await signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo is null)
        {
            logger.LogDebug("Cannot login the user to the app, since no external info is provided");
            return BadRequest();
        }

        var targetUser = await userManager.FindByLoginAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey);
        if (targetUser is null)
        {
            // The user with this login does not exist, should we create one?
            if (authOptions.Value.CreateUserOnLoginIfNotExists)
            {
                // Create default user, use some information from the external cookie, if provided
                var cp = externalLoginInfo.Principal;

                var user = AppUser.NewEmpty();
                user.PrimaryContact.Email = cp.GetEmail();
                user.PrimaryContact.Firstname = cp.GetFirstname();
                user.PrimaryContact.Lastname = cp.GetLastname();
                user.PrimaryContact.Phone = cp.GetPhone();
                await userManager.NewUserAsync(new NewUserModel() { UserDetails = user });
                // Also, we must right away map the external login to the new user
                await userManager.AddLoginAsync(user, externalLoginInfo);
                // await _userManagerOriginal.AddLoginAsync(user, externalLoginInfo);
                 
                targetUser = user;
            }
            else
            {
                logger.LogDebug("Cannot login the user to the app, since target user does not exist and implicit user creation is disabled");
                return BadRequest();
            }
        }
        
        // Now it is certain we have the user, so sign him in
        await signInManager.SignInAsync(targetUser, new AuthenticationProperties() {RedirectUri = returnUrl});
        
        return Redirect(returnUrl); // TODO - let redirection on signinmanager?
    }

    
}