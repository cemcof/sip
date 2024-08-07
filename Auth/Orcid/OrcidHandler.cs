using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        var userElement = tokens.Response!.RootElement.Deserialize<JsonNode>();
        // Todo - obtain name and email, if possible
        // Use orcid api to obtain user information
        // if (!string.IsNullOrEmpty(Options.ApiEndpoint))
        // {
        //     using var httpc = httpClientFactory.CreateClient();
        //     httpc.BaseAddress = new Uri(Options.ApiEndpoint);
        //     httpc.DefaultRequestHeaders.Add("Content-Type", "application/orcid+xml");
        //     httpc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        //     var orcid = tokens.Response!.RootElement.GetString("orcid")!;
        //     var result = await httpc.GetStringAsync(orcid + "/" + "record");
        //     // TODO - deterimine, parse xml...
        //     Logger.LogInformation("OrcidHandler: Obtained user information from orcid api: {}", result);
        // }
        //
        
        if (tokens.Response is null) throw new InvalidOperationException("ORCID: No token response available");
        Logger.LogInformation("OrcidHandler ticket response: {@Response} \n\n{ResponseJson}", 
            tokens.Response, JsonSerializer.Serialize(tokens.Response));
        properties.SetString("LoginProvider", OrcidDefaults.LOGIN_PROVIDER); // To identify external login type later

        var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme,
            Options, Backchannel, tokens, userElement.Deserialize<JsonElement>());
        context.RunClaimActions();
        Logger.LogInformation("Claims after mapping: \n{}", 
            string.Join('\n', context.Identity!.Claims.Select(c => c.Type + " : " + c.Value)));
        await Events.CreatingTicket(context);
        return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
    }
}

public class OrcidOptions : OAuthOptions
{
    /// <summary>
    /// If given, handler will try to obtain and map additional user claims, such as name and email.
    /// </summary>
    public string? ApiEndpoint { get; set; } 
    
    public OrcidOptions()
    {
        CallbackPath = new PathString("/signin-orcid");
        AuthorizationEndpoint = OrcidDefaults.AuthorizationEndpoint;
        TokenEndpoint = OrcidDefaults.TokenEndpoint;
        // UserInformationEndpoint = OrcidDefaults.UserInformationEndpoint;
        // Scope.Add("openid");
        Scope.Add("/authenticate");
        // Scope.Add("/read-public");
        TimeProvider = TimeProvider.System;

        ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "orcid");
        ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
        ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
        ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    }
}