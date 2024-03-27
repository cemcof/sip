namespace sip.Messaging;

/// <summary>
/// Task of thie receiver is to sanity check the message, select handler and pass it to it 
/// </summary>
public class MessageReceiver(IEnumerable<IMessageIngressHandler> messageHandlers, ILogger<MessageReceiver> logger)
    : IRawMessageReceiver
{
    public async Task ReceiveMessage(MimeMessage message)
    {
        // Sanitization by some headers (autoreply, mailer deamon)
        if (message.Headers.Any(h => h.Id is HeaderId.AutoSubmitted or HeaderId.Autosubmitted))
        {
            logger.LogInformation("Auto submitted email detected");
            return; // Just skip further handling
        }
        
        if (message.From.Mailboxes.Any(m => m.Address.Contains("mailer-daemon")))
        {
            logger.LogInformation("Mailer daemon detected: {address}", message.From.Mailboxes.First());
            return; // Just skip further handling
        }
        
        // Try handlers to handle the message
        foreach (var td in messageHandlers)
        {
            try
            {
                logger.LogDebug("Invoking handler {} on message with subject {}", td.GetType().Name, message.Subject);
                var handleResult = await td.HandleIngress(message);
                // If handler ignores the message, try next handler
                if (handleResult is not MessageHandleResult.Ignored) break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Unhandled error during message handling");
            }
        }
    }
}