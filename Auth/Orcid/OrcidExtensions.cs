using Microsoft.AspNetCore.Authentication;

namespace sip.Auth.Orcid;

public static class OrcidExtensions
{
    /// <summary>
    /// Adds Orcid OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
    /// The default scheme is specified by <see cref="OrcidDefaults.AUTHENTICATION_SCHEME"/>.
    /// <para>
    /// Orcid authentication allows application users to sign in with their Orcid account.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddOrcid(this AuthenticationBuilder builder)
        => builder.AddOrcid(OrcidDefaults.AUTHENTICATION_SCHEME, _ => { });

    /// <summary>
    /// Adds Orcid OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
    /// The default scheme is specified by <see cref="OrcidDefaults.AUTHENTICATION_SCHEME"/>.
    /// <para>
    /// Orcid authentication allows application users to sign in with their Orcid account.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="configureOptions">A delegate to configure <see cref="OrcidOptions"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddOrcid(this AuthenticationBuilder builder,
        Action<OrcidOptions> configureOptions)
        => builder.AddOrcid(OrcidDefaults.AUTHENTICATION_SCHEME, configureOptions);

    /// <summary>
    /// Adds Orcid OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
    /// The default scheme is specified by <see cref="OrcidDefaults.AUTHENTICATION_SCHEME"/>.
    /// <para>
    /// Orcid authentication allows application users to sign in with their Orcid account.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="authenticationScheme">The authentication scheme.</param>
    /// <param name="configureOptions">A delegate to configure <see cref="OrcidOptions"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddOrcid(this AuthenticationBuilder builder, string authenticationScheme,
        Action<OrcidOptions> configureOptions)
        => builder.AddOrcid(authenticationScheme, OrcidDefaults.DisplayName, configureOptions);

    /// <summary>
    /// Adds Orcid OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
    /// The default scheme is specified by <see cref="OrcidDefaults.AUTHENTICATION_SCHEME"/>.
    /// <para>
    /// Orcid authentication allows application users to sign in with their Orcid account.
    /// </para>
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
    /// <param name="authenticationScheme">The authentication scheme.</param>
    /// <param name="displayName">A display name for the authentication handler.</param>
    /// <param name="configureOptions">A delegate to configure <see cref="OrcidOptions"/>.</param>
    /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
    public static AuthenticationBuilder AddOrcid(this AuthenticationBuilder builder, string authenticationScheme,
        string displayName, Action<OrcidOptions> configureOptions)
        => builder.AddOAuth<OrcidOptions, OrcidHandler>(authenticationScheme, displayName, configureOptions);
}