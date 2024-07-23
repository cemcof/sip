using Microsoft.Extensions.FileProviders;

namespace sip.Utils;

public static class EmbeddedFilesExtensions
{
    public static async Task<string> ReadAsTextAsync(this EmbeddedFileProvider embeddedFileProvider, string target, bool isNameEndEnough = true)
    {
        var finfo = embeddedFileProvider.GetFileInfo(target);

        if (!finfo.Exists && isNameEndEnough)
        {
            // Search for it using endswith 
            finfo = embeddedFileProvider.GetDirectoryContents(string.Empty)
                .FirstOrDefault(f => f.Name.EndsWith(target), new NotFoundFileInfo(target));
        }
        
        if (!finfo.Exists) 
            throw new ArgumentException($"Not found embedded file for target {target}");
        
        using var sr = new StreamReader(finfo.CreateReadStream());
        var content = await sr.ReadToEndAsync();
        return content;
    }
}