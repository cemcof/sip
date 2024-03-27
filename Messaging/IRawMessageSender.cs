namespace sip.Messaging;

public interface IRawMessageSender
{
    Task SendMessage(MimeMessage message);

    Task ResendMessage(MimeMessage message,
        IEnumerable<InternetAddress> resendTos,
        IEnumerable<InternetAddress> resendCcs,
        IEnumerable<InternetAddress> resendBccs);

}