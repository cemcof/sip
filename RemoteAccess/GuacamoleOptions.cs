namespace sip.RemoteAccess;

public class GuacamoleOptions
{
    [Required] public string BaseUrl { get; set; } = null!;
    [Required] public string SecretKey { get; set; } = null!;
}

