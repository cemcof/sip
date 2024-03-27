namespace sip.RemoteAccess;

public record RemoteConnectionInfo
{
    [Required] public string Name { get; set; } = null!;
    [Required] public string Protocol { get; set; } = null!;
    [Required] public string Hostname { get; set; } = null!;
    [Required] public int Port { get; set; }
    [Required] public string Password { get; set; } = null!;
}