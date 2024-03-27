namespace sip.Messaging.Email;

public class SmtpOptions : EmailConnectionOptions
{
    public string From { get; set; } = null!;

    public string? DefaultReplyTo { get; set; } = null!; 
    public List<string> AppendReceivers { get; set; } = new();
    public string? SinkToReceiver { get; set; }
}