using sip.Core;

namespace sip.Messaging;

public class GeneralMessageHandler(
        IDbContextFactory<AppDbContext> dbFac,
        IRawMessageSender               sender,
        TimeProvider                    timeProvider,
        ILogger<GeneralMessageHandler>  logger)
    : IMessageEgressHandler<GeneralMessage>
{
    protected readonly IDbContextFactory<AppDbContext> DbFac  = dbFac;
    protected readonly IRawMessageSender               Sender = sender;
    protected readonly ILogger                         Logger = logger;

    public virtual async ValueTask<MessageHandleResult> HandleEgress(GeneralMessage message)
    {
        await SendAndSaveMessage(message);
        return MessageHandleResult.Handled;
    }

    public async Task SendAndSaveMessage<TMessage>(TMessage message) where  TMessage : Message
    {
        // Save message to the database and send it
        await using var dbctx = await DbFac.CreateDbContextAsync();

        message.MessageType = MessageType.SystemOut;
        message.DtCreated = timeProvider.DtUtcNow();
        message.MessageStatus = MessageStatus.Pending;

        dbctx.Add(message);

        await dbctx.SaveChangesAsync();
        
        // Now send the message
        await Sender.SendMessage(message.MessageData.Message);

        message.MessageId = message.MessageData.Message.MessageId;
        message.MimeHeaders = message.MessageData.Message.Headers.GetAsString();
        message.MessageStatus = MessageStatus.Handled;
        message.DtStatusChanged = timeProvider.DtUtcNow();
        await dbctx.SaveChangesAsync();
    }
}