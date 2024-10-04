namespace sip.Auth;

public class AuthOptions
{
    public const string EXTERNALMAP_CLAIM_TYPE = "EXTERNAL_MAP";


    /// <summary>
    /// When logging in from external provider, external info is mapped to the internal user database.
    /// If true and the internal user for given external login does not exist, it gets created.
    /// </summary>
    public bool CreateUserOnLoginIfNotExists { get; set; } = true;

    public bool ValidateSecurityStamp { get; set; } = true;
}

public class LoginProviderGuiOptions
{
    public bool Visible { get; set; } = true;
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
    public string LogoUrl { get; set; } = "/unknown.png";
    public int Order { get; set; } = int.MaxValue;
}

public class AuthorizationConstants
{
    public static string PolicyLoginable = "loginable";
    public static string PolicyLogoutable = "logoutable";
}