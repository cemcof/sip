using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sip.Auth;

public class ImpersonationOptions
{
    /// <summary>
    /// If false, impersonation is forbidden and not supported by the app. 
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Whether the impersonator needs to have a role allowing him to impersonate.
    ///
    /// If false, anybody can impersonate! Only false for development purposes. 
    /// </summary>
    public bool Protected { get; set; } = true;

    /// <summary>
    /// User must be in this role in order to perform impersonation.
    /// This does not apply when <see cref="Protected"/> is <value>false</value>
    /// </summary>
    public string ImpersonatorRoleName { get; set; } = RoleDefinition.GetName<ImpersonatorRole>();
    
    /// <summary>
    /// This claim type, if present, denotes that <see cref="ClaimsPrincipal"/> is impersonated.
    /// Value of the claim, if available, identifies impersonating user by id. 
    /// </summary>
    public string ImpersonatedClaimName { get; set; } = "impersonated";

    /// <summary>
    /// Sign in scheme impersonation will happen against. Defaults to <see cref="IdentityConstants.ApplicationScheme"/>
    /// </summary>
    public string SignInScheme { get; set; } = IdentityConstants.ApplicationScheme;

    /// <summary>
    /// Target path to which redirect after impersonation/deimpersonation. Defaults to "/" (root url)
    /// </summary>
    public PathString RedirectTarget { get; set; } = "/";

    // ReSharper disable once InconsistentNaming
    public const string ImpersonatorPolicy = "Impersonator";
    // ReSharper disable once InconsistentNaming
    public const string ImpersonatedPolicy = "Impersonated";
}

[Route("/impersonation")]
public class ImpersonationController(
        ILogger<ImpersonationController>      logger,
        IOptionsMonitor<ImpersonationOptions> options,
        AppUserManager                        userManager,
        IUserClaimsPrincipalFactory<AppUser>  claimsPrincipalFactory,
        IAuthorizationService                 authorizationService)
    : ControllerBase
{
    [HttpGet("impersonate")]
    public async Task<IActionResult> Impersonate(string? claims, Guid? userId)
    {
        var opts = options.CurrentValue;
        logger.LogDebug("Running impersonation endpoint, Enabled={}, Protected={}, the claims are: \n{}", 
            opts.Enabled, opts.Protected, claims);
        
        if (!opts.Enabled) return BadRequest();
        
        // The user might be already impersonated, in that case, deimpersonate him first
        if (HttpContext.User.HasClaim(c => c.Type == opts.ImpersonatedClaimName))
        {
            await Deimpersonate();
        }
        
        // Authorize the request, if necessary.
        var authorizationResult =  await authorizationService.AuthorizeAsync(HttpContext.User, ImpersonationOptions.ImpersonatorPolicy);
        if (opts.Protected && !authorizationResult.Succeeded)
        {
            logger.LogDebug("Forbidding impersonation");
            return Forbid();
        }
        
        // Prepare new claims
        var cls = new List<Claim>();
        
        // If user is targeted, gather his claims and include them to impersonation.
        if (userId is not null)
        {
            // If we are targeting a user, get the claims
            var user = await userManager.FindByIdAsync(userId);
            if (user is not null)
            {
                var claimsPrincipal = await claimsPrincipalFactory.CreateAsync(user);
                cls.AddRange(claimsPrincipal.Claims);
            }
            else
            {
                return NotFound($"Impersonation target user {userId} not found");
            }
        }
        
        // If arbitrary claims were specified, include them to impersonation.
        // Claims are each on separate row, key value separator is = 
        if (!string.IsNullOrWhiteSpace(claims))
        {
            foreach (var row in claims.Split("\n"))
            {
                if (string.IsNullOrWhiteSpace(row))
                    continue;

                var rspl = row.Split("=");
                var type = rspl[0].Trim();
                var value = (rspl.Length < 2) ? type : rspl[1];
                cls.Add(new Claim(type, value));
            }
        }
        
        // Include information that the principal is impersonated and if available who initiated the impersonation,
        // so we can effectively deimpersonate back
        var impersonatorId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        cls.Add(new Claim(opts.ImpersonatedClaimName, impersonatorId ?? ""));
        
        // Prepare impersonated principal and sign him in
        // Consider using different scheme.
        var cp = new ClaimsPrincipal(new ClaimsIdentity(cls, opts.SignInScheme));
        logger.LogDebug("Prepared CP for impersonation (sign in), scheme {}", opts.SignInScheme);
        await HttpContext.SignInAsync(opts.SignInScheme, cp, new AuthenticationProperties()
        {
            IsPersistent = true,
            AllowRefresh = true
        });
        
        logger.LogInformation("User impersonated, redirecting to: {}, claims are:\n{}",
            opts.RedirectTarget,
            string.Join("\n", cls.Select(c => $"{c.Type}={c.Value}")));

        return Redirect(opts.RedirectTarget);
    }
    
    [HttpGet("deimpersonate")]
    public async Task<IActionResult> Deimpersonate()
    {
        var opts = options.CurrentValue;
        logger.LogDebug("Invoked user deimpersonation endpoint");

        if (!opts.Enabled) return BadRequest();

        if (!HttpContext.User.HasClaim(c => c.Type == opts.ImpersonatedClaimName))
        {
            // User is not impersonated. Noop. 
            return Redirect(opts.RedirectTarget);
        }

        // Remember impersonated claim value and sign impersonated principal out
        var impersonatorId = HttpContext.User.FindFirstValue(opts.ImpersonatedClaimName);
        await HttpContext.SignOutAsync(opts.SignInScheme);
        
        // If we have information of who initiated the impersonation, sign back that user.
        if (!string.IsNullOrWhiteSpace(impersonatorId))
        {
            var user = await userManager.FindByIdAsync(impersonatorId);
            if (user is not null)
            {
                await HttpContext.SignInAsync(opts.SignInScheme,
                    await claimsPrincipalFactory.CreateAsync(user));
            }
        }

        logger.LogDebug("User deimpersonated successfully");
        return Redirect(opts.RedirectTarget);
    }
}

public static class ImpersonationExtensions
{
    /// <summary>
    /// Enables impersonation with default configuration.
    /// On development environment, impersonation is not protected, allowing any anonymous user to impersonate.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="webHostEnvironment"></param>
    public static AuthenticationBuilder UseImpersonation(this AuthenticationBuilder builder, IWebHostEnvironment webHostEnvironment)
    {
        //builder.Services.AddOptions<RoleOptions>()
        builder.Services.Configure<ImpersonationOptions>(o =>
        {
            o.Enabled = true;
            o.Protected = !webHostEnvironment.IsDevelopment();
        });

        builder.Services.AddOptions<AuthorizationOptions>()
            .Configure<IOptions<ImpersonationOptions>>((o, impopts) =>
            {
                o.AddPolicy(ImpersonationOptions.ImpersonatorPolicy,
                    p => p.RequireAssertion(kek => kek.User.IsInRoleAnyScope(impopts.Value.ImpersonatorRoleName)));
                o.AddPolicy(ImpersonationOptions.ImpersonatedPolicy, 
                        p => p.RequireAssertion(kek => kek.User.HasClaim(c=> c.Type == impopts.Value.ImpersonatedClaimName)));
            });

        return builder;
    }
}
