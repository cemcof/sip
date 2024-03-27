namespace sip.Messaging;

public interface IMessageSender
{
    Task SendMessage<TMessage>(TMessage message) where TMessage : Message;
}