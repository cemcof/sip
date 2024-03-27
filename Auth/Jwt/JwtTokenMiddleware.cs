using Microsoft.AspNetCore.Authentication;

namespace sip.Auth.Jwt;

/// <summary>
/// This middleware is to be invoked before standard authentication middleware - it should provider redirect when
/// using token in an URL, since that does not work for blazor server in standard way. 
/// </summary>
public class JwtTokenMiddleware(RequestDelegate next, ILogger<JwtTokenMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var tokQuery =
            (context.Request.Query["token"].Count > 0 && !string.IsNullOrWhiteSpace(context.Request.Query["token"]))
                ? context.Request.Query["token"][0]
                : null;
        var cookieRedirect = context.Request.Query.ContainsKey("redirectcookie");

        // Just continue with middleware normally if no token or cookie redirection is requested
        if (string.IsNullOrWhiteSpace(tokQuery) || !cookieRedirect)
        {
            await next(context);
            return;
        }


        // Now we have a token and url is requesting to store it within application cookie
        // context.Request.Headers.Authorization = "Bearer " + tokQuery[0]; // Create header with the token so 
        var authresult = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);

        if (authresult.None || !authresult.Succeeded)
        {
            logger.LogDebug("JWT auth failed: {}", authresult.Failure);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // Now we know that token authentication succeeded, sign principal to application cookie
        var principal = new ClaimsPrincipal(new ClaimsIdentity(authresult.Ticket.Principal.Claims,
            IdentityConstants.ApplicationScheme));
        await context.SignInAsync(IdentityConstants.ApplicationScheme, principal, authresult.Ticket.Properties);

        // Redirect to the same url, but without the token
        var redirectTarget = context.Request.Scheme + Uri.SchemeDelimiter + context.Request.Host +
                             context.Request.PathBase + context.Request.Path;

        logger.LogDebug("JWT middleware successfully completed, redirecting to: {}", redirectTarget);
        context.Response.Redirect(redirectTarget);
    }
}