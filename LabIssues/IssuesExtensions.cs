using sip.Core;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace sip.LabIssues;

public static class IssuesExtensions
{
    public static void AddIssues(this IServiceCollection services, IConfigurationRoot conf)
    {
        services.TryAddSingleton<IssuesService>();
    }
}