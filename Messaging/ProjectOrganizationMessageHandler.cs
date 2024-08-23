using System.Text.RegularExpressions;
using sip.Core;

namespace sip.Messaging;

public class ProjectOrganizationMessageHandler(
        IDbContextFactory<AppDbContext>                           dbContextFactory,
        IEnumerable<IMessageConsumer<ProjectOrganizationMessage>> projOrgMessageConsumers,
        IRawMessageSender                                         sender,
        IProjectMessaging                                         projectMessaging,
        IProjectLoader                                            projectLoader,
        IServiceProvider                                          serviceProvider,
        GeneralMessageHandler                                     generalMessageHandler,
        ILogger<ProjectOrganizationMessageHandler>                logger)
    : IMessageEgressHandler<ProjectOrganizationMessage>, IMessageIngressHandler
{
    public async ValueTask<MessageHandleResult> HandleIngress(MimeMessage message)
    {
        var dbctx = await dbContextFactory.CreateDbContextAsync();
        
        // TODO - it might be better to optimise this or move it to services
        var projectIds = await dbctx.Set<Project>().Select(p => p.Id).ToListAsync();
        var orgIds = await dbctx.Set<Organization>().Select(o => o.Abbreviation).ToListAsync();
        
        // Search for the project id in the subject
        var subj = message.Subject ?? "";
        var projects = projectIds.Where(p => subj.Contains(p, StringComparison.OrdinalIgnoreCase)).ToList();
        var organizations = orgIds.Where(o => subj.Contains(o, StringComparison.OrdinalIgnoreCase)).ToList();

        if (projects.Count == 0 || organizations.Count == 0) return MessageHandleResult.Ignored;
        
        // Process subject and find sender 
        var groupRegex = "\\[*[^\\]]\\]";
        var processedSubj = Regex.Replace(subj, groupRegex, "");
        // Now, get rid of junk from the subject start. (Re: etc.)
        // FROM https://stackoverflow.com/questions/9153629/regex-code-for-removing-fwd-re-etc-from-email-subject
        string subjunkReg = @"/([\[\(] *)?\b(RE?S?|FYI|RIF|I|FS|VB|RV|ENC|ODP|PD|YNT|ILT|SV|VS|VL|AW|WG|FWD?) *([-:;)\]][ :;\])-]*|$)|\]+ *$/i";
        processedSubj = Regex.Replace(processedSubj, subjunkReg, "").Trim();
        var sender = await FindMessageSender(message);
        logger.LogDebug("Sender: {} Processed subject: {}, Original subject: {}", sender?.Fullcontact, processedSubj, subj);
        if (string.IsNullOrEmpty(processedSubj) || sender is null)
        {
            // Invalid (empty) subject or anonymous sender
            
            return MessageHandleResult.Rejected;
        }
        
        
        
        // Save the message to the database - for each project organization we create separate message metadata, but link 
        // them to shared message data
        var messageData = new MessageData {Message = message};
        dbctx.Set<MessageData>().Add(messageData);
        await dbctx.SaveChangesAsync();
            
        foreach (var projectId in projects)
        {
            // Fully load the project 
            var project = await projectLoader.LoadAsync(projectId);

            if (project is null)
            {
                logger.LogDebug("Skipping project {} since it could not be found and loaded", projectId);
                continue;
            }

            // Go through project's organizations
            foreach (var organization in project.Organizations)
            {
                if (!orgIds.Contains(organization.Id)) continue;
                
                // For each organization (and project), create the message, save it and send to to workflow handlers
                var messg = new ProjectOrganizationMessage()
                {
                    Organization = organization,
                    Project = project,
                    MessageDataId = messageData.Id,
                    DtCreated = DateTime.UtcNow,
                    Subject = processedSubj,
                    MimeHeaders = message.Headers.GetAsString(),
                    Sender = sender,
                    MessageStatus = MessageStatus.Handled
                };

                // Process messages by whoever wants to 
                foreach (var projOrgMessageConsumer in projOrgMessageConsumers)
                {
                    try
                    {
                        await projOrgMessageConsumer.ConsumeMessageAsync(messg);
                        messg.MessageStatus = MessageStatus.Handled;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Unhandled exception during message consuming");
                    }
                }
                
                // Process messages by specific project
                dynamic consumers = serviceProvider.GetServices(typeof(IProjectMessageConsumer<>).MakeGenericType(project.GetType()));
                foreach (var consumer in consumers)
                {
                    await consumer.ConsumeMessageAsync(messg);
                }
                
                // Save the message to the database
                dbctx.Set<ProjectOrganizationMessage>()
                    .Add(messg);

                // Resend the message to other ppl, if desired 
                var resendRecipients = await projectMessaging.GetResendRecipientsAsync(messg);
                await Resend(message, resendRecipients);
            }

            await dbctx.SaveChangesAsync();

        }

        return MessageHandleResult.Handled;
    }

    public async ValueTask<MessageHandleResult> HandleEgress(ProjectOrganizationMessage message)
    {
        await generalMessageHandler.SendAndSaveMessage(message);
        return MessageHandleResult.Handled;
    }


    private async Task<AppUser?> FindMessageSender(MimeMessage message)
    {
        await using var dbctx = await dbContextFactory.CreateDbContextAsync();

        // Find message sender
        var senderAddr = message.Sender.Address;

        var match = await dbctx.Users.FirstOrDefaultAsync(u => u.PrimaryContact.Email == senderAddr);
        if (match is not null) return match;
        
        // TODO - handle multiple firstnames
        var splittedNames =
            message.Sender.Name.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (splittedNames.Length >= 2)
        {
            var senderFirstName = string.Join(" ", splittedNames.Take(splittedNames.Length - 1));
            var senderLastName = splittedNames.Last();
            
            // TODO - ignore punctuation?
            return await dbctx.Users.FirstOrDefaultAsync(u =>
                u.Firstname == senderFirstName && u.Lastname == senderLastName);
        }
        
        return null;
    }


    public async Task Resend(MimeMessage message, IEnumerable<MessageRecipient> targets)
    {
        MailboxAddress MboxSelector(MessageRecipient mr)
        {
            return new MailboxAddress(mr.User.Fullname, mr.User.PrimaryContact.Email);
        }

        var targetList = targets.ToList();
        if (targetList.Count == 0) return;
        
        await sender.ResendMessage(
            message,
            targetList.Where(t => t.Type is MessageRecipientType.Primary).Select(MboxSelector),
            targetList.Where(t => t.Type is MessageRecipientType.Copy).Select(MboxSelector),
            targetList.Where(t => t.Type is MessageRecipientType.BlindCopy).Select(MboxSelector)
        );
    }
}