namespace sip.Environment;

public static class EnvironmentExtensions
{
    public static void AddEnvironment(this IServiceCollection services, IConfigurationRoot conf)
    {
        services.AddSingleton<EnvironmentService>();
        // TODO - fix papouch
    }
}