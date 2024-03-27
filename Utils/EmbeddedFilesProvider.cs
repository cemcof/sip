using sip.Core;

namespace sip.Utils;

public interface IEmbeddedFilesProvider
{
    Task<string> ReadAsTextAsync(string target);
}

public class EmbeddedFilesProvider(IOptions<AppOptions> options) : IEmbeddedFilesProvider
{
    private Stream GetFileStream(string target)
    {
        var assembliesToTry = new[] {Assembly.GetExecutingAssembly(), options.Value.HostAppAssembly};
        foreach (var assembly in assembliesToTry)
        {
            string? resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(str => str.EndsWith(target));
            
            if (resourceName is null) continue;

            return assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException();
        }

        throw new ArgumentException($"Not found embedded file for target {target}");
    }
    
    public async Task<string> ReadAsTextAsync(string target)
    {
        await using var strm = GetFileStream(target);
        using var sr = new StreamReader(strm);

        return await sr.ReadToEndAsync();
    }
}