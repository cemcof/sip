using Scrutor;

namespace sip.Utils;

public static class AssemblyUtils
{
    public static Type? FindImplementation(this Assembly ass, Type interfaceType, bool includeSelf = true)
        => ass.FindImplementations(interfaceType, includeSelf).FirstOrDefault();
    
    public static Type? FindImplementation<TInterface>(this Assembly ass, bool includeSelf = true)
        => FindImplementation(ass, typeof(TInterface), includeSelf);
    
    public static IEnumerable<Type> FindImplementations(this Assembly ass, Type interfaceType, bool includeSelf = true)
    {
        var qr = ass.GetTypes().Where(t => t.IsAssignableTo(interfaceType) && t.IsClass && !t.IsAbstract);
        return (includeSelf) ? qr : qr.Where(t => t != interfaceType);
    }

    public static IEnumerable<Type> FindImplementations<TType>(this Assembly ass, bool includeSelf = true)
        => ass.FindImplementations(typeof(TType), includeSelf);

    public static void RegisterServicesAsSingletons(this Assembly ass,
        IServiceCollection serviceCollection)
    {
        serviceCollection.Scan(s =>
        {
            s.FromAssemblies(ass)
                .AddClasses(c => c.Where(cl => cl.Name.EndsWith("Service") 
                                               || cl.Name.EndsWith("Handler") 
                                               || cl.Name.EndsWith("Provider")
                                               ))
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsSelfWithInterfaces()
                .WithSingletonLifetime();
        });
    }
}