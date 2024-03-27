using sip.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using sip.Messaging.Email;
using sip.Projects;
using sip.Scheduling;

namespace sip.Messaging;


public class MessagingBuilder
{
    private readonly IServiceCollection _services;

    public MessagingBuilder(IServiceCollection services)
    {
        _services = services;

        _services.ConfigureDbModel(m =>
        {
            new MessageBaseEntityConfig().Configure(m.Entity<Message>());
            new MessageRecipientEntityConfig().Configure(m.Entity<MessageRecipient>());
            new MessageDataEntityConfiguration().Configure(m.Entity<MessageData>());
        });
        
        // Add default sender
        _services.TryAddSingleton<MessageSender>();
        _services.TryAddSingleton<IMessageSender, MessageSender>();
        
        // Add default receiver
        _services.TryAddSingleton<MessageReceiver>();
        _services.TryAddSingleton<IRawMessageReceiver, MessageReceiver>();

    }
    
    public MessagingBuilder AddGeneralMessaging()
    {
        // Add general message handler and builder
        _services.TryAddSingleton<GeneralMessageHandler>();
        _services.AddSingleton<IMessageEgressHandler<GeneralMessage>, GeneralMessageHandler>();
        _services.TryAddSingleton<GeneralMessageBuilderProvider>();
        _services.TryAddSingleton<IMessageBuilderProvider<GeneralMessageBuilder>, GeneralMessageBuilderProvider>();
        return this;
    }



    public MessagingBuilder AddProjectMessaging()
    {
        // Add general message handler and builder
        _services.TryAddSingleton<ProjectOrganizationMessageHandler>();
        _services.TryAddSingleton<IMessageEgressHandler<ProjectOrganizationMessage>, ProjectOrganizationMessageHandler>();
        _services.TryAddEnumerable(new ServiceDescriptor(typeof(IMessageIngressHandler), typeof(ProjectOrganizationMessageHandler), ServiceLifetime.Singleton));
        _services.TryAddSingleton<ProjectOrganizationMessageBuilderProvider>();
        _services.TryAddSingleton<IMessageBuilderProvider<ProjectOrganizationMessageBuilder>, ProjectOrganizationMessageBuilderProvider>();
        _services.TryAddSingleton<IProjectMessaging, ProjectMessagingDelegatorService>();
        return this;
    }

    
    public MessagingBuilder AddOutEmailMessaging(IConfigurationSection smtpConfig)
    {
        _services.AddOptions<SmtpOptions>()
            .Bind(smtpConfig)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        _services.Configure<SmtpOptions>(smtpConfig);

        _services.AddSingleton<SmtpSender>();
        _services.AddSingleton<IRawMessageSender>(s => s.GetRequiredService<SmtpSender>());
        return this;
    }

    public MessagingBuilder AddInEmailMessaging(IConfigurationSection imapConfig)
    {
        _services.AddOptions<ImapOptions>()
            .Bind(imapConfig)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var schedopts = new ScheduledServiceOptions();
        imapConfig.Bind(schedopts);
        
        // TODO - refactor scheduled service configuration
        _services.AddScheduledService<ImapReceiver>(o =>
        {
            o.Cron = schedopts.Cron;
            o.Enabled = schedopts.Enabled;
            o.SkipRoundIfRunning = schedopts.SkipRoundIfRunning;
        });

        return this;
    }
    
}

public static  class MessagingExtensions;