using System.Text.RegularExpressions;

namespace sip.Messaging;

public class GeneralMessageBuilder<TMessage> where TMessage : Message, new()
{
    protected readonly TMessage Message = new();
    protected MimeMessage MimeMessage => Message.MessageData.Message;
    protected readonly ILogger Logger;
    protected readonly DocumentService DocumentService;
    protected readonly IMessageSender MessageSender;

    protected BodyBuilder BodyBuilder = new();
    protected List<Func<Task>> BuilderHandlers = new();

    public GeneralMessageBuilder(ILogger logger, DocumentService documentService, IMessageSender messageSender)
    {
        Logger = logger;
        DocumentService = documentService;
        MessageSender = messageSender;

        Message.MessageData = new MessageData {Message = new MimeMessage()};
        
        // Set default body setter to empty text body
        BodyBuilder.TextBody = "";
        Subject("");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GeneralMessageBuilder<TMessage> BodyAndSubjectFromFileTemplate(object data, string path)
    {
        BuilderHandlers.Add(() => BodyAndSubjectFromHbsFileTemplateAsync(data, path));
        return this;
    }
    
    public async Task BodyAndSubjectFromHbsFileTemplateAsync(object data, string resourceTarget) 
    {
        try
        {
            var renderer = DocumentService.HandlebarsService;
            var text = await DocumentService.ReadEmbeddedFileAsync(resourceTarget);
            text = text.Trim();
            var lines = text.Split("\n");
            
            // take first line of text and see if it starts with Subject: , if so, extract the subject, use regex
            var subjectRegex = new Regex(@"subject:\s*(.*)", RegexOptions.IgnoreCase);
            var subjectMatch = subjectRegex.Match(lines.First());
            if (subjectMatch.Success)
            {
                var subject = subjectMatch.Groups[1].Value;
                SubjectFromHbsStringTemplate(data, subject);
                text = string.Join("\n", lines.Skip(1));
            }
            
            var rendered = renderer.Render(text, data);
            BodyBuilder.HtmlBody = rendered;
            BodyBuilder.TextBody = rendered.HtmlToText();
            
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"Error during {nameof(BodyAndSubjectFromHbsFileTemplateAsync)}");
            throw;
        }
    }
    
    public void BodyFromHbsStringTemplate(object context, string mailTemplateBody)
    {
        var body = DocumentService.HandlebarsService.Render(mailTemplateBody, context);

        BodyBuilder.HtmlBody = body;
        BodyBuilder.TextBody = body.HtmlToText();
    }

    public virtual async Task<TMessage> BuildAsync()
    {
        foreach (var bodyBuilderDelegate in BuilderHandlers)
        {
            await bodyBuilderDelegate();
        }

        MimeMessage.Body = BodyBuilder.ToMessageBody();
        
        return Message;
    }
    
    public async Task BuildAndSendAsync()
    {
        Logger.LogDebug("Building message, subject is {} and mime subject is {}", Message.Subject, MimeMessage.Subject);
        var mess = await BuildAsync();
        Logger.LogDebug("Message built, subject is {} and mime subject is {}", Message.Subject, MimeMessage.Subject);
        await MessageSender.SendMessage(mess);
    }
    
    public void Subject(string s)
    {
        Message.Subject = s;
        MimeMessage.Subject = s;
    }
    
    public void SubjectFromHbsStringTemplate(object context, string subjectTemplate)
    {
        var subject = DocumentService.HandlebarsService.Render(subjectTemplate, context);
        Subject(subject);
    }

    public GeneralMessageBuilder<TMessage> AddRecipient(AppUser user, MessageRecipientType recipientType = MessageRecipientType.Primary)
    {
        try
        {
            var contactAddress = user.Fullcontact;
            var mailboxAddress = MailboxAddress.Parse(contactAddress);
            AddRecipient(mailboxAddress, recipientType);
            Message.Recipients.Add(new MessageRecipient() { Type = recipientType, UserId = user.Id });
        }
        catch (Exception)
        {
            Logger.LogWarning("Failed to parse contact address for user {UserId}, skipping this recipient", user.Id);
        }
        
        return this;
    }

    public void AddRecipient(MailboxAddress mailbox, MessageRecipientType recipientType = MessageRecipientType.Primary)
    {
        var coll = recipientType switch
        {
            MessageRecipientType.Primary => MimeMessage.To,
            MessageRecipientType.Copy => MimeMessage.Cc,
            MessageRecipientType.BlindCopy => MimeMessage.Bcc,
            _ => throw new ArgumentOutOfRangeException(nameof(recipientType), recipientType, null)
        };

        coll.Add(mailbox);
    }
    
    public void AddAttachment(FileMetadata fileMetadata)
    {
        BodyBuilder.Attachments.Add(fileMetadata.FileNameNormalized, fileMetadata.FileData!.Data);
    }
}

public class GeneralMessageBuilder(ILogger logger, DocumentService documentService, IMessageSender messageSender)
    : GeneralMessageBuilder<GeneralMessage>(logger, documentService, messageSender);