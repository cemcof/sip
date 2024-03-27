using sip.Documents;

namespace sip.Messaging;

public class ProjectOrganizationMessageBuilderProvider(
        IServiceProvider                                   serviceProvider,
        DocumentService                                    documentService,
        ILogger<ProjectOrganizationMessageBuilderProvider> logger,
        IMessageSender                                     sender)
    : IMessageBuilderProvider<ProjectOrganizationMessageBuilder>
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public ProjectOrganizationMessageBuilder CreateBuilder()
    {
        // Create scope
        return new ProjectOrganizationMessageBuilder(logger, documentService, sender);
    }
}