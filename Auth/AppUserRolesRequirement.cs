using Microsoft.AspNetCore.Authorization;

namespace sip.Auth;

public class AppUserRolesRequirement(IReadOnlyList<string>? roles = null, IOrganization? organization = null) : IAuthorizationRequirement
{
    public IReadOnlyList<string>? Roles { get; } = roles;
    public IOrganization? Organization { get; } = organization;
}

public class AppUserRolesRequirementHandler(IAppUserProvider userProvider) : AuthorizationHandler<AppUserRolesRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AppUserRolesRequirement requirement)
    {
        var user = await userProvider.FindByCpAsync(context.User);

        if (user is null)
        {
            context.Fail(new AuthorizationFailureReason(this, "User could not be found using current ClaimsPrincipal"));
            return;
        }

        if (requirement.Roles is null)
        {
            context.Succeed(requirement);
            return;
        }

        var org = (Organization?)requirement.Organization; // TODO - this is dirty for now, consider how to handle IOrganization thing
        var isInRole = user.IsInRole(requirement.Roles, org);
        if (isInRole)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, "User is not in given roel"));
        }
    }
}