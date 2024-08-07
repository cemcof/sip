using System.Web;
using Microsoft.AspNetCore.Authentication;

namespace sip.Auth.Jwt;

public static class JwtAuthExtensions
{
    public static AuthenticationBuilder AddTokenAuth(this AuthenticationBuilder authenticationBuilder,
        IConfigurationSection bindConf)
    {
        const string schemeName = JwtBearerDefaults.AuthenticationScheme;
        const string policySchemeName = "TOKEN_POLICY_DECIDER";

        authenticationBuilder.AddJwtBearer(schemeName, o =>
        {
            o.Events = new JwtBearerEvents
            {
                OnMessageReceived = HandleTokenReceived,
                OnAuthenticationFailed = HandleTokenError,
                OnTokenValidated = HandleTokenValidated
            };
        });

        var jwtOptsBuilder = authenticationBuilder.Services.AddOptions<JwtBearerOptions>(schemeName);
        jwtOptsBuilder.Bind(bindConf.GetSection("Consumer"));
        // Key now needs to be configured explicitly :/
        var key = bindConf.GetValue<string>("Consumer:TokenValidationParameters:Key");
        if (!string.IsNullOrEmpty(key))
            jwtOptsBuilder.Configure(jwtbo =>
                jwtbo.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)));

        // Now setup automatic default scheme detection - if there is token present in the request, do token authentication, otherwise use default identity authentication
        authenticationBuilder.AddPolicyScheme(policySchemeName, null, c =>
        {
            c.ForwardDefaultSelector = context =>
                ExtractToken(context) is null
                    ? IdentityConstants.ApplicationScheme
                    : schemeName;
        });

        // Set that policy scheme as default
        authenticationBuilder.Services.Configure<AuthenticationOptions>(o =>
            o.DefaultAuthenticateScheme = policySchemeName);


        // Now set up configuration for producing JWT's
        authenticationBuilder.Services.Configure<JwtProducerOptions>(bindConf.GetSection("Producer"));
        authenticationBuilder.Services.AddSingleton<JwtTokenProvider>();

        return authenticationBuilder;
    }

    // This enables jwt authentication handler to obtain token from additional soures
    private static Task HandleTokenReceived(MessageReceivedContext rcvCtx)
    {
        rcvCtx.Token = ExtractToken(rcvCtx.HttpContext);
        return Task.CompletedTask;
    }


    private static async Task HandleTokenValidated(TokenValidatedContext ctx)
    {
        // If the token contains claim specifying user id, we load user information from database 
        var userid = ctx.Principal?.GetId();
        if (!string.IsNullOrEmpty(userid))
        {
            var um = ctx.HttpContext.RequestServices.GetRequiredService<AppClaimsPrincipalFactory>();
            var newcp = await um.GenerateClaimsForUserId(userid, ctx.Principal!.Claims);

            // Overwrite the principal in the context, authentication will continue with the new one
            if (newcp is not null)
            {
                ctx.Principal = newcp;
            }
        }

        // Sign us to the application default scheme, since blazor will ignore the token in next requests
        if (ctx.Principal is not null)
        {
            await ctx.HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, ctx.Principal);
        }
        // We also redirect the browser
    }


    private static Task HandleTokenError(AuthenticationFailedContext authenticationFailedContext)
    {
        // If token is expired, we send the user a new one, if possible
        var token = ExtractToken(authenticationFailedContext.HttpContext);
        var validator = authenticationFailedContext.Options.SecurityTokenValidators.First();
        var tvp = authenticationFailedContext.Options.TokenValidationParameters.Clone();
        tvp.ValidateLifetime = false; // Ignore lifetime validation, we know it did not pass 
        var cp = validator.ValidateToken(token, tvp, out _);
        // Now we have both ClaimsPrincipal and Token details
        var targetmail = cp.FindFirstValue(JwtProducerOptions.JWT_REGEN_EMAIL_CLAIM_TYPE);
        if (targetmail is not null)
        {
            // If we have the email, we should send new token to that
            // TODO - send the email

            // Notify the user that we sent him new token - redirect him to notification page
            authenticationFailedContext.HttpContext.Response.Redirect(
                $"/new_token_notification/{HttpUtility.UrlEncode(targetmail.HideEmailPartially())}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Extract authentication token from http request
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private static string? ExtractToken(HttpContext ctx)
    {
        // Check for token presence in the query
        var tokQuery = ctx.Request.Query["token"];

        if (tokQuery.Count > 0 && !string.IsNullOrWhiteSpace(tokQuery[0]))
        {
            return tokQuery[0];
        }

        // Check for token presence in headers
        string? authorization = ctx.Request.Headers.Authorization;

        // If no authorization header found, nothing to process further
        if (!string.IsNullOrEmpty(authorization) &&
            authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var tok = authorization.Substring("Bearer ".Length).Trim();
            if (!string.IsNullOrEmpty(tok)) return tok;
        }


        // No token available
        return null;
    }
}