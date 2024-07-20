
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

using Microsoft.AspNetCore.Authentication;

namespace sip.Auth.Saml2;

public class Saml2AuthenticationOptions : RemoteAuthenticationOptions
{
    public const string SCHEME_NAME = "saml2";
    public const string ENTITYID_PROPERTY_KEY = "entityid";
        
    /// <summary>
    /// Entity ID of this service provider. 
    /// </summary>
    [Required] public string SpEntityId { get; set; } = null!;
    
    /// <summary>
    /// API endpoint of this application that initiates the login. 
    /// </summary>
    [Required] public PathString LoginPath { get; set; }
    
    /// <summary>
    /// If set, login attempt will redirect to DS instead of an IDP.
    /// In DS, user then selects desired IDP to proceed.
    /// </summary>
    public Uri? DiscoveryServiceUrl { get; set; }
    
    // ========== IDP metadata ============
    
    /// <summary>
    /// URL from where XML IDP metadata are downloadable from.
    /// Can be either single IDP (rooted as md:EntityDescriptor element) or multiple IDPs
    /// (rooted as EntitiesDescriptor element, with md:EntityDescriptor as children)
    /// </summary>
    [Required] public Uri IdpMetadataUrl { get; set; } = null!;
    
    /// <summary>
    /// URL from where IDP certificate is downloadable.
    /// If not set, IDP metadata validation is skipped. 
    /// </summary>
    public Uri? IdpMetadataCertUrl { get; set; }
    
    /// <summary>
    /// Whether to validate IDP metadata
    /// </summary>
    public bool ValidateIdpMetadata => IdpMetadataCertUrl is not null;

    public string EntityIdQueryKey { get; set; } = "entityID";
    
    /// <summary>
    /// How often to download (and possibly validate) IDP metadata. 
    /// </summary>
    public TimeSpan IdpMetadataRefreshInterval { get; set; } = TimeSpan.FromDays(7);
    
    
    /// <summary>
    /// Path to directory in which files necessary for SAML (e.g. IDP metadata) will be stored.
    /// </summary>
    public string DataDirectory { get; set; } = "Data/Tmp";

    public string IdpMetaCacheFilename = "idp_meta.xml";

    public string IdpMetaCachePath => Path.Combine(DataDirectory, IdpMetaCacheFilename);

    /// <summary>
    /// Name of the saml:Attribute containing the user identifier.
    /// </summary>
    public string UserIdAttribute { get; set; } = "urn:oid:1.3.6.1.4.1.5923.1.1.1.6";

    /// <summary>
    /// Used to extract additional claims from the saml response. Do not extract <see cref="ClaimTypes.NameIdentifier"/>, that is only
    /// required claim and is done automatically using <see cref="UserIdAttribute"/>.
    /// </summary>
    public Func<Saml2Response, IEnumerable<Claim>> ResponseClaimsProvider { get; set; } = DefulatResponseClaimsProvider;
    
    public Saml2AuthenticationOptions() : base()
    {
        // Set defaults
        SignInScheme = IdentityConstants.ExternalScheme;
        ReturnUrlParameter = "returnUrl";
    }
    
    public static IEnumerable<Claim> DefulatResponseClaimsProvider(Saml2Response saml2Response)
    {
        var claimMap = new Dictionary<string, Func<string?>>()
        {
            {ClaimTypes.Name, saml2Response.GetNameId},
            {ClaimTypes.Email, saml2Response.GetEmail},
            {ClaimTypes.GivenName, saml2Response.GetFirstName},
            {ClaimTypes.Surname, saml2Response.GetLastName},
            {ClaimTypes.Upn, saml2Response.GetUpn}
        };
        
        // Now create claims when produced value for it is not null
        return claimMap.Where(pair => pair.Value() is not null)
            .Select(pair => new Claim(pair.Key, pair.Value()!));
    } 
}