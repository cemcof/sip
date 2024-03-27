using Microsoft.Extensions.DependencyInjection.Extensions;

namespace sip.Autoloaders;

public static class AutoloadersExtensions
{
    public static void AddAutoloaders(this IServiceCollection services, IConfigurationRoot config)
    {
        services.TryAddSingleton<AutoloadersService>();

        services.AddOptions<AutoloadersOptions>()
            .GetOrganizationOptionsBuilder()
            .BindOrganizationConfiguration(config, "Autoloaders");
    }
}