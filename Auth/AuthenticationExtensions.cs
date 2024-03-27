using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using sip.Auth.Orcid;

namespace sip.Auth;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds and configures <see cref="Microsoft.AspNetCore.Authentication.Google"/> authentication provider using
    /// configuration section. 
    /// </summary>
    /// <param name="authenticationBuilder"></param>
    /// <param name="configurationSection"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder authenticationBuilder, IConfigurationSection configurationSection)
    {
        var schemeName = GoogleDefaults.AuthenticationScheme;
        var guiOptions = new LoginProviderGuiOptions();
        configurationSection.Bind(guiOptions);
        
        authenticationBuilder.Services.Configure<GoogleOptions>(schemeName, configurationSection);
        authenticationBuilder.Services.Configure<Dictionary<string, LoginProviderGuiOptions>>(o =>
            o[GoogleDefaults.AuthenticationScheme] = guiOptions);
        authenticationBuilder.AddGoogle(schemeName, o => o.SignInScheme = IdentityConstants.ExternalScheme);
        return authenticationBuilder;
    }
    
    /// <summary>
    /// Adds and configures <see cref="Microsoft.AspNetCore.Authentication.Google"/> authentication provider using
    /// configuration section. 
    /// </summary>
    /// <param name="authenticationBuilder"></param>
    /// <param name="configurationSection"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddOrcid(this AuthenticationBuilder authenticationBuilder, IConfigurationSection configurationSection)
    {
        var schemeName = OrcidDefaults.AUTHENTICATION_SCHEME;
        var guiOptions = new LoginProviderGuiOptions();
        configurationSection.Bind(guiOptions);
        
        authenticationBuilder.Services.Configure<OrcidOptions>(schemeName, configurationSection);
        authenticationBuilder.Services.Configure<Dictionary<string, LoginProviderGuiOptions>>(o =>
            o[OrcidDefaults.AUTHENTICATION_SCHEME] = guiOptions);
        authenticationBuilder.AddOrcid(schemeName, o => o.SignInScheme = IdentityConstants.ExternalScheme);
        return authenticationBuilder;
    }
}