using sip.Core;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace sip.LabIssues;

public static class IssuesExtensions
{
    public static void AddIssues(this IServiceCollection services, IConfigurationRoot conf)
    {
        services.TryAddSingleton<IssuesService>();
        
        // DB configure
        services.ConfigureDbModel(mb =>
        {
            mb.Entity<Issue>()
                .HasOne(i => i.Responsible)
                .WithMany()
                .HasForeignKey(i => i.ResponsibleId);
            
            mb.Entity<Issue>()
                .HasOne(i => i.InitiatedBy)
                .WithMany()
                .HasForeignKey(i => i.InitiatedById);

            mb.Entity<Issue>()
                .Property(i => i.Urgency)
                .HasConversion(new EnumToStringConverter<IssueUrgency>());
            
            mb.Entity<Issue>()
                .Property(i => i.Status)
                .HasConversion(new EnumToStringConverter<IssueStatus>());
        });
    }
}