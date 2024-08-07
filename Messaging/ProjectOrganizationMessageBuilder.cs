namespace sip.Messaging;

public class ProjectOrganizationMessageBuilder(
        ILogger         logger,
        DocumentService documentService,
        IMessageSender  messageSender)
    : GeneralMessageBuilder<ProjectOrganizationMessage>(logger, documentService, messageSender);