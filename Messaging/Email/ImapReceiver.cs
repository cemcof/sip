using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Internal;
using sip.Scheduling;

namespace sip.Messaging.Email;

public class ImapReceiver(
        IOptionsMonitor<ScheduledServiceOptions> optionsMonitor,
        IOptions<ImapOptions>                    imapOptions,
        TimeProvider                             timeProvider,
        ILogger<ImapReceiver>                    logger,
        IRawMessageReceiver                      receiver)
    : ScheduledService(optionsMonitor, timeProvider, logger)
{
    protected override async Task ExecuteRoundAsync(CancellationToken stoppingToken)
    {
        var opts = imapOptions.Value;
        using var ic = new ImapClient(new ProtocolLogger("imap.log"));
        
        // Connect and authenticate
        logger.LogDebug("Connecting to IMAP server {Server} on port {Port}", opts.Host, opts.Port);
        await ic.ConnectAsync(opts.Host, opts.Port, opts.SecureSocket, stoppingToken);
        logger.LogDebug("Authenticating IMAP user {User}", opts.Login);
        await ic.AuthenticateAsync(new NetworkCredential(opts.Login, opts.Password), stoppingToken);
        logger.LogDebug("Successfully connected and authenticated to imap as {User}", opts.Login);
        
        // Fetch messages and submit them for processing, for now, one by one
        logger.LogDebug("Fetching INBOX Messages");

        await ic.Inbox.OpenAsync(FolderAccess.ReadWrite, stoppingToken);
        var inboxIds = await ic.Inbox.SearchAsync(SearchQuery.All, stoppingToken);
        logger.LogDebug("Detected {Count} messages, now enumerating them", inboxIds.Count);
        foreach (var mimeMessageId in inboxIds)
        {
            try
            {
                var message = await ic.Inbox.GetMessageAsync(mimeMessageId, stoppingToken);
                await receiver.ReceiveMessage(message);
                await ic.Inbox.AddFlagsAsync(mimeMessageId, MessageFlags.Deleted, false, stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error during processing of message {}, message is not flagged for deleting and will be processed again in next run", mimeMessageId);
            }
            
        }
        
        await ic.Inbox.ExpungeAsync(stoppingToken);
        await ic.DisconnectAsync(true, stoppingToken);
    }
}