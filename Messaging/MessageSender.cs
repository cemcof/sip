namespace sip.Messaging;

/// <summary>
/// Default implementation of message sender that submits the message to the message handlers according to the message type.
/// </summary>
public class MessageSender(ILogger<MessageSender> logger, IServiceProvider serviceProvider)
    : IMessageSender
{
    private readonly ILogger<MessageSender> _logger          = logger;

    public async Task SendMessage<TMessage>(TMessage message) where TMessage : Message
    {
        // Get message egress handler for this message type
        var handler =
            serviceProvider.GetRequiredService(typeof(IMessageEgressHandler<>).MakeGenericType(typeof(TMessage)));

        // For now, just pass message to the handlers, but later we might add some queuing
        dynamic task =
            handler.GetType().GetMethod(nameof(IMessageEgressHandler<Message>.HandleEgress))!.Invoke(handler,
                new object?[] {message})!;

        await task;
    }
}