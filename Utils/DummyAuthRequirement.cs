using Microsoft.AspNetCore.Authorization;

namespace sip.Utils;

/// <summary>
/// Requirement that always succeeds
/// </summary>
public class DummyAuthRequirement : IAuthorizationRequirement;

// Handler 
public class DummyAuthHandler : AuthorizationHandler<DummyAuthRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DummyAuthRequirement requirement)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}