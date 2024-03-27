namespace sip.Utils;


// This interface is just workaround for covariance on TOptions
public interface INameAndOpt<out TOptions>
{
    string Name { get;  }
    TOptions Instance { get; }
}

public class NamdAndOpt<TOptions>(string name, TOptions instance) : INameAndOpt<TOptions>
{
    public string   Name     { get; } = name;
    public TOptions Instance { get; } = instance;
}


public interface IOptionListProvider<out TOptions>
{
    IEnumerable<TOptions> GetAll();
    IEnumerable<INameAndOpt<TOptions>> GetAllWithNames();
}

public class OptionListProvider<TOptions> : IOptionListProvider<TOptions> 
    where TOptions : class
{
    private readonly IOptionsMonitor<TOptions> _optionsMonitor;
    private readonly string[] _allOptionsNames;

    public OptionListProvider(
        // We require all configurations from options
        // From them, we can determine all configured option names
        IEnumerable<IConfigureOptions<TOptions>> allOptionsSetups,
        IOptionsMonitor<TOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
        // Determine all option names and save them for our GetAll method
        _allOptionsNames = allOptionsSetups
            .OfType<ConfigureNamedOptions<TOptions>>()
            .Where(o => !string.IsNullOrEmpty(o.Name))
            .Select(o => o.Name!)
            .Distinct()
            .ToArray();
    }

    public IEnumerable<TOptions> GetAll()
    {
        return _allOptionsNames.Select(_optionsMonitor.Get);
    }

    public IEnumerable<INameAndOpt<TOptions>> GetAllWithNames()
    {
        return _allOptionsNames.Select(on => new NamdAndOpt<TOptions>(on, _optionsMonitor.Get(on)));
    }
}


public class NotAvailableException : Exception
{
    public NotAvailableException()
    {
        
    }
    
    public NotAvailableException(string message)
    : base(message)
    {
        
    }
}


public interface INamedSetup<in TOptions> : IStringName
{
    void Setup(TOptions opts);
}

public static class OptionsUtils
{
    public static void ConfigureFromNamedSetup<TOptions, TNamedSetup>(this IServiceCollection serviceCollection)
        where TNamedSetup : INamedSetup<TOptions>
        where TOptions : class
    {
        var instance = Activator.CreateInstance<TNamedSetup>();
        serviceCollection.Configure<TOptions>(instance.Name, instance.Setup);
    }    
    
}
