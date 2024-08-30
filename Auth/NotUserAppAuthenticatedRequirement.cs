using Microsoft.AspNetCore.Authorization;

namespace sip.Auth;

public class NotUserAppAuthenticatedRequirement : AuthorizationHandler<NotUserAppAuthenticatedRequirement>, IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NotUserAppAuthenticatedRequirement requirement)
    {
        if (context.User.IsAppAuthenticated())
            context.Fail();
        else 
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}