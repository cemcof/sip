using sip.Core;

namespace sip.Messaging;

public class MessageService(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public async Task<Message?> FindInReplyToMessage(MimeMessage potentialReplyMesage)
    {
        // Determine binding by following rules, (in given order):
        // 1) By in-reply-to header with messageid
        // 2) By references header with last messageid

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var messages = db.Set<Message>();

        if (!string.IsNullOrWhiteSpace(potentialReplyMesage.InReplyTo))
        {
            // We have in reply to messageid
            var result = await messages.FirstOrDefaultAsync(m => m.MessageId == potentialReplyMesage.InReplyTo);
            if (result is not null) return result;
        }
        
        // In reply to unsuccessful, now try references
        var lastReference = potentialReplyMesage.References.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(lastReference))
        {
            // We have in reply to messageid
            var result = await messages.FirstOrDefaultAsync(m => m.MessageId == lastReference);
            if (result is not null) return result;
        }

        return null;
    }
}