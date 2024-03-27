namespace sip.Messaging;

public interface IRawMessageReceiver
{
    Task ReceiveMessage(MimeMessage message);
}