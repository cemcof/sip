using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace sip.Auth.Saml2;
public static class Saml2Ext
{
    /// <summary>
    /// Adds Saml2 authentication configured via <see cref="IConfigurationSection"/>. Scheme name will be the section key />
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configurationSection"></param>
    public static AuthenticationBuilder AddSaml2(this AuthenticationBuilder builder, IConfigurationSection configurationSection)
    {
        
        var scheme = configurationSection.Key;
        var schemeDisplayName = configurationSection.GetValue<string>("DisplayName");
        if (string.IsNullOrWhiteSpace(schemeDisplayName)) schemeDisplayName = "Saml2 Identity Provider";
        
        // Gui options
        var guiOptions = new LoginProviderGuiOptions();
        configurationSection.Bind(guiOptions);
        builder.Services.Configure<Dictionary<string, LoginProviderGuiOptions>>(o =>
            o[scheme] = guiOptions);
        
        builder.Services.TryAddSingleton<ISaml2MetadataProvider, Saml2MetadataProvider>();
        
        var optbuilder = builder.Services.AddOptions<Saml2AuthenticationOptions>(scheme);
        optbuilder.Bind(configurationSection);

        // configure ??= _ => { };
        // optbuilder.Configure(configure); // Configure options via the builder - pass null when calling AddScheme

        builder.AddScheme<Saml2AuthenticationOptions, Saml2AuthenticationHandler>(
            scheme, displayName: schemeDisplayName, null);

        return builder;
    }
}