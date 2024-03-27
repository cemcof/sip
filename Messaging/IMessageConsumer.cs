namespace sip.Messaging;

public interface IMessageConsumer<in TMessage> where TMessage : Message
{
    Task ConsumeMessageAsync(TMessage message);
}