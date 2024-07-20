namespace sip.Auth.Orcid;

public static class OrcidDefaults
{
    /// <summary>
    /// The default scheme for Orcid authentication. Defaults to <c>Orcid</c>.
    /// </summary>
    public const string AUTHENTICATION_SCHEME = "Orcid";

    /// <summary>
    /// The default display name for Orcid authentication. Defaults to <c>Orcid</c>.
    /// </summary>
    public static readonly string DisplayName = "Orcid";

    public const string LOGIN_PROVIDER = "orcid";

    /// <summary>
    /// The default endpoint used to perform Orcid authentication.
    /// </summary>
    /// <remarks>
    /// For more details about this endpoint, see https://developers.google.com/identity/protocols/oauth2/web-server#httprest
    /// </remarks>
    public static readonly string AuthorizationEndpoint = "https://orcid.org/oauth/authorize";

    /// <summary>
    /// The OAuth endpoint used to exchange access tokens.
    /// </summary>
    public static readonly string TokenEndpoint = "https://orcid.org/oauth/token";
    
    public const string ORCID_REGEX = @"^(\d{4}-){3}(\d|X){4}$";
}