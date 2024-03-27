using MailKit.Security;

namespace sip.Messaging.Email;

public abstract class EmailConnectionOptions
{
    [Required] public string Host { get; set; } = null!;
    [Required] public int Port { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public SecureSocketOptions SecureSocket { get; set; }
}