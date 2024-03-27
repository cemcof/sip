namespace sip.Messaging;

public enum MessageHandleResult { Handled, Rejected, Ignored, Errored }

public interface IMessageEgressHandler<in TMessage> where TMessage : Message
{
    ValueTask<MessageHandleResult> HandleEgress(TMessage message);

}

public interface IMessageIngressHandler
{
    ValueTask<MessageHandleResult> HandleIngress(MimeMessage message);
}