using System.Linq.Expressions;
using System.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using sip.Core;

namespace sip.Userman;

public class AppUserManager(
        IServiceScopeFactory            serviceScopeFactory,
        ILogger<AppUserManager>         logger,
        JwtTokenProvider                jwtTokenProvider,
        IDbContextFactory<AppDbContext> dbContextFactory,
        IEntityMerger<AppDbContext>     entityMerger,
        GeneralMessageBuilderProvider   messageBuilderProvider,
        IOptions<AppOptions>            appOptions)
    : IFilterItemsProvider<AppUser>, IAppUserProvider
{
    public string GenerateAccountmapRelativeUrl(AppUser forUser)
    {
        var accMapClaim = new Claim(AuthOptions.EXTERNALMAP_CLAIM_TYPE, forUser.Id.ToString());
        var token = jwtTokenProvider.CreateToken(new[] {accMapClaim}, forUser.EmailAddress!, "EXTERNAL_MAP");

        var targetMapUrlEncoded = HttpUtility.UrlEncode( $"/loginmap?token={token}");
        
        // Now we need to wrap it into the login return url parameter
        var resultLoginUrl = $"/login?returnUrl={targetMapUrlEncoded}";
        return resultLoginUrl;
    }
    
    public async Task InviteNewUser(AppUser user)
    {
        var url = GenerateAccountmapRelativeUrl(user);
        logger.LogInformation("Pseudo user invite: {}", url);
        
        // Send user message with the link 
        var messageBuilder = messageBuilderProvider.CreateBuilder();
        var context = new
        {
            User = user,
            ActivationUrl = url,
            SystemName = appOptions.Value.Name
        };

        messageBuilder.BodyAndSubjectFromFileTemplate(context, "LoginInviteTemplate.hbs");
        messageBuilder.Subject($"Sign up to {context.SystemName}");
        messageBuilder.AddRecipient(user);
        await messageBuilder.BuildAndSendAsync();
    }

    public async Task<IEnumerable<AppUser>> GetAsync(Expression<Func<AppUser, bool>>? predicate = null)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var qr = db.Set<AppUser>().AsQueryable();
        if (predicate is not null) qr = qr.Where(predicate);
        qr = qr.Include(u => u.UserInRoles)
            .ThenInclude(uir => uir.Role)
            .Include(u => u.UserInRoles)
            .ThenInclude(uir => uir.Organization)
            .Include(u => u.Contacts);
        
        
        
        return await qr.ToListAsync();
    }

    public async Task<ItemsResult<AppUser>> GetUsersAsync(IFilter filter)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(filter.CancellationToken);
        
        var contactQuery = db.Set<Contact>()
            .Include(c => c.AppUser)
            .Where(c => c.IsPrimary)
            .AsNoTracking();
        
        
        // if (filter.FilterQuery is not null)
        // {
        //     // Filter by firstname, lastname of email
        //     contactQuery = contactQuery.Where(c =>
        //         EF.Functions.Like(c.Firstname ?? string.Empty, $"%{filter.FilterQuery}%") ||
        //         EF.Functions.Like(c.Lastname ?? string.Empty, $"%{filter.FilterQuery}%") ||
        //         EF.Functions.Like(c.Email ?? string.Empty, $"%{filter.FilterQuery}%"));
        // }
        
        // var count = await contactQuery.CountAsync();
        //
        //
        // contactQuery = contactQuery
        //     .Include(c => c.AppUser)
        //     .OrderBy(c => c.Lastname)
        //     .Skip(filter.Offset)
        //     .Take(filter.Count);
        
        // Implmentation above does not solve solve punctuation and case sensitivity problem
        // For now, fetch all of them and filter in memory instead
        var result = await contactQuery.ToListAsync(cancellationToken: filter.CancellationToken);
        
        var resultUsers = result
            .Where(c => c.IsFilterMatch(filter.FilterQuery))
            .Select(r => r.AppUser)
            .Distinct()
            .OrderBy(r => r.Lastname)
            .ToList();
            
        var count = resultUsers.Count;
        resultUsers = resultUsers    
            .Skip(filter.Offset)
            .Take(filter.Count)
            .ToList();
        
        
        logger.LogTrace("GetUsers: filter={}, offsetreq={}, countreq={}, count {}, resultcount {}", 
            filter.FilterQuery, filter.Offset, filter.Count, count, resultUsers.Count);
        
        return new ItemsResult<AppUser>(resultUsers, count);
    }


    // Following interface implementation is obsolete
    public async Task<IEnumerable<AppUser>> GetItems(string? filter = null, CancellationToken ct = default)
    {
        var dt = DateTime.UtcNow;
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var users = await db.Set<AppUser>()
            .Include(u => u.Contacts)
            .ToListAsync(cancellationToken: ct);
        logger.LogDebug("Materialized users in {Elapsed}", DateTime.UtcNow - dt);
        if (string.IsNullOrWhiteSpace(filter)) return users;
        return users.Where(u => u.IsFilterMatch(filter));
    }
    
    public async Task<IEnumerable<AppUser>> GetItemsExcept(AppUser except, string? filter = null, CancellationToken ct = default)
    {
        var items = await GetItems(filter, ct);
        return items.Where(u => !u.Equals(except));
    }
    

    public AppUser CreateItem()
    => AppUser.NewEmpty();

    public async Task PersistItem(AppUser item)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.Attach(item);
        await db.SaveChangesAsync();
        logger.LogInformation("Persisted user: {}", item.Id);
    }
    
    public async Task<IEnumerable<(AppRole role, Organization? organization)>> GetRolesForUserAsync(AppUser user)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        var entry = db.Attach(user);
        await db.Set<UserInRole>()
            .Include(u => u.Organization)
            .Include(u => u.Role)
            .ThenInclude(u => u.Children)
            .Where(u => u.UserId == user.Id)
            .LoadAsync();

        await entry.Collection(e => e.UserInRoles).LoadAsync();

        return user.Roles;
    }

    public async Task<IEnumerable<AppRole>> GetRolesAsync(Expression<Func<AppRole, bool>>? predicate = null)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var qr = db.Set<AppRole>().AsQueryable();
        if (predicate is not null) qr = qr.Where(predicate);
        return await qr.ToListAsync();
    }

    public async ValueTask<ItemsProviderResult<AppRole>> GetRolesAsync(ItemsProviderRequest request,
        string? search = null)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        
        List<AppRole> tmpResult = await db.Set<AppRole>()
            .OrderBy(apr => apr.Name)
            .ToListAsync();
        if (!string.IsNullOrWhiteSpace(search))
        {
            tmpResult = tmpResult.
                Where(r => StringUtils.IsFilterMatchAtLeastOneOf(search, r.Name, r.Description, r.Id, r.DisplayName))
                .ToList();
        }

        var totalCount = tmpResult.Count;
        tmpResult = tmpResult
            .Skip(request.StartIndex)
            .Take(request.Count)
            .ToList();
        return new ItemsProviderResult<AppRole>(tmpResult, totalCount);
    }

    public async Task<AppRole> EnsureRole(AppRole role)
    {
        await using var db =  await dbContextFactory.CreateDbContextAsync();
        var groups = db.Set<AppRole>();
        var grp = await groups.FirstOrDefaultAsync(g => g.Id == role.Id);

        if (grp is null)
        {
            groups.Add(role);
            await db.SaveChangesAsync();
            grp = role;
        }

        return grp;
    }
    
    
    
    public async Task EnsureUserInRole(UserInRole userInRole)
    {
        // Wrapping - create scope for original user manager and get db context from it
        using var scope = serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        
        
        // Find the group
        var role = await EnsureRole(new AppRole() { Id = userInRole.RoleId });
        
        var memberInGroups = db.Set<UserInRole>();
        // Check if user already exists in the group, if not, add group membership
        if (!await memberInGroups.AnyAsync(m => m.UserId == userInRole.UserId && m.RoleId == role.Id && m.OrganizationId == userInRole.OrganizationId))
        {
            logger.LogInformation("Adding user {} to role {}", userInRole.UserId, role.Id);
            memberInGroups.Add(userInRole);
                
            await db.SaveChangesAsync();

            // Updating security stamp will force user to relog and get a new cookie, if configured so
            var user = await userManager.FindByIdAsync(userInRole.UserId.ToString());
            if (user != null) await UpdateSecurityStampAsync(user);
        }
    }

    public Task EnsureUserInRole(AppUser user, AppRole role, Organization organization)
    {
        return EnsureUserInRole(new UserInRole()
            {RoleId = role.Id, UserId = user.Id, OrganizationId = organization.Id});
    }

    public async Task EnsureUserNotInRole(UserInRole userInRole)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        
        var userid = userInRole.User?.Id ?? userInRole.UserId;
        var roleid = userInRole.Role?.Id ?? userInRole.RoleId;
        var organizationid = userInRole.Organization?.Id ?? userInRole.OrganizationId;

        var delme = await db.Set<UserInRole>()
            .Where(r => r.RoleId == roleid && r.UserId == userid && r.OrganizationId == organizationid)
            .ToListAsync();
        
        foreach (var delRole in delme)
        {
            db.Remove(delRole);
        }
        
        await db.SaveChangesAsync();
    }

    public async Task MergeUsers(AppUser userFrom, AppUser userTo)
    {
        logger.LogDebug("Initiated user merging - {} to {}", userFrom.Fullcontact, userTo.Fullcontact);
        await entityMerger.MergeEntitiesAsync(userFrom, userTo);
        await UpdateSecurityStampAsync(userTo);
    }

    public async Task UpdateSecurityStampAsync(AppUser user)
    {
        // Wrapping - create scope for original user manager and get db context from it
        using var scope = serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Attach(user);
        
        await userManager.UpdateSecurityStampAsync(user);
    }

    public async Task DeleteUser(AppUser user)
    {
        // Wrapping - create scope for original user manager and get db context from it
        using var scope = serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Attach(user);
        
        
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new UserException(result.Errors.First().Description);
        }
    }

    public async Task NewUserAsync(NewUserModel model)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.Set<AppUser>().Add(model.UserDetails);
        await db.SaveChangesAsync();
        
        if (model.InviteToTheSystem)
        {
            await InviteNewUser(model.UserDetails);
        }
    }

    public async Task EnsureUserNotHaveLogin(AppUser user, UserLoginInfo userLoginInfo)
    {
        // This does not work since framework is not calling save changes and does not offer any way to do it, since saving
        // changes is protected
        // var store = (IUserLoginStore<AppUser>) Store;
        // await store.RemoveLoginAsync(user, userLoginInfo.LoginProvider,
            // userLoginInfo.ProviderKey, CancellationToken.None);
            
        await using var db = await dbContextFactory.CreateDbContextAsync();
        
        var login = await db.UserLogins.FirstOrDefaultAsync(l =>
            l.LoginProvider == userLoginInfo.LoginProvider && l.ProviderKey == userLoginInfo.ProviderKey &&
            l.UserId == user.Id);

        if (login is not null)
        {
            db.UserLogins.Remove(login);
        }

        await db.SaveChangesAsync();
    }
    
    public async Task<AppUser?> FindByIdAsync(Guid? userId)
    {
        // Wrapping - create scope for original user manager and get     db context from it
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var user = await db.Set<AppUser>()
            .Include(u => u.Contacts)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user;
    }

    public Task<AppUser?> FindByIdAsync(string id)
        => FindByIdAsync(Guid.Parse(id));

    public async Task<string> GetSecurityStampAsync(AppUser user)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Attach(user);
        
        return await userManager.GetSecurityStampAsync(user);
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(AppUser user)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Attach(user);
        
        return await userManager.GetLoginsAsync(user);
    }

    public async Task UpdateAsync(AppUser user)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.Update(user);
        await db.SaveChangesAsync();
    }
    
    private (IServiceScope, UserManager<AppUser>, AppDbContext) _PrepareScopedDbHelper(AppUser? attachUser = null) 
    {
        var scope = serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (attachUser is not null)
            ctx.Attach(attachUser);
        return (scope, userManager, ctx);
    } 

    public async Task AddLoginAsync(AppUser user, UserLoginInfo externalLoginInfo)
    {
        var (s, userManager, _) = _PrepareScopedDbHelper(user);
        using var scope = s;
        await userManager.AddLoginAsync(user, externalLoginInfo);
    }

    public async Task<AppUser?> FindByLoginAsync(string loginProvider, string providerKey)
    { 
        using var scope = serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByLoginAsync(loginProvider, providerKey);
        if (user is not null)
        {
            // Load contacts for the user as well
            await using var db = await dbContextFactory.CreateDbContextAsync();
            db.Attach(user);
            await db.Entry(user).Collection(u => u.Contacts).LoadAsync();
        }

        return user;
    }

    public async Task<IdentityResult> RemoveLoginAsync(AppUser existingUser, string extinfoLoginProvider, string extinfoProviderKey)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Attach(existingUser);
        
        return await userManager.RemoveLoginAsync(existingUser, extinfoLoginProvider, extinfoProviderKey);
    }

    public async Task<IList<UserInRole>> GetUserRolesAsync(AppUser user)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.Set<UserInRole>()
            .Include(u => u.Role)
            .Include(u => u.Organization)
            .Where(u => u.UserId == user.Id)
            .ToListAsync();
    }

    public async Task<AppUser> EnsureUserByEmailAsync(string email)
    {
        // Wrapping - create scope for original user manager and get db context from it
        using var scope = serviceScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var mbox = MailboxAddress.Parse(email.Trim());
        var contact = await ctx.Set<Contact>()
            .Include(c => c.AppUser)
            .FirstOrDefaultAsync(a => a.Email == mbox.Address);
        
        if (contact is null)
        {
            var user = AppUser.NewEmpty();
            user.Id = Guid.NewGuid();
            user.PrimaryContact.Email = mbox.Address;

            await userManager.CreateAsync(user);
            return user;
        }

        return contact.AppUser;
    }

    // public async Task<AppUser> EnsureUserAsync(AppUser user)
    // {
    //     // Wrapping - create scope for original user manager and get db context from it
    //     using var scope = _serviceScopeFactory.CreateScope();
    //     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    //     var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //     
    //     // Is the new user in db by id?
    //     var userf = await ctx.Set<AppUser>().FindAsync(user.Id);
    //     if (userf is not null)
    //         return userf;
    //     
    //     // No, does email match then?
    //     userf = await ctx.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == user.Email);
    //     
    //
    // }

    public async Task<AppUser?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var result = await db.Set<Contact>()
            .Include(u => u.AppUser)
            .Where(u => u.Email == email)
            .ToListAsync(cancellationToken);
        
        return result.FirstOrDefault()?.AppUser;
    }
    
    
    public async Task<AppUser?> FindByCpAsync(
        ClaimsPrincipal claimsPrincipal, 
        bool loadLoginInfo = false,
        CancellationToken cancellationToken=default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var validid = Guid.TryParse(claimsPrincipal.GetId(), out var id);
        if (validid)
        {
            var qry = db.Set<AppUser>()
                .Include(u => u.Contacts)
                .Include(u => u.UserInRoles)
                .AsQueryable();
            if (loadLoginInfo)
                qry = qry.Include(u => u.IdentityUserLogins);
            var result =
                await qry.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
            return result;
        }

        return null;
    }
    
    public async Task<IEntityContext<AppUser>?> FindByCpEditableAsync(ClaimsPrincipal cp, CancellationToken ct = default)
    {
        var user = await FindByCpAsync(cp, loadLoginInfo:true, cancellationToken: ct);
        return user is null ? null : new EfCoreEntityContext<AppDbContext, AppUser>(user, dbContextFactory);
    }
    

    // -------- TODO

    public enum MatchUserStrategy
    {
        Exact 
    }
    
    public async Task<AppUser?> MatchUser(Contact contact)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        
        // Start by matching user directly
        // Try by email
        var user = await db.Set<AppUser>().FirstOrDefaultAsync(u => u.Email == contact.Email);
        if (user is not null) return user;

        // Try by matching contact
        var contactMatched = await db.Set<Contact>()
            .Include(c => c.AppUser)
            .FirstOrDefaultAsync(c => c.Email == contact.Email);

        if (contactMatched is not null) return contactMatched.AppUser;

        return null;
    }

    public async Task<AppUser> MatchEnsureUser(Contact contact)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();

        var match = await MatchUser(contact);
        if (match is not null) return match;
        
        // We have to create new user 
        var newUserModel = new NewUserModel()
        {
            UserDetails = AppUser.NewEmpty()
        };

        newUserModel.UserDetails.PrimaryContact.Firstname = contact.Firstname;
        newUserModel.UserDetails.PrimaryContact.Lastname = contact.Lastname;
        newUserModel.UserDetails.PrimaryContact.Email = contact.Email;
        
        await NewUserAsync(newUserModel);
        return newUserModel.UserDetails;
    }

    public async Task<IList<Claim>> GetClaimsAsync(AppUser user)
    {
        var (s, userManager, _) = _PrepareScopedDbHelper(user);
        using var scope = s;
        var result = await userManager.GetClaimsAsync(user);
        return result;
    }

    public async Task AddClaimsAsync(AppUser user, IEnumerable<Claim> claims, CancellationToken ct = default)
    {
        var (s, userManager, _) = _PrepareScopedDbHelper(user);
        using var scope = s;
        await userManager.AddClaimsAsync(user, claims);
    }
}