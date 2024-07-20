using Microsoft.AspNetCore.Authentication.Cookies;
using sip.Auth;
using sip.Auth.Verification;
using sip.Auth.Verification.Contact;
using sip.Core;
using sip.Messaging;
using sip.Organizations;
using sip.Utils;

namespace sip.Userman;

public static class UsermanExtensions
{
    public static (string firstname, string surname) GetUsername(this ClaimsPrincipal claimsPrincipal)
    {
        var firstName = claimsPrincipal.GetFirstname() ?? "";
        var surname = claimsPrincipal.GetLastname() ?? "";
        return (firstName, surname);
    }

    public static string? GetFullname(this ClaimsPrincipal cp, bool surnameFirst = false)
    {
        var name = cp.GetUsername();
        if (string.IsNullOrWhiteSpace(name.firstname) || string.IsNullOrWhiteSpace(name.surname))
        {
            return null;
        }

        return surnameFirst ? name.surname + " " + name.firstname : name.firstname + " " + name.surname;
    }

    public static bool HasName(this ClaimsPrincipal cp) =>
        cp.GetFullname() is not null;

    /// <summary>
    /// Finds remote ip claim in the user, if available, returns ip as string.
    /// </summary>
    /// <param name="cp"></param>
    /// <returns></returns>
    public static string? GetRemoteIp(this ClaimsPrincipal cp) =>
        cp.FindFirstValue(NetworkAddressAuth.REMOTE_IP_CLAIMTYPE);

    public static string? GetFirstname(this ClaimsPrincipal cp)
    {
        var fname = cp.FindFirstValue(ClaimTypes.GivenName);
        if (!string.IsNullOrWhiteSpace(fname)) return fname;
        fname = cp.FindFirstValue(ClaimTypes.Name);
        return fname.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
    }

    
    public static string? GetLastname(this ClaimsPrincipal cp)
    {
        var sname = cp.FindFirstValue(ClaimTypes.Surname);
        if (!string.IsNullOrWhiteSpace(sname)) return sname;
        sname = cp.FindFirstValue(ClaimTypes.Name);
        return sname.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
    }

    

    public static bool IsInRoleAnyScope(this ClaimsPrincipal cp, string role)
    {
        return cp.Claims.Any(c => c.Type == ClaimTypes.Role && 
                                  (c.Value == role || c.Value.StartsWith(role + "/")));
    }
    
    /// <summary>
    /// This method checks whether user security stamp got changed, if so, refuses the user, which will
    /// force user to reauthenticate.
    /// If this check is activated via <see cref="AuthOptions"/>, a roundtrip to the user store (database) has to
    /// be made for each request requiring authorization.
    /// </summary>
    /// <param name="ctx"></param>
    internal static async Task ValidatePrincipalHandler(CookieValidatePrincipalContext ctx)
    {
        var aopts = ctx.HttpContext.RequestServices.GetRequiredService<IOptions<AuthOptions>>().Value;
        if (!aopts.ValidateSecurityStamp) return;
        
        var copts = ctx.HttpContext.RequestServices.GetRequiredService<IOptions<ClaimsIdentityOptions>>().Value;
        var userManager = ctx.HttpContext.RequestServices.GetRequiredService<AppUserManager>();
        
        if (ctx.Principal is null) throw new InvalidOperationException("Missing user");
        
        var secStampClaimName = copts.SecurityStampClaimType;
        var securityStamp = ctx.Principal.FindFirstValue(secStampClaimName);
        // We did not find any security stamp in claims, this is ok, but maybe make this configurable
        if (string.IsNullOrWhiteSpace(securityStamp))
            return;

        var userId = ctx.Principal.GetId() ?? throw new InvalidOperationException("Missing user identification");
        // We have the security stamp, compare it with currently trusted one
        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            var stamp = await userManager.GetSecurityStampAsync(user);
            if (stamp == securityStamp)
            {
                // Success, do nothing! 
                return;
            }
        }
        
        ctx.RejectPrincipal();
    }
}

public class UsermanBuilder(IServiceCollection services, ILogger<UsermanBuilder> logger)
{
    /// <summary>
    /// Adds all services related to users, roles...
    /// Uses aspnetcore identity without roles as a base. 
    /// </summary>
    /// <returns></returns>
    public UsermanBuilder AddUserManagement()
    {
        // General messaging is required for user management
        new MessagingBuilder(services)
            .AddGeneralMessaging();
        
        services.AddIdentityCore<AppUser>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>();
        services.AddScoped<AppSignInManager>();
        services.AddSingleton<AppClaimsPrincipalFactory>();
        services.AddSingleton<AppUserManager>();
        services.AddSingleton<IAppUserProvider>(s => s.GetRequiredService<AppUserManager>());
        services.AddSingleton<IFilterItemsProvider<AppUser>>(s => s.GetRequiredService<AppUserManager>());

        services.AddAuthentication(opts =>
            {
                opts.DefaultScheme = IdentityConstants.ApplicationScheme;
                opts.RequireAuthenticatedSignIn = false; // TODO if this is true, saml2 to external sign in is blocked due 
                // to the fact that saml2 does not set authentication type. Resolve how?
            })
            .AddContactVerification()
            .AddIdentityCookies(o => o.ApplicationCookie.Configure(ac =>
            {
                // Configure application cookie = reject the user in case security stamp was changed
                ac.Events.OnValidatePrincipal = UsermanExtensions.ValidatePrincipalHandler;
            }));
        
        // Now setup the db
        void Mb(ModelBuilder builder)
        {
            builder.Entity<Organization>(); // We need organizations in order to have scoped roles.
            builder.Entity<Contact>();
            
            new AppUserEntityTypeConfiguration().Configure(builder.Entity<AppUser>());
            new AppRoleEntityTypeConfiguration().Configure(builder.Entity<AppRole>());
            new UserInRoleEntityTypeConfiguration().Configure(builder.Entity<UserInRole>());
        }
        
        services.AddSingleton((ModelBuilderDelegate) Mb);
        ScanForRoleRefs(Assembly.GetExecutingAssembly());
        
        // Role seeding (from named options)
        services.AddOptions<DbSeedOptions>()
            .Configure<IOptionListProvider<RoleOptions>>((seed, orgopts) =>
            {
                var appRoles = orgopts.GetAll().Select(x => x.RoleDetails);
                seed.SeedEntities.AddRange(appRoles);
            });

        return this;
    }

    public UsermanBuilder AddRole<TRoleRef>() where TRoleRef : RoleDefinition
    {
        services.ConfigureFromNamedSetup<RoleOptions, TRoleRef>();
        return this;
    }

    public UsermanBuilder ScanForRoleRefs(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            ScanForRoleRefs(assembly);
        }

        return this;
    }

    public UsermanBuilder ScanForRoleRefs(Assembly assembly)
    {
        logger.LogDebug("Scanning for role refs in assembly {}", assembly.FullName);
        var adder = GetType().GetMethod(nameof(AddRole))!;
        
        var refs = assembly.FindImplementations<RoleDefinition>(includeSelf: false);
        
        foreach (var type in refs)
        {
            logger.LogDebug("Discovered role ref: {}", type.Name);
            var genadder = adder.MakeGenericMethod(type);
            genadder.Invoke(this, Array.Empty<object?>());
        }

        return this;
    }
}