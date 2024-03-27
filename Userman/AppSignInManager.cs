using Microsoft.AspNetCore.Authentication;

namespace sip.Userman;

public class AppSignInManager(
        UserManager<AppUser>                 userManager,
        IHttpContextAccessor                 contextAccessor,
        IUserClaimsPrincipalFactory<AppUser> claimsFactory,
        IOptions<IdentityOptions>            optionsAccessor,
        ILogger<SignInManager<AppUser>>      logger,
        IAuthenticationSchemeProvider        schemes,
        IUserConfirmation<AppUser>           confirmation)
    : SignInManager<AppUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation);