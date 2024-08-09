namespace sip.Messaging;

public class GeneralMessageBuilderProvider(
        IServiceProvider                           serviceProvider,
        DocumentService                            documentService,
        ILogger<ProjectOrganizationMessageBuilder> logger,
        IMessageSender                             sender)
    : IMessageBuilderProvider<GeneralMessageBuilder>
{
    public GeneralMessageBuilder CreateBuilder()
    {
        // Create scope
        return new GeneralMessageBuilder(logger, documentService, sender);
    }
}