using System.Text.Encodings.Web;
using System.Web;
using Microsoft.AspNetCore.Authentication;

namespace sip.Auth.Saml2;

public class Saml2AuthenticationHandler(
        IOptionsMonitor<Saml2AuthenticationOptions> options,
        ILoggerFactory                              logger,
        UrlEncoder                                  encoder,
        ISaml2MetadataProvider                      saml2MetadataProvider)
    : RemoteAuthenticationHandler<Saml2AuthenticationOptions>(options, logger, encoder)
{
    public override async Task<bool> HandleRequestAsync()
    {
        // Here we determine whether request is hitting login endpoint
        // This actually might be terrible approach, but it does not require adding additional api endpoints
        // and everything gets handled by this handler
        if (Request.Path == Options.LoginPath)
        {
            // Challenge self - this will set the redirect headers.
            await ChallengeAsync(new AuthenticationProperties());
            return true;
        }
        
        return await base.HandleRequestAsync();
    }

    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        // TODO - try/catch?
        // TODO - support query together with form? 
        var samlResponse = Request.Form["SAMLResponse"];
        var samlResponseDecoded = Convert.FromBase64String(samlResponse);
        var relayState = Request.Form["RelayState"];
        
        Logger.LogDebug("Received saml response: \nSAMLResponse={} \nSAMLResponseDecoded={} \nRelayState={}", 
            samlResponse, Encoding.UTF8.GetString(samlResponseDecoded), relayState);
        
        var returnUrl = HttpUtility.ParseQueryString(relayState)
            .Get(Options.ReturnUrlParameter);
        var redirectUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;

        // Extract response, issuer, userid and get metadata for the issuer
        var samlRes = new Saml2Response(samlResponseDecoded);
        var issuer = samlRes.GetIssuer();
        var userid = samlRes.GetRequiredCustomAttribute(Options.UserIdAttribute);
        var metadata = await saml2MetadataProvider.GetMetadata(Options, issuer);
        
        Logger.LogDebug("Handling saml2: returnUrl={}, redirectUrl={}, samlRes={}, issuer={}, userid={}", returnUrl,
            redirectUrl, samlRes, issuer, userid);
        
        // Validate the response, using certificate from issuer's metadata
        if (!samlRes.IsValid(metadata.SignInCerts.First()))
        {
            return HandleRequestResult.Fail($"Saml2Response is not valid, user {userid} of issuer {issuer} is denied");
        }
        
        // Create principal 
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userid),
            new(ClaimTypes.AuthenticationMethod, Scheme.Name)
        };

        var cp = new ClaimsPrincipal(new ClaimsIdentity(claims));
        
        // Create Ticket 
        var props = new AuthenticationProperties()
        {
            RedirectUri = redirectUrl,
            IsPersistent = false,
        };
        
        props.SetString("LoginProvider", issuer); // TODO - why the heck is the constant privately hidden in SignInManager?
        var ticket = new AuthenticationTicket(cp, props, Scheme.Name);
        
        return HandleRequestResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        
        var entityId = properties.GetString(Saml2AuthenticationOptions.ENTITYID_PROPERTY_KEY)
            ?? Request.Query[Options.EntityIdQueryKey];
        var returnUrl = properties.RedirectUri ?? Request.Query[Options.ReturnUrlParameter];
        var consumeReturnUrl = BuildRedirectUri(Options.CallbackPath);
        var loginReturnUrl = BuildRedirectUri(Options.LoginPath);

        if (string.IsNullOrWhiteSpace(entityId))
        {
            // We do not have entity id. Either redirect to WS or fail if no WS is configured.
            var dsUri = Options.DiscoveryServiceUrl;
            if (dsUri is null)
            {
                // No entityID nor discovery service - fail
                Logger.LogError("Saml 2 requires either idp target (entityid) or discovery service URL, where IDP is chosen by the user");
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl)) loginReturnUrl += $"?{Options.ReturnUrlParameter}={HttpUtility.UrlEncode(returnUrl)}";
            var query = HttpUtility.ParseQueryString(dsUri.Query);
            // Add parameters for the DS - the service provider that uses it (this app) and where the DS should return information 
            // about selected entity.
            query["entityID"] = Options.SpEntityId;
            query["return"] = loginReturnUrl;
            var urlb = new UriBuilder(dsUri)
            {
                Query = query.ToString()
            };

            Logger.LogInformation("Redirecting to discovery service, url: {Url}", urlb.ToString());
            //var redirectContext = new RedirectContext<Saml2AuthenticationOptions>(Context, Scheme, Options, properties, urlb.ToString());
            //await Events.RedirectToLogin(redirectContext); TODO - howto?
            Response.Redirect(urlb.ToString());
            return;
        }
        
        // We do have an entity id. Redirect to it. 
        // In such case, we should redirect to identity provider
        var metadata = await saml2MetadataProvider.GetMetadata(Options, entityId);
        var request = new Saml2Request(Options.SpEntityId, consumeReturnUrl, metadata.SingleSignOnDestination);
        var relayState = string.IsNullOrWhiteSpace(returnUrl) ? "" : $"{Options.ReturnUrlParameter}={returnUrl}";
        var red = request.GetRedirectUrl(metadata.SingleSignOnDestination, relayState);
        Logger.LogDebug("Challenged and entityID found, entityID={} relayState={} redirectUri={}, \n metaeid={}, \n metadest={}, \n  metakeys={}", entityId, relayState, red, 
            metadata.EntityId, metadata.SingleSignOnDestination, string.Join(";", metadata.SignInCerts.Select(s => s.ToString())));
        Response.Redirect(red);
    }
}