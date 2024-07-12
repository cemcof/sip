using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace sip.Auth.Orcid;

public class OrcidHandler(
        IOptionsMonitor<OrcidOptions> options,
        ILoggerFactory                logger,
        UrlEncoder                    encoder)
    : OAuthHandler<OrcidOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity,
                                                                          AuthenticationProperties properties, OAuthTokenResponse tokens)
    {
        // Todo - obtain name and email, if possible

        if (tokens.Response is null) throw new InvalidOperationException("ORCID: No token response available");
        Logger.LogInformation("OrcidHandler ticket response: {}", tokens.Response.ToString());
        properties.SetString(nameof(OrcidDefaults.LOGIN_PROVIDER),
            OrcidDefaults.LOGIN_PROVIDER); // To identify external login type later

        var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme,
            Options, Backchannel, tokens, tokens.Response.RootElement);
        context.RunClaimActions();
        await Events.CreatingTicket(context);
        return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
    }
}

public class OrcidOptions : OAuthOptions
{
    public OrcidOptions()
    {
        CallbackPath = new PathString("/signin-orcid");
        AuthorizationEndpoint = OrcidDefaults.AuthorizationEndpoint;
        TokenEndpoint = OrcidDefaults.TokenEndpoint;
        // UserInformationEndpoint = OrcidDefaults.UserInformationEndpoint;
        // Scope.Add("openid");
        Scope.Add("/authenticate");
        TimeProvider = TimeProvider.System;

        ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "orcid");
        ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
        ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
        ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    }
}