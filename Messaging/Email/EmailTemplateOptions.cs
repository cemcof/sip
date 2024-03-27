namespace sip.Messaging.Email;

public record EmailTemplateOptions
{
    public string Subject { get; set; } = string.Empty;
    public string Body    { get; set; } = string.Empty;
}