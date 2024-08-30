namespace sip.Organizations.Centers;

public class CenterNetworkOptions
{
    public IPAddr[] TrustedProxies { get; set; } = [];
    /// <summary>
    /// It will not be possible to login when accessing from these networks / IPs
    /// Useful for internal computers where login is not desired.
    /// </summary>
    public IPAddr[] NoSignInNetworks { get; set; } = [];
}