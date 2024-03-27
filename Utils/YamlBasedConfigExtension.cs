
namespace sip.Utils;

public static class YamlBasedConfigExtension
{
    /// <summary>
    /// Configures app's configuration sources, so they are:
    /// - appsettings YAML file selected by environment (e.g. appsettings.Development.yml), this file is compulsory!
    /// - Environment variables
    /// - Command line arguments
    /// </summary>
    public static void ConfigDefaultSourcesPlusYaml(this WebApplicationBuilder builder, string[] args)
    {
        // NOTE: ConfigureAppConfiguration runs the action immediatelly
        builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.Sources.Clear();
            // One configuration yaml file selected according to current environment
            config.AddYamlFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.yml", false, true);
            config.AddEnvironmentVariables("ASPNETCORE_");
            config.AddCommandLine(args);
        });

    }
}